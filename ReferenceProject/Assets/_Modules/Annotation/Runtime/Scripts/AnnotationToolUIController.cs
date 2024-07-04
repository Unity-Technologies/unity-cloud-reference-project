using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Unity.Cloud.Annotation;
using Unity.Cloud.Annotation.Runtime;
using Unity.Cloud.Assets;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.InputSystem;
using UnityEngine.UIElements;
using Zenject;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.Annotation
{
    public class AnnotationToolUIController : ToolUIController
    {
        static readonly string k_MouseSelectActionKey = "<Mouse>/leftButton";
        static readonly string k_TouchSelectActionKey = "<Touchscreen>/primaryTouch/tap";
        static readonly string k_MouseSelectAction = "ClickAction";
        static readonly string k_TouchSelectAction = "TouchAction";

        [SerializeField]
        TopicPanelController m_TopicPanel;

        [SerializeField]
        CommentPanelController m_CommentPanel;

        [SerializeField]
        LayerMask m_IndicatorsLayerMask;

        ITopic m_EditedTopic;
        AnnotationIndicatorController m_WorkingIndicator;
        ITopic m_CurrentTopic;

        IAnnotationController m_Controller;
        IAnnotationIndicatorManager m_IndicatorManager;
        IInputManager m_InputManager;
        ICameraProvider m_CameraProvider;
        INavigationManager m_NavigationManager;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;
        IAssetEvents m_AssetEvents;

        InputScheme m_InputScheme;
        protected Ray m_SelectionRay;
        protected bool m_SelectionStarted;

        [Inject]
        void Setup(
            IAnnotationController annotationController,
            IAnnotationIndicatorManager indicatorManager,
            IInputManager inputManager,
            PropertyValue<IObjectSelectionInfo> objectSelectionProperty,
            ObjectSelectionActivator objectSelectionActivator,
            ICameraProvider cameraProvider,
            INavigationManager navigationManager,
            IAssetEvents assetEvents,
            IOrganizationRepository organizationRepository)
        {
            m_Controller = annotationController;
            m_IndicatorManager = indicatorManager;
            m_ObjectSelectionProperty = objectSelectionProperty;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_CameraProvider = cameraProvider;
            m_NavigationManager = navigationManager;
            m_InputManager = inputManager;
            m_AssetEvents = assetEvents;
        }

        protected override void Awake()
        {
            base.Awake();

            SetupInputs();

            m_AssetEvents.AssetLoaded += OnAssetLoaded;
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;

            m_Controller.Initialized += OnInitialized;
            m_Controller.TopicCreatedOrUpdated += OnTopicCreatedOrUpdated;
            m_Controller.TopicRemoved += OnTopicRemoved;

            m_WorkingIndicator = m_IndicatorManager.GetEmptyIndicator();
            m_WorkingIndicator.SetSelected(true);
            m_WorkingIndicator.gameObject.SetActive(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_Controller.Initialized -= OnInitialized;
            m_Controller.TopicCreatedOrUpdated -= OnTopicCreatedOrUpdated;
            m_Controller.TopicRemoved -= OnTopicRemoved;
            m_Controller.Shutdown();

            m_InputScheme?.Dispose();
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            Initialize(rootVisualElement);

            return rootVisualElement;
        }

        public override void OnToolOpened()
        {
            m_TopicPanel.Show();
            m_CommentPanel.Hide();

            m_IndicatorManager.SetIndicatorsVisible();
            EnableObjectSelection();
            m_InputScheme?.SetEnable(true);

            m_TopicPanel.TopicSelected += OnTopicSelected;
            m_TopicPanel.TopicDeleted += OnTopicDeleted;
            m_TopicPanel.TopicEdited += OnTopicEdited;
            m_TopicPanel.TopicSubmitEdited += OnSubmitEditTopic;
            m_TopicPanel.TopicSubmitted += OnSubmitAddTopic;
            m_TopicPanel.CancelClicked += OnCancelClicked;
            m_TopicPanel.ReplySelected += OnReplySelected;

            m_CommentPanel.GotoClicked += OnGotoClicked;
            m_CommentPanel.BackClicked += OnBackClicked;
            m_CommentPanel.CommentDeleted += OnDeleteComment;
            m_CommentPanel.CommentEdited += OnSubmitEditComment;
            m_CommentPanel.CommentSubmitted += OnSubmitAddComment;
        }

        public override void OnToolClosed()
        {
            CancelInput();

            if (m_CurrentTopic != null)
            {
                SelectTopic(null);
            }

            m_IndicatorManager.SetIndicatorsVisible(visible: false);
            DisableObjectSelection();
            m_InputScheme?.SetEnable(false);

            m_TopicPanel.TopicSelected -= OnTopicSelected;
            m_TopicPanel.TopicDeleted -= OnTopicDeleted;
            m_TopicPanel.TopicEdited -= OnTopicEdited;
            m_TopicPanel.TopicSubmitEdited -= OnSubmitEditTopic;
            m_TopicPanel.TopicSubmitted -= OnSubmitAddTopic;
            m_TopicPanel.CancelClicked -= OnCancelClicked;
            m_TopicPanel.ReplySelected -= OnReplySelected;

            m_CommentPanel.GotoClicked -= OnGotoClicked;
            m_CommentPanel.BackClicked -= OnBackClicked;
            m_CommentPanel.CommentDeleted -= OnDeleteComment;
            m_CommentPanel.CommentEdited -= OnSubmitEditComment;
            m_CommentPanel.CommentSubmitted -= OnSubmitAddComment;
        }

        void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            m_Controller.Initialize(dataset.Descriptor);
            RefreshPermission();
        }

        void OnAssetUnloaded()
        {
            m_Controller.Shutdown();
        }

        protected void SetupInputs()
        {
            InputAction clickAction = new InputAction(k_MouseSelectAction, InputActionType.Button, k_MouseSelectActionKey, "");
            InputAction touchAction = new InputAction(k_TouchSelectAction, InputActionType.Button, k_TouchSelectActionKey, "");

            m_InputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.Annotation, InputSchemeCategory.Tools, new InputAction[] { clickAction, touchAction });

            m_InputScheme[k_MouseSelectAction].OnStarted += AnnotationSelectionStarted;
            m_InputScheme[k_MouseSelectAction].OnCanceled += AnnotationSelectionCanceled;
            m_InputScheme[k_MouseSelectAction].OnPerformed += PerformSelection;
            m_InputScheme[k_MouseSelectAction].IsUIPointerCheckEnabled = true;

            m_InputScheme[k_TouchSelectAction].OnStarted += AnnotationSelectionStarted;
            m_InputScheme[k_TouchSelectAction].OnCanceled += AnnotationSelectionCanceled;
            m_InputScheme[k_TouchSelectAction].OnPerformed += PerformSelection;
            m_InputScheme[k_TouchSelectAction].IsUIPointerCheckEnabled = true;
        }

        void EnableObjectSelection()
        {
            if (m_Controller.CheckPermissions(AnnotationsPermission.Create))
            {
                // Activate Selection tool
                if (m_ObjectSelectionActivator != null)
                {
                    m_ObjectSelectionActivator.Subscribe(this);
                }
                else
                {
                    Debug.LogError($"Null reference to {nameof(ObjectSelectionActivator)} on {nameof(AnnotationToolUIController)}");
                }

                if (m_ObjectSelectionProperty != null)
                {
                    m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
                    m_ObjectSelectionProperty.ValueChanged += OnObjectSelectionChanged;
                }
            }
        }

        void DisableObjectSelection()
        {
            if (m_Controller.CheckPermissions(AnnotationsPermission.Create))
            {
                if (m_ObjectSelectionProperty != null)
                {
                    m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
                }

                // Disable selection tool
                m_ObjectSelectionActivator?.Unsubscribe(this);

                m_WorkingIndicator.gameObject.SetActive(false);
            }
        }

        void OnInitialized(IEnumerable<ITopic> topics)
        {
            var topicList = topics.ToList();
            _ = m_TopicPanel.RefreshTopics(topicList);
            m_IndicatorManager.SetIndicators(topicList);
        }

        void OnTopicCreatedOrUpdated(ITopic topic)
        {
            if (m_TopicPanel.IsTopicExist(topic))
            {
                var indicator = m_IndicatorManager.GetIndicator(topic.Id);
                indicator.Topic = topic;
                m_TopicPanel.UpdateTopicEntry(topic);
            }
            else
            {
                m_IndicatorManager.AddIndicator(topic);
                _ = m_TopicPanel.AddTopicEntry(topic);
            }
        }

        void OnTopicRemoved(TopicId topicId)
        {
            if (m_CurrentTopic != null && m_CurrentTopic.Id == topicId)
            {
                // Opened topic was deleted, return to topic list
                BackToTopicList();
            }

            m_IndicatorManager.RemoveIndicator(topicId);
            m_TopicPanel.RemoveTopicEntry(topicId);
        }

        void Initialize(VisualElement rootVisualElement)
        {
            m_TopicPanel.Initialize(rootVisualElement);
            m_CommentPanel.Initialize(rootVisualElement);
        }

        void OnCancelClicked()
        {
            CancelInput();
        }

        void CancelInput()
        {
            m_WorkingIndicator.gameObject.SetActive(false);
            ClearEditTopic();
        }

        void ClearEditTopic()
        {
            if (m_EditedTopic == null)
                return;

            var indicator = m_IndicatorManager.GetIndicator(m_EditedTopic.Id);
            if (indicator != null)
            {
                indicator.gameObject.SetActive(true);
                indicator.transform.position = m_WorkingIndicator.transform.position;
                m_WorkingIndicator.gameObject.SetActive(false);
            }

            m_EditedTopic = null;
        }

        void OnTopicSelected(ITopic topic)
        {
            GotoTopic(topic);
            SelectTopic(topic);
        }

        void SelectTopic(ITopic topic)
        {
            if (m_CommentPanel.IsOpen())
            {
                BackToTopicList();
            }
            else
            {
                CancelInput();
                m_TopicPanel.ResetTextInput();
            }

            if (m_CurrentTopic != null)
            {
                m_IndicatorManager.GetIndicator(m_CurrentTopic.Id)?.SetSelected(false);
            }

            m_CurrentTopic = topic;
            if (topic != null)
            {
                m_IndicatorManager.GetIndicator(topic.Id)?.SetSelected(true);
            }

            m_TopicPanel.SelectTopicEntry(topic);
        }

        async Task ShowComments(IReadOnlyCollection<IComment> comments)
        {
            m_CommentPanel.Show();
            m_TopicPanel.Hide();

            DisableObjectSelection();

            m_CurrentTopic.CommentCreated += UpdateCommentPanel;
            m_CurrentTopic.CommentRemoved += UpdateCommentPanel;
            m_CurrentTopic.CommentUpdated += UpdateCommentPanel;

            await m_CommentPanel.ShowComments(m_CurrentTopic, comments);
        }

        void OnBackClicked()
        {
            BackToTopicList();
        }

        void BackToTopicList()
        {
            m_CurrentTopic.CommentCreated -= UpdateCommentPanel;
            m_CurrentTopic.CommentRemoved -= UpdateCommentPanel;
            m_CurrentTopic.CommentUpdated -= UpdateCommentPanel;

            m_TopicPanel.Show();
            m_CommentPanel.Hide();
            EnableObjectSelection();

            // Wait next frame before scrolling to the selected topic because ScrollView doesn't update its content immediately
            StartCoroutine(Common.Utils.WaitAFrame(() =>
            {
                m_TopicPanel.SelectTopicEntry(m_CurrentTopic);
            }));
        }

        void UpdateCommentPanel(IComment comment)
        {
            UpdateCommentPanelTask().ConfigureAwait(false);
        }

        async Task UpdateCommentPanelTask()
        {
            var comments = await m_CurrentTopic.GetCommentsAsync();
            await ShowComments(comments.ToList());
        }

        void AddTopic()
        {
            m_TopicPanel.AddTopic();
        }

        void OnTopicEdited(ITopic topic)
        {
            SelectTopic(topic);

            m_Controller.GetTopic(topic.Id, (ITopic t) =>
            {
                m_EditedTopic = t;
                GotoTopic(t);
            });
        }

        void OnTopicDeleted(ITopic topic)
        {
            m_Controller.DeleteTopic(topic);
        }

        void OnDeleteComment(IComment comment)
        {
            m_Controller.DeleteComment(m_CurrentTopic, comment);
        }

        void OnSubmitAddTopic(string title, string description)
        {
            m_WorkingIndicator.gameObject.SetActive(false);

            var cameraTransform = m_CameraProvider.Camera.transform;
            m_Controller.CreateTopic(new AnnotationData
            {
                Title = title,
                Description = description,
                CameraPosition = cameraTransform.position,
                CameraRotation = cameraTransform.rotation,
                IndicatorPosition = m_WorkingIndicator.transform.position
            });
        }

        void OnSubmitEditTopic(string title, string description)
        {
            m_Controller.GetTopic(m_EditedTopic.Id, (ITopic t) =>
            {
                var cameraTransform = m_CameraProvider.Camera.transform;
                m_Controller.UpdateTopic(t, new AnnotationData
                {
                    Title = title,
                    Description = description,
                    CameraPosition = cameraTransform.position,
                    CameraRotation = cameraTransform.rotation,
                    IndicatorPosition = m_WorkingIndicator.transform.position
                });
            });

            ClearEditTopic();
        }

        async void OnReplySelected(ITopic topic)
        {
            if (m_CurrentTopic != topic)
            {
                SelectTopic(topic);
            }

            var comments = await topic.GetCommentsAsync();
            await ShowComments(comments.ToList());
        }

        void OnSubmitAddComment(string text)
        {
            m_Controller.CreateComment(m_CurrentTopic, text);
        }

        void OnSubmitEditComment(IComment comment, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                OnDeleteComment(comment);
            }
            else if (text != comment.Text)
            {
                m_Controller.UpdateComment(m_CurrentTopic, comment, text);
            }
        }

        void AnnotationSelectionStarted(InputAction.CallbackContext _)
        {
            m_SelectionRay = m_CameraProvider.Camera.ScreenPointToRay(Pointer.current.position.ReadValue());
            m_SelectionStarted = true;
        }

        protected void AnnotationSelectionCanceled(InputAction.CallbackContext _)
        {
            m_SelectionStarted = false;
        }

        protected void PerformSelection(InputAction.CallbackContext context)
        {
            if (!m_SelectionStarted)
                return;

            m_SelectionStarted = false;

            FindIndicator(m_SelectionRay);
        }

        void FindIndicator(Ray ray)
        {
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, m_CameraProvider.Camera.farClipPlane, m_IndicatorsLayerMask))
            {
                var indicator = hit.collider.GetComponentInParent<AnnotationIndicatorController>();
                if (indicator != null && indicator.Topic != null)
                {
                    SelectIndicator(indicator);
                }
            }
        }

        void SelectIndicator(AnnotationIndicatorController indicator)
        {
            if (m_CurrentTopic != null && m_CurrentTopic.Id == indicator.Topic.Id)
            {
                m_TopicPanel.Show();
                m_CommentPanel.Hide();
                EnableObjectSelection();
                SelectTopic(null);
                return;
            }

            SelectTopic(indicator.Topic);
        }

        void OnObjectSelectionChanged(IObjectSelectionInfo objectSelectionInfo)
        {
            SelectTopic(null);

            if (!objectSelectionInfo.HasIntersected)
                return;

            m_WorkingIndicator.gameObject.SetActive(true);
            m_WorkingIndicator.transform.position = objectSelectionInfo.SelectedPosition;

            if (m_EditedTopic == null)
            {
                AddTopic();
            }
        }

        void OnGotoClicked()
        {
            GotoTopic(m_CurrentTopic);
        }

        void GotoTopic(ITopic topic)
        {
            m_NavigationManager.TryTeleport(topic.WorldCameraTransform.Position.ToVector3(),
                topic.WorldCameraTransform.Rotation.ToQuaternion().eulerAngles);
        }

        void RefreshPermission()
        {
            if (InitialToolState == ToolState.Active && !m_Controller.CheckPermissions(AnnotationsPermission.Read))
            {
                SetToolState(ToolState.Inactive);
            }

            m_TopicPanel.SetPermissions(
                m_Controller.CheckPermissions(AnnotationsPermission.Edit),
                m_Controller.CheckPermissions(AnnotationsPermission.Delete));

            m_CommentPanel.SetPermissions(
                m_Controller.CheckPermissions(AnnotationsPermission.CreateComment),
                m_Controller.CheckPermissions(AnnotationsPermission.EditComment),
                m_Controller.CheckPermissions(AnnotationsPermission.DeleteComment));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Unity.Cloud.Annotation;
using Unity.Cloud.Annotation.Runtime;
using Unity.Cloud.Common;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.UIInputBlocker;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Annotation
{
    public class AnnotationToolUIController : ToolUIController
    {
        [SerializeField]
        TopicPanelController m_TopicPanel;

        [SerializeField]
        CommentPanelController m_CommentPanel;

        [SerializeField]
        AnnotationTextInputController m_TextInput;

        [SerializeField]
        LayerMask m_IndicatorsLayerMask;

        ITopic m_EditedTopic;
        IComment m_EditedComment;
        AnnotationIndicatorController m_WorkingIndicator;
        ITopic m_CurrentTopic;

        IAnnotationController m_Controller;
        IAnnotationIndicatorManager m_IndicatorManager;
        IUIInputBlockerEvents m_InputBlockerEvents;
        Camera m_Camera;
        INavigationManager m_NavigationManager;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        PropertyValue<IObjectSelectionInfo> m_ObjectSelectionProperty;

        [Inject]
        void Setup(
            IAnnotationController annotationController,
            IAnnotationIndicatorManager indicatorManager,
            IUIInputBlockerEvents inputBlockerEvents,
            PropertyValue<IObjectSelectionInfo> objectSelectionProperty,
            ObjectSelectionActivator objectSelectionActivator,
            Camera cam,
            INavigationManager navigationManager)
        {
            m_Controller = annotationController;
            m_IndicatorManager = indicatorManager;
            m_InputBlockerEvents = inputBlockerEvents;
            m_ObjectSelectionProperty = objectSelectionProperty;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_Camera = cam;
            m_NavigationManager = navigationManager;
        }

        protected override void Awake()
        {
            base.Awake();

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
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var rootVisualElement = base.CreateVisualTree(template);
            Initialize(rootVisualElement);
            m_Controller.Initialize();

            return rootVisualElement;
        }

        protected override void RegisterCallbacks(VisualElement visualElement)
        {
            var topicContainer = visualElement.Q("TopicListContainer");
            topicContainer.RegisterCallback<FocusInEvent>(OnFocusIn);
            topicContainer.RegisterCallback<FocusOutEvent>(OnFocusOut);
            topicContainer.RegisterCallback<PointerEnterEvent>(OnPointerEntered);
            topicContainer.RegisterCallback<PointerLeaveEvent>(OnPointerExited);

            var commentContainer = visualElement.Q("CommentListContainer");
            commentContainer.RegisterCallback<FocusInEvent>(OnFocusIn);
            commentContainer.RegisterCallback<FocusOutEvent>(OnFocusOut);
            commentContainer.RegisterCallback<PointerEnterEvent>(OnPointerEntered);
            commentContainer.RegisterCallback<PointerLeaveEvent>(OnPointerExited);

            var title = visualElement.Q("TextInputTitle");
            title.RegisterCallback<FocusInEvent>(OnFocusIn);
            title.RegisterCallback<FocusOutEvent>(OnFocusOut);

            var message = visualElement.Q("TextInputMessage");
            message.RegisterCallback<FocusInEvent>(OnFocusIn);
            message.RegisterCallback<FocusOutEvent>(OnFocusOut);
        }

        protected override void UnregisterCallbacks(VisualElement visualElement)
        {
            var topicContainer = visualElement.Q("TopicListContainer");
            topicContainer.UnregisterCallback<FocusInEvent>(OnFocusIn);
            topicContainer.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            topicContainer.UnregisterCallback<PointerEnterEvent>(OnPointerEntered);
            topicContainer.UnregisterCallback<PointerLeaveEvent>(OnPointerExited);

            var commentContainer = visualElement.Q("CommentListContainer");
            commentContainer.UnregisterCallback<FocusInEvent>(OnFocusIn);
            commentContainer.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            commentContainer.UnregisterCallback<PointerEnterEvent>(OnPointerEntered);
            commentContainer.UnregisterCallback<PointerLeaveEvent>(OnPointerExited);

            var title = visualElement.Q("TextInputTitle");
            title.UnregisterCallback<FocusInEvent>(OnFocusIn);
            title.UnregisterCallback<FocusOutEvent>(OnFocusOut);

            var message = visualElement.Q("TextInputMessage");
            message.UnregisterCallback<FocusInEvent>(OnFocusIn);
            message.UnregisterCallback<FocusOutEvent>(OnFocusOut);
        }

        public override void OnToolOpened()
        {
            m_TopicPanel.Show();
            m_CommentPanel.Hide();
            m_TextInput.Hide();

            m_IndicatorManager.SetIndicatorsVisible();
            m_InputBlockerEvents.OnDispatchRay += OnDispatchRay;
            EnableObjectSelection();

            m_TopicPanel.TopicSelected += OnTopicSelected;
            m_TopicPanel.TopicDeleted += OnTopicDeleted;
            m_TopicPanel.TopicEdited += OnTopicEdited;

            m_CommentPanel.AddClicked += OnAddComment;
            m_CommentPanel.GotoClicked += OnGotoClicked;
            m_CommentPanel.BackClicked += OnBackClicked;
            m_CommentPanel.CommentDeleted += OnDeleteComment;
            m_CommentPanel.CommentEdited += OnEditComment;

            m_TextInput.CancelClicked += OnCancelClicked;
            m_TextInput.AddTopicSubmitClicked += OnSubmitAddTopic;
            m_TextInput.EditTopicSubmitClicked += OnSubmitEditTopic;
            m_TextInput.AddCommentSubmitClicked += OnSubmitAddComment;
            m_TextInput.EditCommentSubmitClicked += OnSubmitEditComment;
        }

        public override void OnToolClosed()
        {
            m_TextInput.Clear();
            m_IndicatorManager.SetIndicatorsVisible(visible: false);
            m_InputBlockerEvents.OnDispatchRay -= OnDispatchRay;
            DisableObjectSelection();

            m_TopicPanel.TopicSelected -= OnTopicSelected;
            m_TopicPanel.TopicDeleted -= OnTopicDeleted;
            m_TopicPanel.TopicEdited -= OnTopicEdited;

            m_CommentPanel.AddClicked -= OnAddComment;
            m_CommentPanel.GotoClicked -= OnGotoClicked;
            m_CommentPanel.BackClicked -= OnBackClicked;
            m_CommentPanel.CommentDeleted -= OnDeleteComment;
            m_CommentPanel.CommentEdited -= OnEditComment;

            m_TextInput.CancelClicked -= OnCancelClicked;
            m_TextInput.AddTopicSubmitClicked -= OnSubmitAddTopic;
        }

        void EnableObjectSelection()
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

        void DisableObjectSelection()
        {
            if (m_ObjectSelectionProperty != null)
            {
                m_ObjectSelectionProperty.ValueChanged -= OnObjectSelectionChanged;
            }

            // Disable selection tool
            m_ObjectSelectionActivator?.Unsubscribe(this);

            m_WorkingIndicator.gameObject.SetActive(false);
        }

        void OnInitialized(IEnumerable<ITopic> topics)
        {
            m_TopicPanel.RefreshTopic(topics);
            m_IndicatorManager.SetIndicators(topics);
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
                m_TopicPanel.AddTopicEntry(topic);
            }
        }

        void OnTopicRemoved(Guid topicId)
        {
            if (m_CurrentTopic != null && m_CurrentTopic.Id == topicId)
            {
                if (m_TextInput.IsOpened)
                {
                    CancelInput();
                }

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
            m_TextInput.Initialize(rootVisualElement);
        }

        void OnCancelClicked()
        {
            CancelInput();
        }

        void CancelInput()
        {
            m_TextInput.Clear();
            m_WorkingIndicator.gameObject.SetActive(false);
            m_EditedComment = null;
            m_TopicPanel.EnablePanel();
            m_CommentPanel.EnablePanel();
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

        async void OnTopicSelected(ITopic topic)
        {
            await SelectTopic(topic);
        }

        async Task SelectTopic(ITopic topic)
        {
            m_CurrentTopic = topic;
            m_IndicatorManager.GetIndicator(topic.Id)?.SetSelected(true);

            var comments = await topic.GetCommentsAsync();
            ShowComments(comments);
        }

        void ShowComments(IEnumerable<IComment> comments)
        {
            m_CommentPanel.Show();
            m_TopicPanel.Hide();

            DisableObjectSelection();

            m_CurrentTopic.CommentCreated += UpdateCommentPanel;
            m_CurrentTopic.CommentRemoved += UpdateCommentPanel;
            m_CurrentTopic.CommentUpdated += UpdateCommentPanel;

            m_CommentPanel.ShowComments(m_CurrentTopic, comments);
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

            m_IndicatorManager.GetIndicator(m_CurrentTopic.Id)?.SetSelected(false);

            m_CurrentTopic = null;

            m_TopicPanel.Show();
            m_CommentPanel.Hide();
            EnableObjectSelection();
        }

        void UpdateCommentPanel(IComment comment)
        {
            UpdateCommentPanelTask().ConfigureAwait(false);
        }

        async Task UpdateCommentPanelTask()
        {
            var comments = await m_CurrentTopic.GetCommentsAsync();
            ShowComments(comments.ToList());
        }

        void AddTopic()
        {
            m_TopicPanel.EnablePanel(false);
            m_TextInput.ShowAddTopic();
        }

        void OnTopicEdited(ITopic topic)
        {
            m_TopicPanel.EnablePanel(false);

            var indicator = m_IndicatorManager.GetIndicator(topic.Id);
            if (indicator != null)
            {
                indicator.gameObject.SetActive(false);
                m_WorkingIndicator.transform.position = indicator.transform.position;
                m_WorkingIndicator.gameObject.SetActive(true);
            }

            m_Controller.GetTopic(topic.Id, (ITopic t) =>
            {
                m_TextInput.ShowEditTopic(t.Title, t.Description);
                m_EditedTopic = t;
                GotoTopic(t);
            });
        }

        void OnTopicDeleted(ITopic topic)
        {
            m_Controller.DeleteTopic(topic);
        }

        void OnAddComment()
        {
            m_TextInput.ShowAddComment();
            m_CommentPanel.EnablePanel(false);
        }

        void OnEditComment(IComment comment)
        {
            m_TextInput.ShowEditComment(comment.Text);
            m_CommentPanel.EnablePanel(false);
            m_EditedComment = comment;
        }

        void OnDeleteComment(IComment comment)
        {
            m_Controller.DeleteComment(m_CurrentTopic, comment);
        }

        void OnSubmitAddTopic(string title, string description)
        {
            m_TopicPanel.EnablePanel();
            m_WorkingIndicator.gameObject.SetActive(false);

            var cameraTransform = m_Camera.transform;
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
            m_TopicPanel.EnablePanel();

            m_Controller.GetTopic(m_EditedTopic.Id, (ITopic t) =>
            {
                var cameraTransform = m_Camera.transform;
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

        void OnSubmitAddComment(string text)
        {
            m_CommentPanel.EnablePanel();
            m_Controller.CreateComment(m_CurrentTopic, text);
        }

        void OnSubmitEditComment(string text)
        {
            m_CommentPanel.EnablePanel();

            if (string.IsNullOrEmpty(text))
            {
                OnDeleteComment(m_EditedComment);
            }
            else if (text != m_EditedComment.Text)
            {
                m_Controller.UpdateComment(m_CurrentTopic, m_EditedComment, text);
            }

            m_EditedComment = null;
        }

        void OnDispatchRay(Ray ray)
        {
            // If we are adding or editing a comment, we don't want to select an indicator
            if (m_TextInput.IsOpened)
                return;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, m_Camera.farClipPlane, m_IndicatorsLayerMask))
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
            if (m_CurrentTopic != null)
            {
                m_IndicatorManager.GetIndicator(m_CurrentTopic.Id)?.SetSelected(false);
                m_CurrentTopic.CommentCreated -= UpdateCommentPanel;
                m_CurrentTopic.CommentRemoved -= UpdateCommentPanel;
                m_CurrentTopic.CommentUpdated -= UpdateCommentPanel;

                // If the same indicator is selected again
                if (m_CurrentTopic.Id == indicator.Topic.Id)
                {
                    m_CurrentTopic = null;
                    m_TopicPanel.Show();
                    m_CommentPanel.Hide();
                    EnableObjectSelection();
                    return;
                }
            }

            SelectTopic(indicator.Topic).ConfigureAwait(false);
        }

        void OnObjectSelectionChanged(IObjectSelectionInfo objectSelectionInfo)
        {
            if (objectSelectionInfo.SelectedInstanceId == InstanceId.None)
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.Cloud.Annotation;
using Unity.Cloud.Annotation.Runtime;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.Navigation;
using Unity.ReferenceProject.UIInputBlocker;
using UnityEngine.UIElements;
using Zenject;
using Avatar = Unity.AppUI.UI.Avatar;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.ReferenceProject.Annotation
{
    public class AnnotationToolUIController : ToolUIController
    {
        [SerializeField]
        ColorPalette m_ColorPalette;

        [SerializeField]
        LayerMask m_IndicatorsLayerMask;

        [Header("UI Toolkit")]
        [SerializeField]
        VisualTreeAsset m_TopicEntryTemplate;

        [SerializeField]
        VisualTreeAsset m_CommentEntryTemplate;

        [SerializeField]
        VisualTreeAsset m_TopicDialogTemplate;

        [SerializeField]
        VisualTreeAsset m_CommentDialogTemplate;

        [Header("Localization")]
        [SerializeField]
        string m_Delete = "@Annotation:Delete";

        [SerializeField]
        string m_Edit = "@Annotation:Edit";

        [SerializeField]
        string m_NoTopic = "@Annotation:NoTopic";

        [SerializeField]
        string m_NoComment = "@Annotation:NoComment";

        [SerializeField]
        string m_NoTitleWarning = "@Annotation:NoTitleWarning";

        [SerializeField]
        string m_NoCommentWarning = "@Annotation:NoCommentWarning";

        VisualElement m_TopicPanel;
        VisualElement m_TopicContainer;
        VisualElement m_CommentsPanel;
        VisualElement m_CommentTopicContainer;
        VisualElement m_CommentContainer;
        VisualElement m_LastTopicEntry;

        Guid m_CurrentTopicId;
        readonly Dictionary<Guid, VisualElement> m_TopicVisualElements = new();
        readonly Dictionary<Guid, ITopic> m_Topics = new();

        UserInfo m_UserInfo;

        IAnnotationController m_Controller;
        IAnnotationIndicatorManager m_IndicatorManager;
        IAppMessaging m_AppMessaging;
        IUIInputBlockerEvents m_InputBlockerEvents;
        IUserInfoProvider m_UserInfoProvider;
        Camera m_Camera;
        INavigationManager m_NavigationManager;

        [Inject]
        void Setup(IAnnotationController annotationController, IAnnotationIndicatorManager indicatorManager,
            IAppMessaging appMessaging, IUIInputBlockerEvents inputBlockerEvents, IUserInfoProvider userInfoProvider,
            Camera camera, INavigationManager navigationManager)
        {
            m_Controller = annotationController;
            m_IndicatorManager = indicatorManager;
            m_AppMessaging = appMessaging;
            m_InputBlockerEvents = inputBlockerEvents;
            m_UserInfoProvider = userInfoProvider;
            m_Camera = camera;
            m_NavigationManager = navigationManager;
        }

        protected override void Awake()
        {
            base.Awake();
            m_Controller.Initialized += OnInitialized;
            m_Controller.TopicCreatedOrUpdated += OnTopicCreatedOrUpdated;
            m_Controller.TopicRemoved += OnTopicRemoved;

            GetUserInfoAsync().ConfigureAwait(false);
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
        }

        public override void OnToolOpened()
        {
            Show(m_TopicPanel);
            Hide(m_CommentsPanel);
            m_IndicatorManager.SetIndicatorsVisible();
            m_InputBlockerEvents.OnDispatchRay += OnDispatchRay;
        }

        public override void OnToolClosed()
        {
            m_IndicatorManager.SetIndicatorsVisible(visible:false);
            m_InputBlockerEvents.OnDispatchRay -= OnDispatchRay;
        }

        async Task GetUserInfoAsync()
        {
            m_UserInfo = await m_UserInfoProvider.GetUserInfoAsync();
        }

        void OnInitialized(IEnumerable<ITopic> topics)
        {
            m_TopicContainer.Clear();
            m_Topics.Clear();
            m_TopicVisualElements.Clear();
            var size = topics.Count();
            if (size == 0)
            {
                AddNoTopicMessage();
            }
            else
            {
                m_IndicatorManager.SetIndicators(topics);

                for (int i = 0; i < size; i++)
                {
                    var topic = topics.ElementAt(i);
                    var topicEntry = CreateTopicEntry(topic);
                    m_TopicContainer.Add(topicEntry);
                    m_Topics.Add(topic.Id, topic);
                    m_TopicVisualElements.Add(topic.Id, topicEntry);

                    if (i == size - 1)
                    {
                        m_LastTopicEntry = topicEntry;
                        var divider = topicEntry.Q("TopicEntryDivider");
                        Hide(divider);
                    }
                }
            }
        }

        void AddNoTopicMessage()
        {
            var noTopic = new Text(m_NoTopic);
            noTopic.AddToClassList("topic-no-topic");
            m_TopicContainer.Add(noTopic);
        }

        void OnTopicCreatedOrUpdated(ITopic topic)
        {
            if (m_TopicVisualElements.TryGetValue(topic.Id, out var topicEntry))
            {
                m_Topics[topic.Id] = topic;
                var indicator = m_IndicatorManager.GetIndicator(topic.Id);
                indicator.Topic = topic;
                UpdateTopicEntry(topicEntry, topic);
            }
            else
            {
                if (!m_Topics.Any())
                {
                    // Remove no topic message
                    m_TopicContainer.Clear();
                }

                m_IndicatorManager.AddIndicator(topic);

                topicEntry = CreateTopicEntry(topic);
                m_TopicContainer.Add(topicEntry);
                m_Topics.Add(topic.Id, topic);
                m_TopicVisualElements.Add(topic.Id, topicEntry);

                // Update dividers
                if (m_LastTopicEntry != null)
                {
                    var divider = m_LastTopicEntry.Q("TopicEntryDivider");
                    Show(divider);
                    m_LastTopicEntry = topicEntry;
                    divider = topicEntry.Q("TopicEntryDivider");
                    Hide(divider);
                }
            }
        }

        void OnTopicRemoved(Guid topicId)
        {
            m_IndicatorManager.RemoveIndicator(topicId);
            if (m_TopicVisualElements.TryGetValue(topicId, out var topicEntry))
            {
                m_TopicContainer.Remove(topicEntry);
                m_Topics.Remove(topicId);
                m_TopicVisualElements.Remove(topicId);

                if (topicEntry == m_LastTopicEntry)
                {
                    var size = m_TopicContainer.childCount;
                    if (size > 0)
                    {
                        m_LastTopicEntry = m_TopicContainer.ElementAt(size - 1);
                        var divider = m_LastTopicEntry.Q("TopicEntryDivider");
                        Hide(divider);
                    }
                    else
                    {
                        m_LastTopicEntry = null;
                        AddNoTopicMessage();
                    }
                }
            }
        }

        void Initialize(VisualElement rootVisualElement)
        {
            m_TopicPanel = rootVisualElement.Q("TopicList");
            m_TopicContainer = rootVisualElement.Q("TopicListContainer");
            var addTopicButton = rootVisualElement.Q<ActionButton>("AddTopicButton");
            addTopicButton.clicked += AddTopic;

            m_CommentsPanel = rootVisualElement.Q("CommentList");
            m_CommentTopicContainer = rootVisualElement.Q("CommentTopicContainer");
            m_CommentContainer = rootVisualElement.Q("CommentListContainer");

            var backButton = rootVisualElement.Q<ActionButton>("BackButton");
            backButton.clicked += () =>
            {
                m_Topics[m_CurrentTopicId].CommentCreated -= UpdateCommentPanel;
                m_Topics[m_CurrentTopicId].CommentRemoved -= UpdateCommentPanel;
                m_Topics[m_CurrentTopicId].CommentUpdated -= UpdateCommentPanel;

                m_IndicatorManager.GetIndicator(m_CurrentTopicId)?.SetSelected(false);

                m_CurrentTopicId = Guid.Empty;

                Show(m_TopicPanel);
                Hide(m_CommentsPanel);
            };

            var addCommentButton = rootVisualElement.Q<ActionButton>("AddCommentButton");
            addCommentButton.clicked += AddComment;
        }

        VisualElement CreateTopicEntry(ITopic topic, bool isTopicPanel = true)
        {
            var topicEntry = m_TopicEntryTemplate.Instantiate();
            var topicContainer = topicEntry.Q("TopicEntryContainer");
            var reply = topicEntry.Q("TopicEntryReply");
            var optionButton = topicEntry.Q<ActionButton>("TopicEntryOptionButton");

            UpdateTopicEntry(topicEntry, topic);

            if (isTopicPanel)
            {
                topicContainer.focusable = true;
                topicContainer.AddManipulator(new Pressable());
                UpdateComments(topic, topicContainer).ConfigureAwait(false);

                if(topic.CreationAuthor.Id != m_UserInfo?.Id)
                {
                    optionButton.SetEnabled(false);
                }
                optionButton.clicked += () =>
                {
                    var contentView = new VisualElement();
                    contentView.style.alignItems = Align.FlexStart;
                    var popover = Popover.Build(optionButton, contentView);

                    contentView.Add(OptionButton("delete", m_Delete, () =>
                    {
                        DeleteTopic(topic);
                        popover.Dismiss();
                    }));

                    contentView.Add(OptionButton("pen", m_Edit, () =>
                    {
                        popover.Dismiss();
                        EditTopic(topic);
                    }));

                    popover.Show();
                };
            }
            else
            {
                Hide(reply);
                Hide(optionButton);
                Hide(topicEntry.Q<Divider>());
            }

            return topicEntry;
        }

        void UpdateTopicEntry(VisualElement topicEntry, ITopic topic)
        {
            var avatar = topicEntry.Q<Avatar>();
            var topicTitle = topicEntry.Q<Text>("TopicEntryTitle");
            var topicDescription = topicEntry.Q<Text>("TopicEntryDescription");
            var topicDate = topicEntry.Q<Text>("TopicEntryDate");
            var topicAuthor = topicEntry.Q<Text>("TopicEntryAuthor");

            UpdateEntryHeader(avatar, topicAuthor, topic.CreationAuthor, topicDate, topic.CreationDate);

            topicTitle.text = topic.Title;

            var description = topic.Description;
            if (string.IsNullOrWhiteSpace(description))
            {
                Hide(topicDescription);
            }
            else
            {
                Show(topicDescription);
                topicDescription.text = description;
            }
        }

        void UpdateEntryHeader(Avatar avatar, Text authorText, Author author, Text dateText, DateTime date)
        {
            var avatarInitials = avatar.Q<Text>();
            avatar.outlineColor = Color.clear;
            avatarInitials.text = Utils.GetInitials(author.FullName);
            avatar.backgroundColor = m_ColorPalette.GetColor(author.ColorIndex);

            authorText.text = author.FullName;

            dateText.text = Utils.GetTimeIntervalSinceNow(date.ToLocalTime(), out var variables);
            var localizedTextElement = dateText.Q<LocalizedTextElement>();
            localizedTextElement.variables = variables;
            dateText.tooltip = date.ToString();
        }

        VisualElement OptionButton(string icon, string text, Action action)
        {
            var button = new ActionButton();
            button.quiet = true;
            button.icon = icon;
            button.label = text;
            button.clicked += () =>
            {
                action?.Invoke();
            };
            return button;
        }

        async Task UpdateComments(ITopic topic, VisualElement topicEntry)
        {
            var reply = topicEntry.Q<Text>("TopicEntryReply");

            await UpdateReplyLabel(topic, reply);

            topic.CommentCreated += async _ =>
            {
                await UpdateReplyLabel(topic, reply);
            };

            topic.CommentRemoved += async _ =>
            {
                await UpdateReplyLabel(topic, reply);
            };

            topicEntry.RegisterCallback<ClickEvent>(async _ =>
            {
                await SelectTopic(topic.Id);
            });
        }

        async Task UpdateReplyLabel(ITopic topic, Text reply)
        {
            var comments = await topic.GetCommentsAsync();
            var count = comments.Count();

            if (count > 0)
            {
                Show(reply);
                var localisedText = reply.Q<LocalizedTextElement>();
                localisedText.variables = new object[] { count };
            }
            else
            {
                Hide(reply);
            }
        }

        async Task SelectTopic(Guid topicId)
        {
            var topic = m_Topics[topicId];

            m_CurrentTopicId = topicId;
            m_IndicatorManager.GetIndicator(m_CurrentTopicId)?.SetSelected(true);

            m_NavigationManager.TryTeleport(topic.WorldCameraTransform.Position.ToVector3(),
                topic.WorldCameraTransform.Rotation.ToQuaternion().eulerAngles);

            var comments = await topic.GetCommentsAsync();
            ShowComments(comments);
        }

        void ShowComments(IEnumerable<IComment> comments)
        {
            Show(m_CommentsPanel);
            Hide(m_TopicPanel);

            m_Topics[m_CurrentTopicId].CommentCreated += UpdateCommentPanel;
            m_Topics[m_CurrentTopicId].CommentRemoved += UpdateCommentPanel;
            m_Topics[m_CurrentTopicId].CommentUpdated += UpdateCommentPanel;

            m_CommentTopicContainer.Clear();
            m_CommentContainer.Clear();

            var topicEntry = CreateTopicEntry(m_Topics[m_CurrentTopicId], false);
            var topicEntryContainer = topicEntry.Q("TopicEntryContainer");
            topicEntryContainer.AddToClassList("topic-entry-container-selected");
            m_CommentTopicContainer.Add(topicEntry);

            if (!comments.Any())
            {
                var noComment = new Text(m_NoComment);
                noComment.AddToClassList("comment-no-comment");
                m_CommentContainer.Add(noComment);
            }
            else
            {
                var size = comments.Count();
                for (int i = 0; i < size; i++)
                {
                    var comment = comments.ElementAt(i);
                    Debug.Log(comment.TopicId);
                    var commentUI = CreateCommentEntry(comment);
                    m_CommentContainer.Add(commentUI);

                    if (i == size - 1)
                    {
                        var divider = commentUI.Q("CommentEntryDivider");
                        Hide(divider);
                    }
                }
            }
        }

        VisualElement CreateCommentEntry(IComment comment)
        {
            var commentEntry = m_CommentEntryTemplate.Instantiate();

            UpdateCommentEntry(commentEntry, comment);

            var optionButton = commentEntry.Q<ActionButton>("CommentEntryOptionButton");
            if(comment.Author.Id != m_UserInfo?.Id)
            {
                optionButton.SetEnabled(false);
            }
            optionButton.clicked += () =>
            {
                var contentView = new VisualElement();
                contentView.style.alignItems = Align.FlexStart;
                var popover = Popover.Build(optionButton, contentView);

                contentView.Add(OptionButton("delete", m_Delete, () =>
                {
                    popover.Dismiss();
                    DeleteComment(comment);
                }));

                contentView.Add(OptionButton("pen", m_Edit, () =>
                {
                    popover.Dismiss();
                    EditComment(comment);
                }));

                popover.Show();
            };

            return commentEntry;
        }

        void AddTopic()
        {
            var topicDialog = m_TopicDialogTemplate.Instantiate();
            var title = topicDialog.Q<TextField>("TopicDialogTitle");
            var description = topicDialog.Q<TextField>("TopicDialogDescription");
            var cancelButton = topicDialog.Q<ActionButton>("TopicDialogCancel");
            var submitButton = topicDialog.Q<ActionButton>("TopicDialogSubmit");

            var modal = m_AppMessaging.ShowCustomDialog(topicDialog);
            cancelButton.clicked += () =>
            {
                modal.Dismiss();
            };
            submitButton.clicked += () =>
            {
                var titleText = title.value;
                var descriptionText = description.value;
                if (!string.IsNullOrWhiteSpace(titleText))
                {
                    modal.Dismiss();
                    m_Controller.CreateTopic(titleText, descriptionText, m_Camera.transform.position, m_Camera.transform.rotation);
                }
                else
                {
                    m_AppMessaging.ShowWarning(m_NoTitleWarning);
                }
            };
        }

        void EditTopic(ITopic topic)
        {
            var topicDialog = m_TopicDialogTemplate.Instantiate();
            var title = topicDialog.Q<TextField>("TopicDialogTitle");
            var description = topicDialog.Q<TextField>("TopicDialogDescription");
            var cancelButton = topicDialog.Q<ActionButton>("TopicDialogCancel");
            var submitButton = topicDialog.Q<ActionButton>("TopicDialogSubmit");

            m_Controller.GetTopic(topic.Id, (ITopic t) =>
            {
                title.value = t.Title;
                description.value = t.Description;
                topic = t;
            });

            var modal = m_AppMessaging.ShowCustomDialog(topicDialog);
            cancelButton.clicked += () =>
            {
                modal.Dismiss();
            };
            submitButton.clicked += () =>
            {
                var titleText = title.value;
                var descriptionText = description.value;
                if (!string.IsNullOrWhiteSpace(titleText))
                {
                    modal.Dismiss();
                    if (titleText != topic.Title || descriptionText != topic.Description)
                    {
                        m_Controller.UpdateTopic(topic, titleText, descriptionText);
                    }
                }
                else
                {
                    m_AppMessaging.ShowWarning(m_NoTitleWarning);
                }
            };
        }

        void DeleteTopic(ITopic topic)
        {
            m_Controller.DeleteTopic(topic);
        }

        void AddComment()
        {
            var commentDialog = m_CommentDialogTemplate.Instantiate();
            var comment = commentDialog.Q<TextField>("CommentDialogText");
            var cancelButton = commentDialog.Q<ActionButton>("CommentDialogCancel");
            var submitButton = commentDialog.Q<ActionButton>("CommentDialogSubmit");

            var modal = m_AppMessaging.ShowCustomDialog(commentDialog);
            cancelButton.clicked += () =>
            {
                modal.Dismiss();
            };
            submitButton.clicked += () =>
            {
                var text = comment.value;
                if (!string.IsNullOrEmpty(text))
                {
                    modal.Dismiss();
                    m_Controller.CreateComment(m_Topics[m_CurrentTopicId], text);
                }
                else
                {
                    m_AppMessaging.ShowWarning(m_NoCommentWarning);
                }
            };
        }

        void EditComment(IComment comment)
        {
            var commentDialog = m_CommentDialogTemplate.Instantiate();
            var textField = commentDialog.Q<TextField>("CommentDialogText");
            var cancelButton = commentDialog.Q<ActionButton>("CommentDialogCancel");
            var submitButton = commentDialog.Q<ActionButton>("CommentDialogSubmit");

            textField.value = comment.Text;

            var modal = m_AppMessaging.ShowCustomDialog(commentDialog);
            cancelButton.clicked += () =>
            {
                modal.Dismiss();
            };

            submitButton.clicked += () =>
            {
                modal.Dismiss();
                var text = textField.value;
                if (string.IsNullOrEmpty(text))
                {
                    DeleteComment(comment);
                }
                else if (text != comment.Text)
                {
                    m_Controller.UpdateComment(m_Topics[m_CurrentTopicId], comment, text);
                }
            };
        }

        void DeleteComment(IComment comment)
        {
            m_Controller.DeleteComment(m_Topics[m_CurrentTopicId], comment);
        }

        void UpdateCommentEntry(VisualElement commentEntry, IComment comment)
        {
            var avatar = commentEntry.Q<Avatar>();
            var commentDate = commentEntry.Q<Text>("CommentEntryDate");
            var commentAuthor = commentEntry.Q<Text>("CommentEntryAuthor");
            var commentText = commentEntry.Q<Text>("CommentEntryText");

            UpdateEntryHeader(avatar, commentAuthor, comment.Author, commentDate, comment.Date);

            commentText.text = comment.Text;
        }

        void UpdateCommentPanel(IComment comment)
        {
            UpdateCommentPanelTask().ConfigureAwait(false);
        }

        async Task UpdateCommentPanelTask()
        {
            var comments = await m_Topics[m_CurrentTopicId].GetCommentsAsync();
            ShowComments(comments.ToList());
        }

        void OnDispatchRay(Ray ray)
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, m_IndicatorsLayerMask))
            {
                var indicator = hit.collider.GetComponentInParent<AnnotationIndicatorController>();
                if (indicator != null)
                {
                    SelectIndicator(indicator);
                }
            }
        }

        void SelectIndicator(AnnotationIndicatorController indicator)
        {
            if(m_CurrentTopicId != Guid.Empty)
            {
                m_IndicatorManager.GetIndicator(m_CurrentTopicId)?.SetSelected(false);
                m_Topics[m_CurrentTopicId].CommentCreated -= UpdateCommentPanel;
                m_Topics[m_CurrentTopicId].CommentRemoved -= UpdateCommentPanel;
                m_Topics[m_CurrentTopicId].CommentUpdated -= UpdateCommentPanel;

                // If the same indicator is selected again
                if (m_CurrentTopicId == indicator.Topic.Id)
                {
                    m_CurrentTopicId = Guid.Empty;
                    Show(m_TopicPanel);
                    Hide(m_CommentsPanel);
                    return;
                }
            }

            SelectTopic(indicator.Topic.Id).ConfigureAwait(false);
        }

        static void Show(VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.Flex;
        }

        static void Hide(VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.None;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Annotation;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.CustomKeyboard;
using Unity.ReferenceProject.InputSystem;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using TextField = UnityEngine.UIElements.TextField;

namespace Unity.ReferenceProject.Annotation
{
    public class TopicPanelController : MonoBehaviour
    {
        [SerializeField]
        ColorPalette m_ColorPalette;

        [Header("UI Toolkit")]
        [SerializeField]
        VisualTreeAsset m_TopicEntryTemplate;

        [SerializeField]
        VisualTreeAsset m_TextInputTemplate;

        [SerializeField]
        VisualTreeAsset m_OptionButtonTemplate;

        [Header("Localization")]
        [SerializeField]
        string m_Delete = "@Annotation:Delete";

        [SerializeField]
        string m_Edit = "@Annotation:Edit";

        [SerializeField]
        string m_NoTopic = "@Annotation:NoTopic";

        [SerializeField]
        string m_DescriptionPlaceholder = "@Annotation:AddDescription";

        [SerializeField]
        string m_NoTitleWarning = "@Annotation:NoTitleWarning";

        [SerializeField]
        string m_TextTooLong = "@Annotation:TextTooLong";

        [SerializeField]
        string m_Reply = "@Annotation:Reply";

        [SerializeField]
        string m_AddReply = "@Annotation:AddReply";

        public event Action<ITopic> TopicSelected;
        public event Action<ITopic> TopicDeleted;
        public event Action<ITopic> TopicEdited;
        public event Action<string, string> TopicSubmitEdited;
        public event Action<string, string> TopicSubmitted;
        public event Action<ITopic> ReplySelected;
        public event Action CancelClicked;

        Action CancelEdit;

        public AnnotationsPermission Permissions { get; set; }

        VisualElement m_TopicPanel;
        ScrollView m_TopicContainer;
        VisualElement m_LastTopicEntry;
        VisualElement m_TopicTextInputContainer;
        AppUI.UI.TextField m_TopicTitle;
        TextArea m_TopicDescription;
        ActionButton m_SubmitButton;
        VisualElement m_FocusedTextInput;

        KeyboardHandler m_KeyboardHandler;
        ITopic m_EditedTopic;

        bool m_CanEdit;
        bool m_CanDelete;

        readonly Dictionary<TopicId, VisualElement> m_TopicVisualElements = new();

        static readonly string k_NoTopicStyle = "text__no-topic";
        static readonly string k_TopicEntrySelected = "container__topic-entry--selected";

        IUserInfoProvider m_UserInfoProvider;
        IAppMessaging m_AppMessaging;
        IInputManager m_InputManager;

        [Inject]
        void Setup(IUserInfoProvider userInfoProvider, IAppMessaging appMessaging, IInputManager inputManager)
        {
            m_UserInfoProvider = userInfoProvider;
            m_AppMessaging = appMessaging;
            m_InputManager = inputManager;
        }

        void Awake()
        {
            m_KeyboardHandler = GetComponent<KeyboardHandler>();
        }

        public void Initialize(VisualElement rootVisualElement)
        {
            m_TopicPanel = rootVisualElement.Q("TopicList");
            m_TopicContainer = rootVisualElement.Q<ScrollView>("TopicListContainer");

            m_TopicTextInputContainer = m_TextInputTemplate.Instantiate().Q("TextInputContainer");
            m_TopicTitle = m_TopicTextInputContainer.Q<AppUI.UI.TextField>("TextInputTitle");
            m_TopicTitle.validateValue += ValidateLength;
            m_TopicTitle.validateValue += ValidateSubmitState;
            m_TopicTitle.RegisterCallback<FocusInEvent>(UIFocused);
            m_TopicTitle.RegisterCallback<FocusOutEvent>(UIUnFocused);
            m_TopicDescription = m_TopicTextInputContainer.Q<TextArea>("TextInputMessage");
            m_TopicDescription.placeholder = m_DescriptionPlaceholder;
            m_TopicDescription.validateValue += ValidateLength;
            m_TopicDescription.validateValue += ValidateSubmitState;
            m_TopicDescription.RegisterCallback<FocusInEvent>(UIFocused);
            m_TopicDescription.RegisterCallback<FocusOutEvent>(UIUnFocused);
            m_SubmitButton = m_TopicTextInputContainer.Q<ActionButton>("TextInputSubmit");
            m_SubmitButton.clicked += OnSubmitPressed;
            m_SubmitButton.SetEnabled(false);
            var cancelButton = m_TopicTextInputContainer.Q<ActionButton>("TextInputCancel");
            cancelButton.clicked += OnCancelPressed;

            m_TopicPanel.hierarchy.Add(m_TopicTextInputContainer);

            if (m_KeyboardHandler != null)
            {
                m_KeyboardHandler.RegisterRootVisualElement(m_TopicTextInputContainer);
            }
        }

        public void SetPermissions(bool canEdit, bool canDelete)
        {
            m_CanEdit = canEdit;
            m_CanDelete = canDelete;
        }

        public void Show()
        {
            Utils.Show(m_TopicPanel);
        }

        public void Hide()
        {
            Utils.Hide(m_TopicPanel);
        }

        public async Task RefreshTopics(IReadOnlyCollection<ITopic> topics)
        {
            if (topics == null)
                return;

            m_TopicContainer.Clear();
            m_TopicVisualElements.Clear();

            var size = topics.Count();
            if (size == 0)
            {
                AddNoTopicMessage();
            }
            else
            {
                var userInfo = await Utils.GetCurrentUserInfoAsync(m_UserInfoProvider);

                for (int i = size - 1; i >= 0; i--)
                {
                    var topic = topics.ElementAt(i);
                    var topicEntry = CreateTopicEntry(topic, Utils.IsSameUser(userInfo, topic.CreationAuthor));

                    m_TopicContainer.Add(topicEntry);
                    m_TopicVisualElements.Add(topic.Id, topicEntry);

                    if (i == 0)
                    {
                        m_LastTopicEntry = topicEntry;
                        var divider = topicEntry.Q("TopicEntryDivider");
                        Utils.Hide(divider);
                    }
                }
            }

            ResetTextInput();
        }

        public bool IsTopicExist(ITopic topic)
        {
            return m_TopicVisualElements.ContainsKey(topic.Id);
        }

        public async Task AddTopicEntry(ITopic topic)
        {
            if (!m_TopicVisualElements.Any())
            {
                // Remove no topic message
                m_TopicContainer.Clear();
            }

            var userInfo = await Utils.GetCurrentUserInfoAsync(m_UserInfoProvider);
            var topicEntry = CreateTopicEntry(topic, Utils.IsSameUser(userInfo, topic.CreationAuthor));
            m_TopicContainer.Add(topicEntry);
            m_TopicVisualElements.Add(topic.Id, topicEntry);
            topicEntry.SendToBack();

            // Update dividers
            if (m_LastTopicEntry == null)
            {
                m_LastTopicEntry = topicEntry;
                var divider = topicEntry.Q("TopicEntryDivider");
                Utils.Hide(divider);
            }

            StartCoroutine(Common.Utils.WaitAFrame(() => m_TopicContainer.ScrollTo(topicEntry)));
        }

        public void RemoveTopicEntry(TopicId topicId)
        {
            if (m_TopicVisualElements.TryGetValue(topicId, out var topicEntry))
            {
                m_TopicContainer.Remove(topicEntry);
                m_TopicVisualElements.Remove(topicId);

                if (topicEntry == m_LastTopicEntry)
                {
                    var size = m_TopicContainer.childCount;
                    if (size > 0)
                    {
                        m_LastTopicEntry = m_TopicContainer.ElementAt(0);
                        var divider = m_LastTopicEntry.Q("TopicEntryDivider");
                        Utils.Hide(divider);
                    }
                    else
                    {
                        m_LastTopicEntry = null;
                        AddNoTopicMessage();
                    }
                }
            }
        }

        public void AddTopic()
        {
            ResetTextInput();
            Common.Utils.SetVisible(m_TopicTextInputContainer, true);
        }

        public void ResetTextInput()
        {
            CancelEdit?.Invoke();
            m_TopicTitle.value = string.Empty;
            m_TopicDescription.value = string.Empty;
            Common.Utils.SetVisible(m_TopicTextInputContainer, false);
        }

        void AddNoTopicMessage()
        {
            var noTopic = new Text(m_NoTopic);
            noTopic.AddToClassList(k_NoTopicStyle);
            m_TopicContainer.Add(noTopic);
        }

        VisualElement CreateTopicEntry(ITopic topic, bool isUserAuthor)
        {
            var topicEntry = m_TopicEntryTemplate.Instantiate();
            var topicContainer = topicEntry.Q("TopicEntryContainer");
            var topicText = topicEntry.Q("TopicEntryText");
            var topicTitle = topicEntry.Q<Text>("TopicEntryTitle");
            var topicDescription = topicEntry.Q<Text>("TopicEntryDescription");
            var topicTextInputContainer = topicEntry.Q("TopicEntryTextInput");
            var topicTitleInput = topicEntry.Q<AppUI.UI.TextField>("TopicEntryTextField");
            var topicDescriptionInput = topicEntry.Q<TextArea>("TopicEntryTextArea");
            var topicCancel = topicEntry.Q<ActionButton>("TextInputCancel");
            var topicSubmit = topicEntry.Q<ActionButton>("TextInputSubmit");
            var optionButton = topicEntry.Q<ActionButton>("TopicEntryOptionButton");
            var reply = topicEntry.Q<Text>("TopicEntryReply");

            void ResetEntry()
            {
                m_EditedTopic = null;
                CancelEdit -= ResetEntry;

                topicTitleInput.UnregisterCallback<FocusInEvent>(UIFocused);
                topicTitleInput.UnregisterCallback<FocusOutEvent>(UIUnFocused);
                topicDescriptionInput.UnregisterCallback<FocusInEvent>(UIFocused);
                topicDescriptionInput.UnregisterCallback<FocusOutEvent>(UIUnFocused);

                Common.Utils.SetVisible(topicText, true);
                Common.Utils.SetVisible(topicTextInputContainer, false);
                optionButton.SetEnabled(true);
                reply.SetEnabled(true);
            }

            topicTitleInput.validateValue += ValidateLength;
            topicDescriptionInput.validateValue += ValidateLength;
            topicCancel.clicked += ResetEntry;
            topicSubmit.clicked += () =>
            {
                ResetEntry();
                TopicSubmitEdited?.Invoke(topicTitleInput.value, topicDescriptionInput.value);
            };

            var color = m_ColorPalette.GetColor(topic.CreationAuthor.ColorIndex);
            Utils.UpdateTopicEntry(topicEntry, topic, color);

            _ = UpdateComments(topic, topicContainer);

            topicContainer.focusable = true;
            topicContainer.AddManipulator(new Pressable());

            var replyPressable = new Pressable();
            reply.AddManipulator(replyPressable);
            replyPressable.clicked += () => {
                ReplySelected?.Invoke(topic);
            };

            optionButton.clicked += () =>
            {
                var contentView = new VisualElement();
                contentView.style.alignItems = Align.Stretch;
                var popover = Popover.Build(optionButton, contentView);

                var commentButton = Utils.OptionButton(m_OptionButtonTemplate, "comment", m_AddReply, () =>
                {
                    popover.Dismiss();
                    ReplySelected?.Invoke(topic);
                });
                contentView.Add(commentButton);

                var deleteButton = Utils.OptionButton(m_OptionButtonTemplate,"delete", m_Delete, () =>
                {
                    TopicDeleted?.Invoke(topic);

                    // Hide topic entry before removing it from the list to avoid user manipulating it while it's being removed
                    Utils.Hide(topicEntry);
                    popover.Dismiss();
                });

                deleteButton.SetEnabled(isUserAuthor && m_CanDelete);
                contentView.Add(deleteButton);

                var editButton = Utils.OptionButton(m_OptionButtonTemplate,"pen", m_Edit, () =>
                {
                    popover.Dismiss();
                    CancelEdit?.Invoke();
                    m_EditedTopic = topic;

                    TopicEdited?.Invoke(topic);

                    Common.Utils.SetVisible(topicText, false);
                    Common.Utils.SetVisible(topicTextInputContainer, true);

                    topicTitleInput.value = topicTitle.text;
                    topicDescriptionInput.value = topicDescription.text;

                    topicTitle.RegisterCallback<FocusInEvent>(UIFocused);
                    topicTitle.RegisterCallback<FocusOutEvent>(UIUnFocused);
                    topicDescription.RegisterCallback<FocusInEvent>(UIFocused);
                    topicDescription.RegisterCallback<FocusOutEvent>(UIUnFocused);

                    optionButton.SetEnabled(false);
                    reply.SetEnabled(false);

                    CancelEdit += ResetEntry;
                });
                editButton.SetEnabled(isUserAuthor && m_CanEdit);
                contentView.Add(editButton);

                popover.Show();
            };

            if (m_KeyboardHandler != null)
            {
                m_KeyboardHandler.RegisterRootVisualElement(topicEntry);
            }

            return topicEntry;
        }

        public void UpdateTopicEntry(ITopic topic)
        {
            if (m_TopicVisualElements.TryGetValue(topic.Id, out var topicEntry))
            {
                var color = m_ColorPalette.GetColor(topic.CreationAuthor.ColorIndex);
                Utils.UpdateTopicEntry(topicEntry, topic, color);
            }
        }

        public void SelectTopicEntry(ITopic topic)
        {
            foreach (var keyValue in m_TopicVisualElements)
            {
                var topicId = keyValue.Key;
                var topicEntry = keyValue.Value;
                var topicContainer = topicEntry.Q("TopicEntryContainer");

                if (topic != null && topicId == topic.Id)
                {
                    topicContainer.AddToClassList(k_TopicEntrySelected);
                    m_TopicContainer.ScrollTo(topicContainer);
                }
                else
                {
                    topicContainer.RemoveFromClassList(k_TopicEntrySelected);
                }
            }
        }

        async Task UpdateComments(ITopic topic, VisualElement topicEntry)
        {
            var reply = topicEntry.Q<Text>("TopicEntryReply");
            Utils.Hide(reply);

            await UpdateReplyLabel(topic, reply);

            topic.CommentCreated += async _ =>
            {
                await UpdateReplyLabel(topic, reply);
            };

            topic.CommentRemoved += async _ =>
            {
                await UpdateReplyLabel(topic, reply);
            };

            topicEntry.RegisterCallback<ClickEvent>(_ =>
            {
                if (topic != m_EditedTopic)
                {
                    TopicSelected?.Invoke(topic);
                }
            });
        }

        async Task UpdateReplyLabel(ITopic topic, Text reply)
        {
            var comments = await topic.GetCommentsAsync();
            var count = comments.Count();

            Utils.Show(reply);
            if (count == 0)
            {
                reply.text = m_AddReply;
            }
            else
            {
                reply.text = m_Reply;
                var localisedText = reply.Q<LocalizedTextElement>();
                localisedText.variables = new object[] { count };
            }
        }

        void OnSubmitPressed()
        {
            if (m_TopicTitle.value.Length == 0)
            {
                m_AppMessaging.ShowWarning(m_NoTitleWarning);
                return;
            }

            TopicSubmitted?.Invoke(m_TopicTitle.value, m_TopicDescription.value);
            ResetTextInput();
        }

        void OnCancelPressed()
        {
            CancelClicked?.Invoke();
            ResetTextInput();
        }

        bool ValidateLength(string newValue)
        {
            if (newValue.Length > AnnotationController.k_TextMaxChar)
            {
                m_AppMessaging.ShowWarning(m_TextTooLong, false, AnnotationController.k_TextMaxChar);
                if (m_FocusedTextInput != null)
                {
                    var subValue = newValue.Substring(0, AnnotationController.k_TextMaxChar);
                    if (m_FocusedTextInput is TextField title)
                    {
                        title.value = subValue;
                    }
                    else if (m_FocusedTextInput is TextArea description)
                    {
                        description.value = subValue;
                    }
                }

                return false;
            }

            return true;
        }

        bool ValidateSubmitState(string newValue)
        {
            var isEmpty = string.IsNullOrWhiteSpace(newValue);
            m_SubmitButton.SetEnabled(!isEmpty);

            return true;
        }

        void UIFocused(FocusInEvent ev)
        {
            m_FocusedTextInput = ev.target as VisualElement;
            m_InputManager.IsUIFocused = true;
        }

        void UIUnFocused(FocusOutEvent ev)
        {
            m_FocusedTextInput = null;
            m_InputManager.IsUIFocused = false;
        }
    }
}

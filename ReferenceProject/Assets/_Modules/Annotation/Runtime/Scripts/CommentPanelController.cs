using System;
using System.Collections.Generic;
using System.Linq;
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
using Avatar = Unity.AppUI.UI.Avatar;

namespace Unity.ReferenceProject.Annotation
{
    public class CommentPanelController : MonoBehaviour
    {
        [SerializeField]
        ColorPalette m_ColorPalette;

        [Header("UI Toolkit")]
        [SerializeField]
        VisualTreeAsset m_CommentEntryTemplate;

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
        string m_CommentPlaceholder = "@Annotation:AddComment";

        [SerializeField]
        string m_TextTooLong = "@Annotation:TextTooLong";

        public event Action BackClicked;
        public event Action GotoClicked;
        public event Action<IComment> CommentDeleted;
        public event Action<IComment, string> CommentEdited;
        public event Action<string> CommentSubmitted;

        Action CancelEdit;

        static readonly string k_NoTitleUssClassStyle = "container__text-input--hidden";
        static readonly string k_SelectedUssClassStyle = "container__comment-entry--selected";

        VisualElement m_CommentsPanel;
        VisualElement m_CommentTopicContainer;
        VisualElement m_CommentContainer;
        VisualElement m_CommentSeparator;
        Text m_CommentCounterText;
        VisualElement m_CommentTextInput;
        TextArea m_CommentTextArea;
        ActionButton m_SubmitButton;
        ActionButton m_CancelButton;
        TextArea m_FocusedTextArea;

        ITopic m_CurrentTopic;
        IComment m_EditedComment;
        KeyboardHandler m_KeyboardHandler;

        IAuthenticatedUserInfoProvider m_UserInfoProvider;
        IAppMessaging m_AppMessaging;
        IInputManager m_InputManager;

        [Inject]
        void Setup(IAuthenticatedUserInfoProvider userInfoProvider, IAppMessaging appMessaging, IInputManager inputManager)
        {
            m_UserInfoProvider = userInfoProvider;
            m_AppMessaging = appMessaging;
            m_InputManager = inputManager;
        }

        void Awake()
        {
            m_KeyboardHandler = GetComponent<KeyboardHandler>();
        }

        void OnDestroy()
        {
            if (m_CommentTextArea != null)
            {
                m_CommentTextArea.validateValue -= ValidateLength;
                m_CommentTextArea.validateValue -= ValidateButtonState;
                m_CommentTextArea.UnregisterCallback<FocusInEvent>(UIFocused);
                m_CommentTextArea.UnregisterCallback<FocusOutEvent>(UIUnFocused);
            }

            if (m_SubmitButton != null)
            {
                m_SubmitButton.clicked -= OnSubmitPressed;
            }

            if (m_CancelButton != null)
            {
                m_CancelButton.clicked -= OnCancelPressed;
            }
        }

        public void Initialize(VisualElement rootVisualElement)
        {
            m_CommentsPanel = rootVisualElement.Q("CommentList");
            m_CommentTopicContainer = rootVisualElement.Q("CommentTopicContainer");
            m_CommentSeparator = rootVisualElement.Q("CommentSeparator");
            m_CommentCounterText = rootVisualElement.Q<Text>("CommentNumberReply");
            m_CommentContainer = rootVisualElement.Q("CommentListContainer");

            var goToButton = rootVisualElement.Q<ActionButton>("GotoButton");
            goToButton.clicked += () => GotoClicked?.Invoke();

            var backButton = rootVisualElement.Q<ActionButton>("BackButton");
            backButton.clicked += () => BackClicked?.Invoke();

            m_CommentTextInput = m_TextInputTemplate.Instantiate();
            m_CommentTextInput.AddToClassList(k_NoTitleUssClassStyle);
            m_CommentTextArea = m_CommentTextInput.Q<TextArea>("TextInputMessage");
            m_CommentTextArea.placeholder = m_CommentPlaceholder;
            m_CommentTextArea.validateValue += ValidateLength;
            m_CommentTextArea.validateValue += ValidateButtonState;
            m_CommentTextArea.RegisterCallback<FocusInEvent>(UIFocused);
            m_CommentTextArea.RegisterCallback<FocusOutEvent>(UIUnFocused);
            m_SubmitButton = m_CommentTextInput.Q<ActionButton>("TextInputSubmit");
            m_SubmitButton.clicked += OnSubmitPressed;
            m_SubmitButton.SetEnabled(false);
            m_CancelButton = m_CommentTextInput.Q<ActionButton>("TextInputCancel");
            m_CancelButton.clicked += OnCancelPressed;

            if (m_KeyboardHandler != null)
            {
                m_KeyboardHandler.RegisterRootVisualElement(m_CommentTextArea);
            }
        }

        public bool IsOpen()
        {
            return Common.Utils.IsVisible(m_CommentsPanel);
        }

        public void Show()
        {
            Utils.Show(m_CommentsPanel);
        }

        public void Hide()
        {
            Utils.Hide(m_CommentsPanel);
        }

        public void ShowComments(ITopic topic, IEnumerable<IComment> comments)
        {
            m_CommentTopicContainer.Clear();
            m_CommentContainer.Clear();

            var topicEntry = CreateTopicEntry(topic);
            m_CommentTopicContainer.Add(topicEntry);

            if (!comments.Any())
            {
                Common.Utils.SetVisible(m_CommentSeparator, false);
            }
            else
            {
                var size = comments.Count();
                Common.Utils.SetVisible(m_CommentSeparator, true);
                m_CommentCounterText.variables = new object[] { size };
                for (int i = 0; i < size; i++)
                {
                    var comment = comments.ElementAt(i);
                    var commentUI = CreateCommentEntry(comment);
                    m_CommentContainer.Add(commentUI);

                    if (i == size - 1)
                    {
                        var divider = commentUI.Q("CommentEntryDivider");
                        Utils.Hide(divider);
                    }
                }
            }

            m_CommentContainer.Add(m_CommentTextInput);
            ResetTextInput();
        }

        VisualElement CreateCommentEntry(IComment comment)
        {
            var commentEntry = m_CommentEntryTemplate.Instantiate();
            var commentEntryContainer = commentEntry.Q("CommentEntryContainer");
            var commentText = commentEntry.Q<Text>("CommentEntryText");
            var commentTextInputContainer = commentEntry.Q("CommentEntryTextInput");
            var commentTextInput = commentEntry.Q<TextArea>("CommentEntryTextArea");
            var commentCancel = commentEntry.Q<ActionButton>("TextInputCancel");
            var commentSubmit = commentEntry.Q<ActionButton>("TextInputSubmit");
            var optionButton = commentEntry.Q<ActionButton>("CommentEntryOptionButton");

            void ResetEntry()
            {
                CancelEdit -= ResetEntry;
                m_EditedComment = null;

                commentEntryContainer.RemoveFromClassList(k_SelectedUssClassStyle);
                commentTextInput.UnregisterCallback<FocusInEvent>(UIFocused);
                commentTextInput.UnregisterCallback<FocusOutEvent>(UIUnFocused);
                Common.Utils.SetVisible(commentText, true);
                Common.Utils.SetVisible(commentTextInputContainer, false);
                optionButton.SetEnabled(true);

                Common.Utils.SetVisible(m_CommentTextInput, true);
            }

            commentTextInput.validateValue += ValidateLength;
            commentCancel.clicked += ResetEntry;
            commentSubmit.clicked += () =>
            {
                ResetEntry();
                CommentEdited?.Invoke(comment, commentTextInput.value);
            };

            UpdateCommentEntry(commentEntry, comment);

            var isUserAuthor = comment.Author.FullName == m_UserInfoProvider.GetUserInfo(AuthenticatedUserInfoClaims.Id);

            optionButton.clicked += () =>
            {
                var contentView = new VisualElement();
                contentView.style.alignItems = Align.Stretch;
                var popover = Popover.Build(optionButton, contentView);

                var deleteButton = Utils.OptionButton(m_OptionButtonTemplate,"delete", m_Delete, () =>
                {
                    popover.Dismiss();
                    CommentDeleted?.Invoke(comment);
                });
                deleteButton.SetEnabled(isUserAuthor);
                contentView.Add(deleteButton);

                var editButton = Utils.OptionButton(m_OptionButtonTemplate,"pen", m_Edit, () =>
                {
                    commentTextInput.RegisterCallback<FocusInEvent>(UIFocused);
                    commentTextInput.RegisterCallback<FocusOutEvent>(UIUnFocused);

                    CancelEdit?.Invoke();
                    m_EditedComment = comment;
                    Common.Utils.SetVisible(m_CommentTextInput, false);

                    popover.Dismiss();
                    optionButton.SetEnabled(false);
                    commentTextInput.value = comment.Text;
                    Common.Utils.SetVisible(commentText, false);
                    Common.Utils.SetVisible(commentTextInputContainer, true);
                    commentEntryContainer.AddToClassList(k_SelectedUssClassStyle);

                    CancelEdit += ResetEntry;
                });
                editButton.SetEnabled(isUserAuthor);
                contentView.Add(editButton);

                popover.Show();
            };

            if (m_KeyboardHandler != null)
            {
                m_KeyboardHandler.RegisterRootVisualElement(commentTextInput);
            }

            return commentEntry;
        }

        void UpdateCommentEntry(VisualElement commentEntry, IComment comment)
        {
            var avatar = commentEntry.Q<Avatar>();
            var commentDate = commentEntry.Q<Text>("CommentEntryDate");
            var commentAuthor = commentEntry.Q<Text>("CommentEntryAuthor");
            var commentText = commentEntry.Q<Text>("CommentEntryText");

            var author = comment.Author;
            var color = m_ColorPalette.GetColor(author.ColorIndex);
            Utils.UpdateEntryHeader(avatar, commentAuthor, author, color, commentDate, comment.Date);

            commentText.text = comment.Text;
        }

        VisualElement CreateTopicEntry(ITopic topic)
        {
            var topicEntry = m_TopicEntryTemplate.Instantiate();
            var reply = topicEntry.Q("TopicEntryReply");
            var optionButton = topicEntry.Q<ActionButton>("TopicEntryOptionButton");

            Utils.Hide(reply);
            Utils.Hide(optionButton);
            Utils.Hide(topicEntry.Q<Divider>());

            var color = m_ColorPalette.GetColor(topic.CreationAuthor.ColorIndex);
            Utils.UpdateTopicEntry(topicEntry, topic, color);

            return topicEntry;
        }

        bool ValidateLength(string newValue)
        {
            if (newValue.Length > AnnotationController.k_TextMaxChar)
            {
                m_AppMessaging.ShowWarning(m_TextTooLong, false, AnnotationController.k_TextMaxChar);
                if(m_FocusedTextArea != null)
                {
                    m_FocusedTextArea.value = newValue.Substring(0, AnnotationController.k_TextMaxChar);
                }

                return false;
            }

            return true;
        }

        bool ValidateButtonState(string newValue)
        {
            var isEmpty = string.IsNullOrWhiteSpace(newValue);
            m_CancelButton.SetEnabled(!isEmpty);
            m_SubmitButton.SetEnabled(!isEmpty);

            return true;
        }

        void OnCancelPressed()
        {
            ResetTextInput();
        }

        void OnSubmitPressed()
        {
            CommentSubmitted?.Invoke(m_CommentTextArea.value);
            ResetTextInput();
        }

        void ResetTextInput()
        {
            m_CommentTextArea.value = string.Empty;
            Common.Utils.SetVisible(m_CommentTextInput, true);
        }

        void UIFocused(FocusInEvent ev)
        {
            m_FocusedTextArea = ev.currentTarget as TextArea;
            m_InputManager.IsUIFocused = true;
        }

        void UIUnFocused(FocusOutEvent ev)
        {
            m_FocusedTextArea = null;
            m_InputManager.IsUIFocused = false;
        }
    }
}

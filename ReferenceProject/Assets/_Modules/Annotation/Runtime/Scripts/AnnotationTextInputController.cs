using System;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.ReferenceProject.Annotation
{
    public class AnnotationTextInputController : MonoBehaviour
    {
        [Header("Localization")]
        [SerializeField]
        string m_TopicDescriptionPlaceholder = "@Annotation:AddDescription";

        [SerializeField]
        string m_CommentPlaceholder = "@Annotation:AddComment";

        [SerializeField]
        string m_NoTitleWarning = "@Annotation:NoTitleWarning";

        [SerializeField]
        string m_NoCommentWarning = "@Annotation:NoCommentWarning";

        public bool IsOpened => m_Container.style.display == DisplayStyle.Flex;

        public event Action CancelClicked;
        public event Action<string, string> AddTopicSubmitClicked;
        public event Action<string, string> EditTopicSubmitClicked;
        public event Action<string> AddCommentSubmitClicked;
        public event Action<string> EditCommentSubmitClicked;

        VisualElement m_Container;
        TextField m_Title;
        TextArea m_Message;
        ActionButton m_SubmitButton;

        IAppMessaging m_AppMessaging;

        [Inject]
        void Setup(IAppMessaging appMessaging)
        {
            m_AppMessaging = appMessaging;
        }

        public void ShowAddTopic()
        {
            Utils.Show(m_Container);
            Utils.Show(m_Title);
            m_Title.value = string.Empty;
            m_Message.value = string.Empty;
            m_Message.placeholder = m_TopicDescriptionPlaceholder;

            m_SubmitButton.clicked -= OnSubmitAddTopic;
            m_SubmitButton.clicked += OnSubmitAddTopic;
        }

        public void ShowEditTopic(string title, string description)
        {
            Utils.Show(m_Container);
            Utils.Show(m_Title);
            m_Title.value = title;
            m_Message.value = description;
            m_Message.placeholder = m_TopicDescriptionPlaceholder;

            m_SubmitButton.clicked -= OnSubmitEditTopic;
            m_SubmitButton.clicked += OnSubmitEditTopic;
        }

        public void ShowAddComment()
        {
            Utils.Show(m_Container);
            Utils.Hide(m_Title);
            m_Message.value = string.Empty;
            m_Message.placeholder = m_CommentPlaceholder;

            m_SubmitButton.clicked -= OnSubmitAddComment;
            m_SubmitButton.clicked += OnSubmitAddComment;
        }

        public void ShowEditComment(string comment)
        {
            Utils.Show(m_Container);
            Utils.Hide(m_Title);
            m_Message.value = comment;
            m_Message.placeholder = m_CommentPlaceholder;

            m_SubmitButton.clicked -= OnSubmitEditComment;
            m_SubmitButton.clicked += OnSubmitEditComment;
        }

        public void Hide()
        {
            Utils.Hide(m_Container);
        }

        public void Clear()
        {
            ClearTextInput();

            Hide();
        }

        public void Initialize(VisualElement rootVisualElement)
        {
            m_Container = rootVisualElement.Q("TextInputContainer");
            m_Title = rootVisualElement.Q<TextField>("TextInputTitle");
            m_Message = rootVisualElement.Q<TextArea>("TextInputMessage");
            m_SubmitButton = rootVisualElement.Q<ActionButton>("TextInputSubmit");

            var textInputCancelButton = rootVisualElement.Q<ActionButton>("TextInputCancel");
            textInputCancelButton.clicked += OnCancelClicked;
            Utils.Hide(m_Container);
        }

        void OnCancelClicked()
        {
            CancelClicked?.Invoke();
        }

        void ClearTextInput()
        {
            m_Title.value = string.Empty;
            m_Message.value = string.Empty;
            m_SubmitButton.clicked -= OnSubmitAddTopic;
            m_SubmitButton.clicked -= OnSubmitEditTopic;
            m_SubmitButton.clicked -= OnSubmitAddComment;
            m_SubmitButton.clicked -= OnSubmitEditComment;
        }

        void OnSubmitAddTopic()
        {
            var titleText = m_Title.value;
            var descriptionText = m_Message.value;
            if (string.IsNullOrWhiteSpace(titleText))
            {
                m_AppMessaging.ShowWarning(m_NoTitleWarning);
            }
            else
            {
                AddTopicSubmitClicked?.Invoke(titleText, descriptionText);
                Clear();
            }
        }

        void OnSubmitEditTopic()
        {
            var titleText = m_Title.value;
            var descriptionText = m_Message.value;
            if (string.IsNullOrWhiteSpace(titleText))
            {
                m_AppMessaging.ShowWarning(m_NoTitleWarning);
            }
            else
            {
                EditTopicSubmitClicked?.Invoke(titleText, descriptionText);
                Clear();
            }
        }

        void OnSubmitAddComment()
        {
            var commentText = m_Message.value;
            if (string.IsNullOrWhiteSpace(commentText))
            {
                m_AppMessaging.ShowWarning(m_NoCommentWarning);
            }
            else
            {
                AddCommentSubmitClicked?.Invoke(commentText);
                Clear();
            }
        }

        void OnSubmitEditComment()
        {
            var commentText = m_Message.value;
            if (string.IsNullOrWhiteSpace(commentText))
            {
                m_AppMessaging.ShowWarning(m_NoCommentWarning);
            }
            else
            {
                EditCommentSubmitClicked?.Invoke(commentText);
                Clear();
            }
        }
    }
}

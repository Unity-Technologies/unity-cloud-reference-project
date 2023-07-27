using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Annotation;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.Common;
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

        [Header("Localization")]
        [SerializeField]
        string m_NoComment = "@Annotation:NoComment";

        [SerializeField]
        string m_Delete = "@Annotation:Delete";

        [SerializeField]
        string m_Edit = "@Annotation:Edit";

        public event Action BackClicked;
        public event Action AddClicked;
        public event Action GotoClicked;
        public event Action<IComment> CommentDeleted;
        public event Action<IComment> CommentEdited;

        VisualElement m_CommentsPanel;
        VisualElement m_CommentTopicContainer;
        VisualElement m_CommentContainer;
        ActionButton m_AddCommentButton;
        ActionButton m_GotoButton;
        ActionButton m_BackButton;

        UserInfo m_UserInfo;
        ITopic m_CurrentTopic;
        bool m_IsPanelEnabled;

        readonly List<VisualElement> m_MyCommentOptionButtons = new();

        IUserInfoProvider m_UserInfoProvider;

        [Inject]
        void Setup(IUserInfoProvider userInfoProvider)
        {
            m_UserInfoProvider = userInfoProvider;
        }

        void Start()
        {
            m_UserInfoProvider.GetUserInfoAsync().ContinueWith(task =>
            {
                m_UserInfo = task.Result;
            });
        }

        public void Initialize(VisualElement rootVisualElement)
        {
            m_CommentsPanel = rootVisualElement.Q("CommentList");
            m_CommentTopicContainer = rootVisualElement.Q("CommentTopicContainer");
            m_CommentContainer = rootVisualElement.Q("CommentListContainer");

            m_AddCommentButton = rootVisualElement.Q<ActionButton>("AddCommentButton");
            m_AddCommentButton.clicked += () => AddClicked?.Invoke();

            m_GotoButton = rootVisualElement.Q<ActionButton>("GotoButton");
            m_GotoButton.clicked += () => GotoClicked?.Invoke();

            m_BackButton = rootVisualElement.Q<ActionButton>("BackButton");
            m_BackButton.clicked += () => BackClicked?.Invoke();
        }

        public void Show()
        {
            Utils.Show(m_CommentsPanel);
        }

        public void Hide()
        {
            Utils.Hide(m_CommentsPanel);
        }

        public void EnablePanel(bool enable = true)
        {
            m_IsPanelEnabled = enable;

            m_AddCommentButton.SetEnabled(enable);
            m_GotoButton.SetEnabled(enable);
            m_BackButton.SetEnabled(enable);
            m_AddCommentButton.RemoveFromClassList("is-hovered");

            foreach (var optionButton in m_MyCommentOptionButtons)
            {
                optionButton.SetEnabled(enable);
            }
        }

        public void ShowComments(ITopic topic, IEnumerable<IComment> comments)
        {
            m_CommentTopicContainer.Clear();
            m_CommentContainer.Clear();
            m_MyCommentOptionButtons.Clear();

            var topicEntry = CreateTopicEntry(topic);
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
                    var commentUI = CreateCommentEntry(comment);
                    m_CommentContainer.Add(commentUI);

                    if (i == size - 1)
                    {
                        var divider = commentUI.Q("CommentEntryDivider");
                        Utils.Hide(divider);
                    }
                }
            }
        }

        VisualElement CreateCommentEntry(IComment comment)
        {
            var commentEntry = m_CommentEntryTemplate.Instantiate();

            UpdateCommentEntry(commentEntry, comment);

            var optionButton = commentEntry.Q<ActionButton>("CommentEntryOptionButton");
            if (comment.Author.Id != m_UserInfo?.Id)
            {
                optionButton.SetEnabled(false);
            }
            else
            {
                if (!m_IsPanelEnabled)
                {
                    optionButton.SetEnabled(false);
                }

                m_MyCommentOptionButtons.Add(optionButton);
            }

            optionButton.clicked += () =>
            {
                var contentView = new VisualElement();
                contentView.style.alignItems = Align.FlexStart;
                var popover = Popover.Build(optionButton, contentView);

                contentView.Add(Utils.OptionButton("delete", m_Delete, () =>
                {
                    popover.Dismiss();
                    CommentDeleted?.Invoke(comment);
                }));

                contentView.Add(Utils.OptionButton("pen", m_Edit, () =>
                {
                    popover.Dismiss();
                    CommentEdited?.Invoke(comment);
                }));

                popover.Show();
            };

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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class TopicPanelController : MonoBehaviour
    {
        [SerializeField]
        ColorPalette m_ColorPalette;

        [Header("UI Toolkit")]
        [SerializeField]
        VisualTreeAsset m_TopicEntryTemplate;

        [Header("Localization")]
        [SerializeField]
        string m_Delete = "@Annotation:Delete";

        [SerializeField]
        string m_Edit = "@Annotation:Edit";

        [SerializeField]
        string m_NoTopic = "@Annotation:NoTopic";

        public event Action<ITopic> TopicSelected;
        public event Action<ITopic> TopicDeleted;
        public event Action<ITopic> TopicEdited;

        VisualElement m_TopicPanel;
        VisualElement m_TopicContainer;
        VisualElement m_LastTopicEntry;

        UserInfo m_UserInfo;

        readonly Dictionary<Guid, VisualElement> m_TopicVisualElements = new();

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
            m_TopicPanel = rootVisualElement.Q("TopicList");
            m_TopicContainer = rootVisualElement.Q("TopicListContainer");
        }

        public void Show()
        {
            Utils.Show(m_TopicPanel);
        }

        public void Hide()
        {
            Utils.Hide(m_TopicPanel);
        }

        public void EnablePanel(bool enable = true)
        {
            foreach (var topicEntry in m_TopicVisualElements.Values)
            {
                topicEntry.SetEnabled(enable);
                topicEntry.RemoveFromClassList("is-hovered");
            }
        }

        public void RefreshTopic(IEnumerable<ITopic> topics)
        {
            m_TopicContainer.Clear();
            m_TopicVisualElements.Clear();
            var size = topics.Count();
            if (size == 0)
            {
                AddNoTopicMessage();
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    var topic = topics.ElementAt(i);
                    var topicEntry = CreateTopicEntry(topic);
                    m_TopicContainer.Add(topicEntry);
                    m_TopicVisualElements.Add(topic.Id, topicEntry);

                    if (i == size - 1)
                    {
                        m_LastTopicEntry = topicEntry;
                        var divider = topicEntry.Q("TopicEntryDivider");
                        Utils.Hide(divider);
                    }
                }
            }
        }

        public bool IsTopicExist(ITopic topic)
        {
            return m_TopicVisualElements.ContainsKey(topic.Id);
        }

        public void AddTopicEntry(ITopic topic)
        {
            if (!m_TopicVisualElements.Any())
            {
                // Remove no topic message
                m_TopicContainer.Clear();
            }

            var topicEntry = CreateTopicEntry(topic);
            m_TopicContainer.Add(topicEntry);
            m_TopicVisualElements.Add(topic.Id, topicEntry);

            // Update dividers
            if (m_LastTopicEntry != null)
            {
                var divider = m_LastTopicEntry.Q("TopicEntryDivider");
                Utils.Show(divider);
                m_LastTopicEntry = topicEntry;
                divider = topicEntry.Q("TopicEntryDivider");
                Utils.Hide(divider);
            }
        }

        public void RemoveTopicEntry(Guid topicId)
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
                        m_LastTopicEntry = m_TopicContainer.ElementAt(size - 1);
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

        void AddNoTopicMessage()
        {
            var noTopic = new Text(m_NoTopic);
            noTopic.AddToClassList("topic-no-topic");
            m_TopicContainer.Add(noTopic);
        }

        VisualElement CreateTopicEntry(ITopic topic)
        {
            var topicEntry = m_TopicEntryTemplate.Instantiate();
            var topicContainer = topicEntry.Q("TopicEntryContainer");
            var optionButton = topicEntry.Q<ActionButton>("TopicEntryOptionButton");

            var color = m_ColorPalette.GetColor(topic.CreationAuthor.ColorIndex);
            Utils.UpdateTopicEntry(topicEntry, topic, color);

            UpdateComments(topic, topicContainer).ConfigureAwait(false);

            topicContainer.focusable = true;
            topicContainer.AddManipulator(new Pressable());

            if (topic.CreationAuthor.Id != m_UserInfo?.Id)
            {
                optionButton.SetEnabled(false);
            }

            optionButton.clicked += () =>
            {
                var contentView = new VisualElement();
                contentView.style.alignItems = Align.FlexStart;
                var popover = Popover.Build(optionButton, contentView);

                contentView.Add(Utils.OptionButton("delete", m_Delete, () =>
                {
                    TopicDeleted?.Invoke(topic);
                    popover.Dismiss();
                }));

                contentView.Add(Utils.OptionButton("pen", m_Edit, () =>
                {
                    popover.Dismiss();
                    TopicEdited?.Invoke(topic);
                }));

                popover.Show();
            };

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
                TopicSelected?.Invoke(topic);
            });
        }

        async Task UpdateReplyLabel(ITopic topic, Text reply)
        {
            var comments = await topic.GetCommentsAsync();
            var count = comments.Count();

            if (count > 0)
            {
                Utils.Show(reply);
                var localisedText = reply.Q<LocalizedTextElement>();
                localisedText.variables = new object[] { count };
            }
            else
            {
                Utils.Hide(reply);
            }
        }
    }
}

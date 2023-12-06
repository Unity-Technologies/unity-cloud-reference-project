using System;
using Unity.AppUI.UI;
using Unity.Cloud.Annotation;
using UnityEngine;
using UnityEngine.UIElements;
using Avatar = Unity.AppUI.UI.Avatar;

namespace Unity.ReferenceProject.Annotation
{
    public static class Utils
    {
        public static void Show(VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.Flex;
        }

        public static void Hide(VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.None;
        }

        public static VisualElement OptionButton(VisualTreeAsset template, string icon, string text, Action action)
        {
            var instance = template.Instantiate();
            var button = instance.Q<ActionButton>();
            button.icon = icon;
            button.label = text;
            button.clicked += () =>
            {
                action?.Invoke();
            };
            return instance;
        }

        public static void UpdateEntryHeader(Avatar avatar, Text authorText, Author author, Color color, Text dateText, DateTime date)
        {
            var avatarInitials = avatar.Q<Text>();
            avatar.outlineColor = Color.clear;
            avatarInitials.text = Common.Utils.GetInitials(author.FullName);
            avatar.backgroundColor = color;

            authorText.text = author.FullName;

            dateText.text = Common.Utils.GetTimeIntervalSinceNow(date.ToLocalTime(), out var variables);
            var localizedTextElement = dateText.Q<LocalizedTextElement>();
            localizedTextElement.variables = variables;
            dateText.tooltip = date.ToString();
        }

        public static void UpdateTopicEntry(VisualElement topicEntry, ITopic topic, Color color)
        {
            var avatar = topicEntry.Q<Avatar>();
            var topicTitle = topicEntry.Q<Text>("TopicEntryTitle");
            var topicDescription = topicEntry.Q<Text>("TopicEntryDescription");
            var topicDate = topicEntry.Q<Text>("TopicEntryDate");
            var topicAuthor = topicEntry.Q<Text>("TopicEntryAuthor");

            UpdateEntryHeader(avatar, topicAuthor, topic.CreationAuthor, color, topicDate, topic.CreationDate);

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
    }
}

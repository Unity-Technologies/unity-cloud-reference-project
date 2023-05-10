using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Settings
{
    public interface IGlobalSettings
    {
        bool IsDirty { get; }
        VisualElement CreateVisualTree();
        void RefreshSettingsEnableState();
        void AddSetting(ISetting setting);
        void RemoveSetting(ISetting setting);
    }

    public interface ISetting
    {
        bool IsEnabled { get; }
        VisualElement CreateVisualTree();
    }

    public static class GlobalSettingsStyleClasses
    {
        public static readonly string EntryStyle = "global-setting-entry";
        public static readonly string EntryHeaderStyle = "setting-entry-header";
        public static readonly string EntryValueStyle = "setting-entry-value";
    }

    [Serializable]
    public class GlobalSettings : IGlobalSettings
    {
        VisualElement m_RootVisualElement;
        Dictionary<ISetting, VisualElement> m_Settings = new();
        public bool IsDirty { get; private set; }

        public void AddSetting(ISetting setting)
        {
            m_Settings.Add(setting, null);
            IsDirty = true;
        }

        public void RemoveSetting(ISetting setting)
        {
            m_Settings.Remove(setting);
            IsDirty = true;
        }

        public VisualElement CreateVisualTree()
        {
            if (!IsDirty && m_RootVisualElement != null)
            {
                return m_RootVisualElement;
            }

            IsDirty = false;

            if (m_RootVisualElement == null)
            {
                m_RootVisualElement = new VisualElement();
            }
            else
            {
                m_RootVisualElement.Clear();
            }

            foreach (var setting in m_Settings.Keys.ToArray())
            {
                var element = setting.CreateVisualTree();
                element.AddToClassList(GlobalSettingsStyleClasses.EntryStyle);
                m_RootVisualElement.Add(element);

                m_Settings[setting] = element;
            }

            return m_RootVisualElement;
        }

        public void RefreshSettingsEnableState()
        {
            foreach (var setting in m_Settings)
            {
                setting.Value?.SetEnabled(setting.Key.IsEnabled);
            }
        }
    }
}

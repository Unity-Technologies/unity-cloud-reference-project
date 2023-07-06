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
        void AddSetting(ISetting setting, uint order = 0);
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
        [Serializable]
        struct SettingEntry
        {
            public VisualElement Element;
            public uint Order;
        }

        VisualElement m_RootVisualElement;
        Dictionary<ISetting, SettingEntry> m_Settings = new();
        public bool IsDirty { get; private set; }

        public void AddSetting(ISetting setting, uint order = 0)
        {
            m_Settings.Add(setting, new SettingEntry{ Element = null, Order = order });
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

            var settings = m_Settings.Keys.OrderByDescending(s => m_Settings[s].Order);
            foreach (var setting in settings)
            {
                var element = setting.CreateVisualTree();
                element.AddToClassList(GlobalSettingsStyleClasses.EntryStyle);
                m_RootVisualElement.Add(element);

                var settingEntryCopy = m_Settings[setting];
                settingEntryCopy.Element = element;
                m_Settings[setting] = settingEntryCopy;
            }

            return m_RootVisualElement;
        }

        public void RefreshSettingsEnableState()
        {
            foreach (var setting in m_Settings)
            {
                setting.Value.Element?.SetEnabled(setting.Key.IsEnabled);
            }
        }
    }
}

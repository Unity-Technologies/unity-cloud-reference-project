using System;
using System.Collections.Generic;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Core
{
    /// <summary>
    /// The main manager for the AppUI system.
    /// This class is responsible for managing the main looper, the notification manager and the settings.
    /// It also provides access to them.
    /// </summary>
    public class AppUIManager
    {
        // ReSharper disable once InconsistentNaming
        internal AppUISettings m_Settings;

        Looper m_MainLooper;

        readonly Dictionary<PanelSettings, HashSet<Panel>> m_PanelSettings = new Dictionary<PanelSettings, HashSet<Panel>>();

        NotificationManager m_NotificationManager;

        internal AppUISettings defaultSettings { get; private set; }

        /// <summary>
        /// The current settings.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the settings are not ready.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the provided settings object is null.</exception>
        internal AppUISettings settings
        {
            get
            {
                if (m_Settings)
                    return m_Settings;
                
                if (!defaultSettings)
                    defaultSettings = ScriptableObject.CreateInstance<AppUISettings>();
                
                return defaultSettings;
            }
            set
            {
                if (!value)
                    throw new ArgumentNullException(nameof(value));

                if (m_Settings == value)
                    return;

                m_Settings = value;
                ApplySettings();
            }
        }

        /// <summary>
        /// The main looper.
        /// </summary>
        internal Looper mainLooper => m_MainLooper;

        /// <summary>
        /// The notification manager.
        /// </summary>
        internal NotificationManager notificationManager => m_NotificationManager;

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal AppUIManager()
        {

        }

        /// <summary>
        /// Initializes the AppUIManager.
        /// </summary>
        /// <param name="newSettings">The settings to use.</param>
        internal void Initialize(AppUISettings newSettings)
        {
            Debug.Assert(newSettings, "The provided settings object can't be null");

            m_Settings = newSettings;
            m_MainLooper = new Looper();
            m_NotificationManager = new NotificationManager(this);

            m_MainLooper.Loop();

            ApplySettings();
        }

        /// <summary>
        /// Shutdown the AppUIManager.
        /// </summary>
        internal void Shutdown()
        {
            m_MainLooper.Quit();
        }

        /// <summary>
        /// Applies the current settings.
        /// </summary>
        internal void ApplySettings()
        {

        }

        /// <summary>
        /// Update method that should be called every frame.
        /// </summary>
        internal void Update()
        {
            Platform.PollSystemTheme();

            if (m_PanelSettings is { Count: > 0 } && m_Settings.autoCorrectUiScale)
            {
                var dpi = Platform.referenceDpi;
                foreach (var panelSettings in m_PanelSettings.Keys)
                {
                    if (panelSettings)
                    {
                        if (!Mathf.Approximately(panelSettings.referenceDpi, dpi))
                        {
                            var previousValue = panelSettings.referenceDpi;
                            panelSettings.referenceDpi = dpi;
                            foreach (var panel in m_PanelSettings[panelSettings])
                            {
                                using var evt = DpiChangedEvent.GetPooled();
                                evt.previousValue = previousValue;
                                evt.newValue = dpi;
                                evt.target = panel;
                                panel.SendEvent(evt);
                            }
                        }
                        
                        if (Input.GetMouseButtonDown(0))
                        {
                            foreach (var panel in m_PanelSettings[panelSettings])
                            {
                                if (panel is {panel: { }})
                                {
                                    var mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
                                    var coord = RuntimePanelUtils.ScreenToPanel(panel.panel, mousePosition);
                                    var picked = panel.panel.Pick(coord);
                                    if (picked == null)
                                        panel.DismissAnyPopups(DismissType.OutOfBounds);
                                }
                            }
                        }
                    }
                }
            }

            m_MainLooper.LoopOnce();
        }

        /// <summary>
        /// Registers a panel.
        /// </summary>
        /// <param name="element">The panel to register.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided panel is null.</exception>
        internal void RegisterPanel(Panel element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var panelSettings = element.panel?.GetPanelSettings();

            if (panelSettings == null)
                return;

            if (m_PanelSettings.ContainsKey(panelSettings))
                m_PanelSettings[panelSettings].Add(element);
            else
                m_PanelSettings.Add(panelSettings, new HashSet<Panel> { element });
        }

        /// <summary>
        /// Unregisters a panel.
        /// </summary>
        /// <param name="element">The panel to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided panel is null.</exception>
        internal void UnregisterPanel(Panel element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var panelSettings = element.panel?.GetPanelSettings();

            if (panelSettings == null)
                return;

            if (m_PanelSettings.ContainsKey(panelSettings))
            {
                m_PanelSettings[panelSettings].Remove(element);
                if (m_PanelSettings[panelSettings].Count == 0)
                    m_PanelSettings.Remove(panelSettings);
            }
        }
    }
}

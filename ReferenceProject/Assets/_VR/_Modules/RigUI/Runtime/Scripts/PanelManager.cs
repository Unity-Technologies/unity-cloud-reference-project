using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR.RigUI
{
    public interface IPanelManager
    {
        public T CreatePanel<T>(Vector2 size, VisualTreeAsset visualTree = null) where T : PanelController;
        public T SwapPanelType<T>(PanelController panel) where T : PanelController;
        public List<PanelController> Panels { get; }
        public void DestroyPanel(PanelController panel);
        public Action<PanelController> OnPanelCreated { get; set; }
        public void BlockPanels(bool block=true);
    }

    public class PanelManager : MonoBehaviour, IPanelManager
    {
        [SerializeField]
        List<PanelController> m_PanelPrefabs = new ();

        [SerializeField]
        VisualTreeAsset m_AppUIVisualTreeAsset;

        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        Transform m_Root;

        [SerializeField]
        ThemeStyleSheet m_ThemeStyleSheet;

        readonly List<PanelController> m_PanelControllers = new ();
        public List<PanelController> Panels => m_PanelControllers;

        public Transform Root
        {
            get => m_Root;
            set => m_Root = value;
        }

        public Action<PanelController> OnPanelCreated { get; set; }

        DiContainer m_DiContainer;

        [Inject]
        void Setup(DiContainer diContainer)
        {
            m_DiContainer = diContainer;
        }

        void Awake()
        {
            if (m_Camera == null)
            {
                m_Camera = Camera.main;
            }
        }

        public T CreatePanel<T>(Vector2 size, VisualTreeAsset visualTree = null) where T : PanelController
        {
            if (visualTree == null)
            {
                visualTree = m_AppUIVisualTreeAsset;
            }

            var prefab = (T)m_PanelPrefabs.FirstOrDefault(p => p is T);

            if (prefab == default)
            {
                Debug.LogError($"Panel prefab of type {typeof(T)} not found.");
                return null;
            }

            var panel = m_DiContainer.InstantiatePrefabForComponent<T>(prefab, m_Root);
            if (size.x > 0 && size.y > 0)
            {
                panel.UIDocument.panelSettings.themeStyleSheet = m_ThemeStyleSheet;
                panel.XRCamera = m_Camera;
                panel.PanelSize = size;
                panel.VisualTreeAsset = visualTree;
            }
            else
            {
                panel.UIDocument.gameObject.SetActive(false);
            }

            Panels.Add(panel);
            OnPanelCreated?.Invoke(panel);

            return panel;
        }

        public T SwapPanelType<T>(PanelController panel) where T : PanelController
        {
            var newPanel = CreatePanel<T>(panel.PanelSize, panel.VisualTreeAsset);
            DestroyPanel(panel);

            return newPanel;
        }

        public void DestroyPanel(PanelController panel)
        {
            Panels.Remove(panel);
            Destroy(panel.gameObject);
        }

        public void BlockPanels(bool block=true)
        {
            foreach (var panel in Panels)
            {
                var appUIPanel = panel.Root.Q<Panel>();
                var blocker = appUIPanel.popupContainer.Q("blocker");
                if (block)
                {
                    // Check if a blocker already exists to avoid creating multiple blockers
                    if (blocker == null)
                    {
                        blocker = new VisualElement { name = "blocker" };
                        blocker.style.position = Position.Absolute;
                        blocker.style.left = blocker.style.right = blocker.style.top = blocker.style.bottom = 0;
                        blocker.style.backgroundColor = Color.clear;
                        appUIPanel.popupContainer.Add(blocker);
                    }
                }
                else if(blocker != null)
                {
                    appUIPanel.popupContainer.Remove(blocker);
                }
            }
        }
    }
}

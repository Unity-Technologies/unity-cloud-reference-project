using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.VR.RigUI
{
    public interface IPanelManager
    {
        public T CreatePanel<T>(Vector2 size, VisualTreeAsset visualTree = null) where T : PanelController;
        public T SwapPanelType<T>(PanelController panel) where T : PanelController;
        public List<PanelController> Panels { get; }
        public void DestroyPanel(PanelController panel);
        public Action<PanelController> OnPanelCreated { get; set; }
    }

    public class PanelManager : MonoBehaviour, IPanelManager
    {
        [SerializeField]
        List<PanelController> m_PanelPrefabs = new List<PanelController>();

        [SerializeField]
        VisualTreeAsset m_AppUIVisualTreeAsset;

        [SerializeField]
        Camera m_Camera;

        [SerializeField]
        Transform m_Root;

        [SerializeField]
        ThemeStyleSheet m_ThemeStyleSheet;

        [SerializeField]
        Material m_MaterialOpaque;

        [SerializeField]
        Material m_MaterialTransparent;

        readonly List<PanelController> m_PanelControllers = new List<PanelController>();
        public List<PanelController> Panels => m_PanelControllers;

        public Transform Root
        {
            get => m_Root;
            set => m_Root = value;
        }

        public Action<PanelController> OnPanelCreated { get; set; }

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

            var panel = Instantiate(prefab, m_Root);
            if (size.x > 0 && size.y > 0)
            {
                panel.WorldSpaceUIToolkit.OpaqueMaterial = m_MaterialOpaque;
                panel.WorldSpaceUIToolkit.TransparentMaterial = m_MaterialTransparent;
                panel.WorldSpaceUIToolkit.PanelSettingsPrefab.themeStyleSheet = m_ThemeStyleSheet;
                panel.XRCamera = m_Camera;
                panel.PanelSize = size;
                panel.VisualTreeAsset = visualTree;
            }
            else
            {
                panel.WorldSpaceUIToolkit.gameObject.SetActive(false);
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
            DestroyImmediate(panel.gameObject);
        }
    }
}

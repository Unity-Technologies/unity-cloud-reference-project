using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR.RigUI
{
    public interface IRigUIController
    {
        public Transform DockPoint { get; }
        public Transform PermanentDockPoint { get; }
        public void InitMainBar(List<ActionButton> buttons);
        public void ClearMainBar();
        public void AddSecondaryBar(List<ActionButton> buttons);
        public void ClearSecondaryBar();
        public PanelController DockButtonClicked(PanelController panelController, Transform dockPoint, Vector3 offset);
    }

    public class RigUIController : MonoBehaviour, IRigUIController
    {
        [SerializeField]
        Camera m_XRCamera;

        [SerializeField]
        InputActionReference m_Reset;

        [SerializeField]
        Transform m_DockPoint;

        [SerializeField]
        Transform m_PermanentDockPoint;

        [SerializeField]
        Vector3 m_DockPointOffset;

        [SerializeField]
        float m_ButtonSize = 76f;

        [SerializeField]
        float m_DistanceFace = 1f;

        [SerializeField]
        float m_HeightDelta = -0.2f;

        IPanelManager m_PanelManager;
        FloatingPanelController m_RigUIBarPanel;
        VisualElement m_MainBar;
        VisualElement m_SecondaryBar;
        InputAction m_ResetAction;

        Vector3 m_LastResetPosition;
        int m_MaxSecondaryBarButton;
        List<ActionButton> m_MainBarButtons;
        List<ActionButton> m_SecondaryBarButtons;
        readonly List<FloatingPanelController> m_FloatingPanels = new();

        public Transform DockPoint => m_DockPoint;
        public Transform PermanentDockPoint => m_PermanentDockPoint;

        [Inject]
        public void Setup(IPanelManager panelManager)
        {
            m_PanelManager = panelManager;
        }

        void Awake()
        {
            m_ResetAction = m_Reset.action;
            m_ResetAction.performed += OnReset;
        }

        void OnDestroy()
        {
            m_ResetAction.performed -= OnReset;
        }

        void OnEnable()
        {
            m_ResetAction.Enable();

            IEnumerator WaitVRHeadsetReady()
            {
                while (m_XRCamera.transform.localPosition == Vector3.zero)
                {
                    yield return null;
                }

                ResetPosition();
            }

            StartCoroutine(WaitVRHeadsetReady());
        }

        void OnDisable()
        {
            m_ResetAction.Disable();
        }

        public void ResetPosition()
        {
            m_LastResetPosition = ComputeInFacePosition();
            MoveAtPosition(m_LastResetPosition);
        }

        public void InitMainBar(List<ActionButton> buttons)
        {
            m_MainBarButtons = buttons;

            // Compute the max width of the bar
            var nbButton = Mathf.Max(buttons.Count, m_MaxSecondaryBarButton);
            nbButton = nbButton > 1 ? nbButton : 2; // At least two button width to avoid tooltip to be cut.

            // Create main bar panel
            m_RigUIBarPanel = m_PanelManager.CreatePanel<FloatingPanelController>(new Vector2(nbButton * m_ButtonSize, m_ButtonSize * 2));
            OnPanelBuilt(m_RigUIBarPanel.UIDocument);

            MoveAtPosition(m_LastResetPosition);
        }

        public void ClearMainBar()
        {
            if (m_RigUIBarPanel != null)
            {
                ClearSecondaryBar();

                m_MainBarButtons = null;
                m_MaxSecondaryBarButton = 0;
                m_LastResetPosition = m_RigUIBarPanel.transform.localPosition;

                m_DockPoint.transform.SetParent(transform, true);

                foreach (Transform child in m_DockPoint)
                {
                    var panelController = child.GetComponent<PanelController>();
                    if (panelController != null)
                    {
                        m_PanelManager.DestroyPanel(panelController);
                    }
                    else
                    {
                        Destroy(child.gameObject);
                    }
                }

                foreach (var floatingPanel in m_FloatingPanels)
                {
                    m_PanelManager.DestroyPanel(floatingPanel);
                }

                m_PermanentDockPoint.transform.SetParent(transform, true);
                m_PanelManager.DestroyPanel(m_RigUIBarPanel);
            }
        }

        public void AddSecondaryBar(List<ActionButton> buttons)
        {
            m_SecondaryBarButtons = buttons;
            if (m_SecondaryBarButtons.Count > m_MaxSecondaryBarButton)
            {
                m_MaxSecondaryBarButton = buttons.Count;
                if (m_RigUIBarPanel != null)
                {
                    m_LastResetPosition = m_RigUIBarPanel.transform.localPosition;
                    m_DockPoint.transform.SetParent(transform, true);
                    m_PermanentDockPoint.transform.SetParent(transform, true);
                    m_PanelManager.DestroyPanel(m_RigUIBarPanel);
                }

                if (m_MainBarButtons != null)
                {
                    InitMainBar(m_MainBarButtons);
                }
            }
            else
            {
                if (m_SecondaryBar != null)
                {
                    foreach (var button in m_SecondaryBarButtons)
                    {
                        m_SecondaryBar.Add(button);
                    }

                    m_SecondaryBar.style.display = DisplayStyle.Flex;
                    ResetDockPointPosition();
                }
            }
        }

        public void ClearSecondaryBar()
        {
            if (m_SecondaryBar != null)
            {
                if (m_SecondaryBarButtons != null)
                {
                    foreach (var button in m_SecondaryBarButtons)
                    {
                        m_SecondaryBar.Remove(button);
                    }
                }

                m_SecondaryBar.style.display = DisplayStyle.None;
            }

            m_SecondaryBarButtons = null;
            ResetDockPointPosition();
        }

        public PanelController DockButtonClicked(PanelController panelController, Transform dockPoint, Vector3 offset)
        {
            PanelController newPanelController;
            if (panelController is FloatingPanelController floatingPanel)
            {
                m_FloatingPanels.Remove(floatingPanel);

                var dockedPanel = m_PanelManager.SwapPanelType<DockedPanelController>(panelController);
                dockedPanel.DockPoint = dockPoint;
                dockedPanel.transform.localPosition = offset;

                newPanelController = dockedPanel;
            }
            else
            {
                floatingPanel = m_PanelManager.SwapPanelType<FloatingPanelController>(panelController);
                floatingPanel.transform.position = dockPoint.position +
                    offset.x * dockPoint.right +
                    offset.y * dockPoint.up +
                    offset.z * dockPoint.forward;

                newPanelController = floatingPanel;
                m_FloatingPanels.Add(floatingPanel);
            }

            return newPanelController;
        }

        void MoveAtPosition(Vector3 position)
        {
            if (m_RigUIBarPanel != null)
            {
                m_DockPoint.SetParent(m_RigUIBarPanel.transform, false);
                m_PermanentDockPoint.SetParent(m_RigUIBarPanel.transform, false);
                ResetDockPointPosition();

                m_RigUIBarPanel.transform.localPosition = position;
                m_RigUIBarPanel.TurnToFace();
            }
            else
            {
                m_PermanentDockPoint.localPosition = position;
                m_DockPoint.localPosition = position;
            }
        }

        void ResetDockPointPosition()
        {
            var position = m_DockPointOffset +
                (((m_MainBarButtons != null && m_MainBarButtons.Count > 0 ? 0f : -1f) +
                        (m_SecondaryBarButtons != null && m_SecondaryBarButtons.Count > 0 ? 1f : 0f))
                    * m_ButtonSize / 1000f) * Vector3.up;

            if (m_DockPoint != null)
            {
                m_DockPoint.localPosition = position;
                m_DockPoint.localRotation = Quaternion.identity;
            }

            if (m_PermanentDockPoint != null)
            {
                m_PermanentDockPoint.localPosition = position;
                m_PermanentDockPoint.localRotation = Quaternion.identity;
            }
        }

        void OnPanelBuilt(UIDocument document)
        {
            var root = document.rootVisualElement;
            var appUIPanel = root.Q<Panel>();

            ConstructMainBar(appUIPanel);
        }

        void ConstructMainBar(VisualElement root)
        {
            VisualElement container = new VisualElement
            {
                name = "container"
            };
            container.AddToClassList("rigui-container");
            root.Add(container);

            VisualElement rigUIBar = new VisualElement
            {
                name = "rigui-bar"
            };
            rigUIBar.AddToClassList("rigui-bar");
            container.Add(rigUIBar);

            m_SecondaryBar = new VisualElement
            {
                name = "rigui-secondary-bar"
            };
            m_SecondaryBar.AddToClassList("rigui-secondary-bar");
            m_SecondaryBar.style.display = m_SecondaryBarButtons == null || m_SecondaryBarButtons.Count == 0 ? DisplayStyle.None : DisplayStyle.Flex;
            rigUIBar.Add(m_SecondaryBar);

            m_MainBar = new VisualElement
            {
                name = "rigui-main-bar"
            };
            m_MainBar.AddToClassList("rigui-main-bar");
            rigUIBar.Add(m_MainBar);

            AddButtons();
        }

        void AddButtons()
        {
            foreach (var button in m_MainBarButtons)
            {
                m_MainBar.Add(button);
            }

            if (m_SecondaryBarButtons != null)
            {
                foreach (var button in m_SecondaryBarButtons)
                {
                    m_SecondaryBar.Add(button);
                }
            }
        }

        Vector3 ComputeInFacePosition()
        {
            return m_XRCamera.transform.localPosition + m_DistanceFace * Vector3.Scale(m_XRCamera.transform.forward, new Vector3(1f, 0, 1f)).normalized + m_HeightDelta * Vector3.up;
        }

        void OnReset(InputAction.CallbackContext obj)
        {
            ResetPosition();
        }
    }
}

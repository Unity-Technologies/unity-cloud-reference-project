using System.Collections;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.CustomKeyboard;
using Unity.ReferenceProject.VR.RigUI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class VRKeyboardController : MonoBehaviour
    {
        [SerializeField]
        Vector2 m_KeyboardSize = new(425, 250);

        [SerializeField]
        float m_DistanceFace = 0.9f;

        [SerializeField]
        float m_HeightDelta = -0.35f;

        [SerializeField]
        bool m_VerticalOnly;

        [SerializeField]
        InputActionReference m_Reset;

        FloatingPanelController m_FloatingPanelController;
        FloatingPanelManipulator m_FloatingPanelManipulator;
        InputAction m_ResetAction;

        IKeyboardController m_KeyboardController;
        IPanelManager m_PanelManager;
        ICameraProvider m_CameraProvider;

        bool m_IsEntered;

        [Inject]
        void Setup(IKeyboardController keyboardController, IPanelManager panelManager, ICameraProvider xrCamera)
        {
            m_KeyboardController = keyboardController;
            m_PanelManager = panelManager;
            m_CameraProvider = xrCamera;
        }

        void Awake()
        {
            var keyboard = m_KeyboardController.RootVisualElement;
            keyboard.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            keyboard.RegisterCallback<PointerEnterEvent>(OnPointerEnter);

            m_FloatingPanelController = m_PanelManager.CreatePanel<FloatingPanelController>(m_KeyboardSize);
            var root = m_FloatingPanelController.UIDocument.rootVisualElement;
            var appUIPanel = root.Q<Panel>();
            appUIPanel.Add(keyboard);
            m_FloatingPanelController.PanelManipulator.OnlyVerticalAxis = m_VerticalOnly;
            m_FloatingPanelController.SetVisible(false);

            ResetPosition();

            m_KeyboardController.KeyboardOpened += OnKeyboardOpen;
            m_KeyboardController.KeyboardClosed += OnKeyboardClose;

            m_ResetAction = m_Reset.action;
            m_ResetAction.performed += OnReset;
        }

        void OnEnable()
        {
            m_ResetAction.Enable();

            IEnumerator WaitVRHeadsetReady()
            {
                while (m_CameraProvider.Camera.transform.localPosition == Vector3.zero)
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

        void OnReset(InputAction.CallbackContext obj)
        {
            ResetPosition();
        }

        void OnDestroy()
        {
            m_ResetAction.performed -= OnReset;
            m_KeyboardController.KeyboardOpened -= OnKeyboardOpen;
            m_KeyboardController.KeyboardClosed -= OnKeyboardClose;
        }

        void OnKeyboardOpen()
        {
            m_FloatingPanelController.SetVisible(true);
        }

        void OnKeyboardClose()
        {
            if (!m_IsEntered)
            {
                var textField = m_KeyboardController.TextField;

                StartCoroutine(Common.Utils.WaitAFrame(tf =>
                {
                    if (m_FloatingPanelController.PanelManipulator.IsDragging)
                    {
                        m_KeyboardController.ForceTextFieldFocus();
                    }
                    else if(tf == m_KeyboardController.TextField) // If the keyboard is closed by selecting another text field, don't close the panel
                    {
                        m_FloatingPanelController.SetVisible(false);
                    }
                }, textField));
            }
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            m_IsEntered = true;
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            m_IsEntered = false;
        }

        void ResetPosition()
        {
            var xrCameraTransform = m_CameraProvider.Camera.transform;
            
            m_FloatingPanelController.transform.localPosition = xrCameraTransform.localPosition +
                m_DistanceFace * Vector3.Scale(xrCameraTransform.forward, new Vector3(1f, 0, 1f)).normalized +
                m_HeightDelta * Vector3.up;
            m_FloatingPanelController.TurnToFace();
        }
    }
}

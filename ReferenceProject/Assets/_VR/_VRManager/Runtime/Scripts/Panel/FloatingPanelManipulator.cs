using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.ReferenceProject.VRManager
{
    public class FloatingPanelManipulator : MonoBehaviour
    {
        [SerializeField]
        Transform m_FloatingPanel;

        [SerializeField]
        RectTransform m_ManipulatorRectTransform;

        [SerializeField]
        WorldSpaceUIDocument m_TargetPanel;

        [SerializeField]
        Handle m_Handle;

        [SerializeField]
        Transform m_FaceTarget;

        [SerializeField]
        bool m_IsFaceTarget;

        [SerializeField]
        bool m_OnlyVerticalAxis;

        [SerializeField, Range(0f, 0.999f)]
        float m_Resistance = 0.85f;

        bool m_DragInProcess;
        CanvasScaler m_ManipulatorCanvasScaler;
        Vector3 m_TargetPosition;
        Vector3 m_TargetRotation;
        Coroutine m_UpdateCoroutine;

        public bool IsFaceTarget
        {
            get => m_IsFaceTarget;
            set => m_IsFaceTarget = value;
        }

        public Transform FaceTarget
        {
            get => m_FaceTarget;
            set
            {
                m_FaceTarget = value;
                TurnToFace();
            }
        }

        void Awake()
        {
            m_Handle.DragStarted += OnHandleDragStarted;
            m_Handle.Dragging += OnHandleDragging;
            m_Handle.DragEnded += OnHandleDragEnded;
        }

        void OnEnable()
        {
            UpdateSize();

            IEnumerator WaitAFrame()
            {
                yield return null;
                TurnToFace();
            }

            StartCoroutine(WaitAFrame());
        }

        void OnDestroy()
        {
            m_Handle.DragStarted -= OnHandleDragStarted;
            m_Handle.Dragging -= OnHandleDragging;
            m_Handle.DragEnded -= OnHandleDragEnded;
        }

        public void UpdateSize()
        {
            m_ManipulatorRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_TargetPanel.PanelWidth);
            m_ManipulatorRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_TargetPanel.PanelHeight);
        }

        public void TurnToFace()
        {
            if (m_IsFaceTarget && m_FaceTarget != null)
            {
                var forward = m_FloatingPanel.position - m_FaceTarget.position;
                if (m_OnlyVerticalAxis)
                {
                    forward.y = 0f;
                }

                var angle = m_OnlyVerticalAxis ? 90f : Vector3.Angle(forward, Vector3.up);
                if (angle > 5f && angle < 175f) // Avoid problem when forward is near vertical
                {
                    m_FloatingPanel.rotation = forward.sqrMagnitude > float.Epsilon ? Quaternion.LookRotation(forward) : Quaternion.identity;
                }
            }
        }

        void OnHandleDragStarted()
        {
            m_DragInProcess = true;
            if (m_UpdateCoroutine == null)
            {
                m_TargetPosition = m_FloatingPanel.position;
                m_UpdateCoroutine = StartCoroutine(UpdateCoroutine());
            }
        }

        void OnHandleDragging(Vector3 delta)
        {
            m_TargetPosition += delta * (1f - m_Resistance);
        }

        void OnHandleDragEnded()
        {
            m_DragInProcess = false;
        }

        IEnumerator UpdateCoroutine()
        {
            while (m_FloatingPanel.position != m_TargetPosition || m_DragInProcess)
            {
                m_FloatingPanel.position = m_TargetPosition;
                TurnToFace();
                yield return null;
            }

            m_UpdateCoroutine = null;
        }
    }
}

using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.UIPanel;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasureSegment : MonoBehaviour
    {
        [SerializeField]
        MeasureLine m_MeasureLine = new ();

        [SerializeField]
        MeasureLabel m_MeasureLabel = new ();
        
        [SerializeField]
        Transform m_StartPoint;
        
        [SerializeField]
        Transform m_EndPoint;

        ICameraProvider m_CameraProvider;
        readonly List<Renderer> m_PointRenderers = new ();

        public Vector3 StartPosition
        {
            get => m_StartPoint.position;
            set => m_StartPoint.position = value;
        }

        public Vector3 EndPosition
        {
            get => m_EndPoint.position;
            set => m_EndPoint.position = value;
        }

        public void SetLabelText(string text)
        {
            m_MeasureLabel.Text = text;
        }
        
        public void SetLabelVisible(bool visible)
        {
            m_MeasureLabel.SetLabelVisible(visible);
        }

        [Inject]
        public void Setup(ICameraProvider cameraProvider, IMainUIPanel mainUIPanel)
        {
            m_CameraProvider = cameraProvider;
            mainUIPanel.Panel.Insert(0, m_MeasureLabel.CreateVisualElement());
        }

        void Awake()
        {
            m_PointRenderers.AddRange(m_StartPoint.GetComponentsInChildren<Renderer>());
            m_PointRenderers.AddRange(m_EndPoint.GetComponentsInChildren<Renderer>());
        }

        void LateUpdate()
        {
            m_MeasureLine.Update(m_StartPoint.position, m_EndPoint.position, m_CameraProvider.Camera.transform.position);
            UpdateLabel();
        }

        void UpdateLabel()
        {
            Vector3 startPosition = m_StartPoint.position;
            Vector3 endPosition = m_EndPoint.position;

            var screenCenter = m_CameraProvider.Camera.WorldToScreenPoint((startPosition + endPosition) * 0.5f);
            var isCenterInView = screenCenter.z > 0.0f;

            Vector2 screenPointA = m_CameraProvider.Camera.WorldToScreenPoint(startPosition);
            Vector2 screenPointB = m_CameraProvider.Camera.WorldToScreenPoint(endPosition);

            if (!ShouldLabelBeDisplayed(screenPointB - screenPointA, m_MeasureLabel.Element.Size))
            {
                m_MeasureLabel.SetLabelVisible(false);
            }
            else
            {
                if (isCenterInView)
                {
                    m_MeasureLabel.SetLabelPosition(screenCenter);
                }
                m_MeasureLabel.SetLabelVisible(true);
            }
        }

        bool ShouldLabelBeDisplayed(Vector2 lineSize, Vector2 labelSize)
        {
            Vector2 scaledLine = new Vector2(lineSize.x / m_CameraProvider.Camera.pixelWidth * m_MeasureLabel.Element.panel.visualTree.worldBound.width, lineSize.y / m_CameraProvider.Camera.pixelHeight * m_MeasureLabel.Element.panel.visualTree.worldBound.height);

            if (Mathf.Abs(scaledLine.y) > labelSize.y || Mathf.Abs(scaledLine.x) > labelSize.x)
                return true;

            return false;
        }

        public void SetColor(Color color)
        {
            m_MeasureLine.SetColor(color);
            m_MeasureLabel.SetColor(color);
            
            foreach (var pointRenderer in m_PointRenderers)
            {
                pointRenderer.material.color = color;
            }
        }
        
        public void SetVisible(bool visible)
        {
            m_MeasureLabel.SetLabelVisible(visible);
            gameObject.SetActive(visible);
        }

        void OnDestroy()
        {
            m_MeasureLabel.Destroy();
        }
    }
}
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.MeasureTool
{
    [Serializable]
    public class MeasureLabel
    {
        [SerializeField]
        VisualTreeAsset m_MeasureLabelTemplate;
        
        [SerializeField]
        string m_BackgroundElementName = "line_measure_label";

        MeasureLabelElement m_MeasureLabelElement;

        VisualElement m_LabelBackground;

        public string Text
        {
            get => m_MeasureLabelElement?.Label.text;
            set => m_MeasureLabelElement?.SetText(value);
        }

        internal MeasureLabelElement Element => m_MeasureLabelElement;

        public VisualElement CreateVisualElement()
        {
            m_MeasureLabelElement = new MeasureLabelElement(m_MeasureLabelTemplate)
            {
                style =
                {
                    position = Position.Absolute
                }
            };

            m_LabelBackground = m_MeasureLabelElement.Q(m_BackgroundElementName);

            return m_MeasureLabelElement;
        }

        public void SetLabelPosition(Vector3 screenPos)
        {
            var localPoint = new Vector2(screenPos.x / Screen.width, 1 - screenPos.y / Screen.height);

            localPoint *= m_MeasureLabelElement.parent.layout.size;
            localPoint -= m_MeasureLabelElement.parent.layout.position;

            m_MeasureLabelElement.style.left = localPoint.x - (m_MeasureLabelElement.layout.width / 2);
            m_MeasureLabelElement.style.top = localPoint.y - (m_MeasureLabelElement.layout.height / 2);
        }

        public void SetLabelVisible(bool visible)
        {
            Common.Utils.SetVisible(m_MeasureLabelElement, visible);
        }

        public void SetColor(Color color)
        {
            m_LabelBackground.style.backgroundColor = color;
        }

        public void Destroy()
        {
            m_MeasureLabelElement?.RemoveFromHierarchy();
            m_MeasureLabelElement = null;
        }
    }
}

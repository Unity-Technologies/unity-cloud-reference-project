using System;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    [Serializable]
    public class MeasureLine
    {
        [SerializeField]
        LineRenderer m_Line;
        
        [SerializeField]
        float m_Width = 6.0f;

        public void Update(Vector3 startPosition, Vector3 endPosition, Vector3 cameraPosition)
        {
            m_Line.SetPosition(0, startPosition);
            m_Line.SetPosition(1, endPosition);

            var w = m_Width / 1000.0f; // Divide by 1000 to avoid small numbers

            m_Line.startWidth = Vector3.Distance(m_Line.GetPosition(0), cameraPosition) * w;
            m_Line.endWidth = Vector3.Distance(m_Line.GetPosition(1), cameraPosition) * w;
        }

        public void SetColor(Color color)
        {
            m_Line.material.color = color;
        }
    }
}
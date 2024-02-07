using System;
using System.Text;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Unity.ReferenceProject.Stats;
using Zenject;

namespace Unity.ReferenceProject
{
    public class SceneBoundsStats : TextStat
    {
        IDataStreamBound m_DataStreamBound;
        
        [SerializeField]
        Color m_ValuesColor = new (0.7f, 0.7f, 0.7f);
        
        [Inject]
        void Setup(IDataStreamBound dataStreamBound)
        {
            m_DataStreamBound = dataStreamBound;
        }

        protected override void OnStatUpdate()
        {
            var bounds = m_DataStreamBound.GetBound();

            var color = ColorUtility.ToHtmlStringRGBA(m_ValuesColor);

            var sb = new StringBuilder();
            sb.AppendLine("Bounds");
            sb.AppendLine($"Size <color=#{color}>{bounds.size}</color>");
            sb.Append($"Center <color=#{color}>{bounds.center}</color>");

            Text = sb.ToString();
        }
        
        void OnDrawGizmos()
        {
            if (m_DataStreamBound == null)
                return;

            var bounds = m_DataStreamBound.GetBound();
            
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}

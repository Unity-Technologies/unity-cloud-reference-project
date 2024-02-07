using System;
using System.Text;
using Unity.ReferenceProject.Common;
using UnityEngine;
using Unity.ReferenceProject.Stats;
using Zenject;

namespace Unity.ReferenceProject
{
    public class CameraStats : TextStat
    {
        ICameraProvider m_CameraProvider;
        
        [SerializeField]
        Color m_ValuesColor = new (0.7f, 0.7f, 0.7f);
        
        [Inject]
        void Setup(ICameraProvider cameraProvider)
        {
            m_CameraProvider = cameraProvider;
        }

        protected override void OnStatUpdate()
        {
            if (m_CameraProvider.Camera != null)
            {
                var cameraTransform = m_CameraProvider.Camera.transform;

                var sb = new StringBuilder();
                
                var color = ColorUtility.ToHtmlStringRGBA(m_ValuesColor);

                sb.AppendLine("Camera");
                sb.AppendLine($"Pos <color=#{color}>{cameraTransform.position}</color>");
                sb.AppendLine($"Rot <color=#{color}>{cameraTransform.rotation.eulerAngles}</color>");
                sb.Append($"Fov <color=#{color}>{m_CameraProvider.Camera.fieldOfView}</color>");
                sb.Append($" Near <color=#{color}>{m_CameraProvider.Camera.nearClipPlane}</color>");
                sb.Append($" Far <color=#{color}>{m_CameraProvider.Camera.farClipPlane}</color>");

                Text = sb.ToString();
            }
            else
            {
                Text = null;
            }
        }
    }
}

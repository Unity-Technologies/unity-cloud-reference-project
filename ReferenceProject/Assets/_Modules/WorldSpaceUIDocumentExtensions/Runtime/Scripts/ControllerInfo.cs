using System;
using UnityEngine;

namespace Unity.ReferenceProject.WorldSpaceUIDocumentExtensions
{
    public interface IControllerInfo
    {
        public Transform ControllerTransform { get; set; }
    }

    [Serializable]
    public class ControllerInfo : IControllerInfo
    {
        [SerializeField]
        Transform m_ControllerTransform;

        public Transform ControllerTransform
        {
            get => m_ControllerTransform;
            set => m_ControllerTransform = value;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarFadePosition : MonoBehaviour
    {
        [SerializeField]
        Renderer[] m_Renderers;
        
        static readonly string k_PositionNameID = "_Position";

        void Update()
        {
            foreach (var renderer in m_Renderers)
            {
                renderer.material.SetVector(k_PositionNameID, transform.position);
            }
        }
    }
}

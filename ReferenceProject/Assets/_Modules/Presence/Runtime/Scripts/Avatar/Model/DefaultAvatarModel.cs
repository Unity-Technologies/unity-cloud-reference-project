using System;
using UnityEngine;

namespace Unity.ReferenceProject.Presence
{
    public class DefaultAvatarModel : AvatarModel
    {
        [SerializeField]
        Renderer[] m_ColorRenderers;
        
        public override void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public override void SetColor(Color color)
        {
            foreach (var r in m_ColorRenderers)
            {
                foreach (var material in r.materials)
                {
                    material.color = color;
                }
            }
        }
    }
}

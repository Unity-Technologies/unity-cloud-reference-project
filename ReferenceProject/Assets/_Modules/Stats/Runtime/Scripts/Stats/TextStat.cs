using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Stats
{
    public class TextStat : BaseStat
    {
        [SerializeField]
        string m_DefaultText;
        
        [SerializeField]
        Color m_Color = Color.white;
        
        [SerializeField]
        HeadingSize m_Size = HeadingSize.XS;
        
        [SerializeField]
        bool m_EnableRichText;

        Heading m_Heading;
        
        protected string Text { get => m_Heading.text; set => m_Heading.text = value; }
        
        protected Heading Heading => m_Heading;

        public override VisualElement CreateVisualTree()
        {
            m_Heading = new Heading
            {
                size = m_Size,
                text = m_DefaultText,
                enableRichText = m_EnableRichText,
                style =
                {
                    color = m_Color
                }
            };
            return m_Heading;
        }
        
        protected override void OnStatUpdate()
        {
        }
    }
}
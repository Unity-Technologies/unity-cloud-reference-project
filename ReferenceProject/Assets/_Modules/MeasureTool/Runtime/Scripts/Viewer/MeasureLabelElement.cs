using System;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.MeasureTool
{
    /// <summary>
    /// UI element for a measure distance label
    /// </summary>
    public class MeasureLabelElement : VisualElement
    {
        readonly Text m_MeasureLabelText;

        public Text Label => m_MeasureLabelText;
        public Vector2 Size => new Vector2(resolvedStyle.width, resolvedStyle.height);

        public new class UxmlFactory : UxmlFactory<MeasureLabelElement, UxmlTraits>
        {
            public override string uxmlName => "MeasureLabel";
        }
        
        public MeasureLabelElement()
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>("MeasureLabel");
            template.CloneTree(this);
            
            m_MeasureLabelText = this.Q<Text>("line_measure_label_text");
        }
        
        public MeasureLabelElement(VisualTreeAsset template)
        {
            template.CloneTree(this);
            
            m_MeasureLabelText = this.Q<Text>("line_measure_label_text");
        }
        
        public void SetText(string text)
        {
            m_MeasureLabelText.text = text;
        }
    }
}

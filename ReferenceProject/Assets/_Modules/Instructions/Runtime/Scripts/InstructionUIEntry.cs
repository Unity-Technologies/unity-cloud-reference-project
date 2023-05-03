using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Instructions
{
    public abstract class InstructionUIEntry : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_Template;
        
        VisualElement m_InstructionsInstance;

        protected abstract bool IsSupportPlatform { get; }

        public void AddInstructions(VisualElement container)
        {
            if (!IsSupportPlatform)
                return;

            var instructions = m_InstructionsInstance ??= m_Template.Instantiate();
            container.Add(instructions);
        }
    }
}

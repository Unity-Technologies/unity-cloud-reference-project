using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject
{
    [RequireComponent(typeof(UIDocument))]
    public class AdditionalStyle : MonoBehaviour
    {
        [SerializeField]
        StyleSheet m_OptionalStyleSheet;

        [SerializeField]
        string m_ClassName;

        void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (m_OptionalStyleSheet != null)
            {
                uiDocument.rootVisualElement.styleSheets.Add(m_OptionalStyleSheet);
            }

            if (!string.IsNullOrEmpty(m_ClassName))
            {
                uiDocument.rootVisualElement.AddToClassList(m_ClassName);
            }
        }
    }
}

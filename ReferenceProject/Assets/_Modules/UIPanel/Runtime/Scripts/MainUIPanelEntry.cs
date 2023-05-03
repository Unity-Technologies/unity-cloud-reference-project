using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UIPanel
{
    [RequireComponent(typeof(UIDocument))]
    public class MainUIPanelEntry : MonoBehaviour
    {
        void Awake()
        {
            var uiDocument = gameObject.GetComponent<UIDocument>();
            MainUIPanel.Instance.Add(uiDocument);
        }
    }
}

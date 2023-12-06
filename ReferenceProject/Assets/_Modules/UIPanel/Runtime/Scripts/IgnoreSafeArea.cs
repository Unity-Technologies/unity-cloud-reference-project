using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject
{
    public class IgnoreSafeArea : MonoBehaviour
    {
#if UNITY_ANDROID || UNITY_IPHONE
        void Start()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null || uiDocument.rootVisualElement == null)
                return;

            uiDocument.rootVisualElement.RegisterCallback<GeometryChangedEvent>((e) => IgnoreSafeAreaMargin(uiDocument));
        }

        static void IgnoreSafeAreaMargin(UIDocument uiDocument)
        {
            var element = uiDocument.rootVisualElement;
            var ancestorPanel = element.GetFirstAncestorWithName(uiDocument.panelSettings.name);
            var (leftTop, rightBottom) = ancestorPanel.SafeArea();

            element.style.marginLeft = -leftTop.x;
            element.style.marginTop = -leftTop.y;
            element.style.marginBottom = -rightBottom.y;
            element.style.marginRight = -rightBottom.x;
        }
#endif
    }
}

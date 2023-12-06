using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject
{
    public static class VisualElementExtensions
    {
        public static (Vector2 leftTop, Vector2 rightBottom) SafeArea(this VisualElement element)
        {
            var safeArea = Screen.safeArea;

            var leftTop = RuntimePanelUtils.ScreenToPanel(element.panel,
                new Vector2(safeArea.xMin, Screen.height - safeArea.yMax));
            var rightBottom = RuntimePanelUtils.ScreenToPanel(element.panel,
                new Vector2(Screen.width - safeArea.xMax, safeArea.yMin));

            return (leftTop, rightBottom);
        }

        public static void SetSafeAreaMargin(this VisualElement element)
        {
            var (leftTop, rightBottom) = element.SafeArea();
            element.style.marginLeft = leftTop.x;
            element.style.marginTop = leftTop.y;
            element.style.marginBottom = rightBottom.y;
            element.style.marginRight = rightBottom.x;
        }

        public static void SetSafeAreaPadding(this VisualElement element)
        {
            var (leftTop, rightBottom) = element.SafeArea();
            element.style.paddingLeft = leftTop.x;
            element.style.paddingTop = leftTop.y;
            element.style.paddingBottom = rightBottom.y;
            element.style.paddingRight = rightBottom.x;
        }

        public static VisualElement GetFirstAncestorWithName(this VisualElement element, string name)
        {
            if (element == null)
                return null;

            if (element.name.Equals(name))
                return element;

            return element.parent.GetFirstAncestorWithName(name);
        }
    }
}

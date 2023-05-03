using System;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Editor
{
    public class KitPage : StoryBookPage
    {
        public override string displayName => "Kit";

        public override Type componentType => null;

        public KitPage()
        {
            m_Stories.Add(new StoryBookStory("Main", () =>
            {
                var element = new VisualElement();
                var tree = Resources.Load<VisualTreeAsset>("Examples");
                tree.CloneTree(element);
                var root = element.Q<VisualElement>("root-main");
                // remove scrollview
                // var scrollView = element.Q<ScrollView>();
                // var panel = scrollView.parent;
                // var root = panel.parent;
                // var children = scrollView.contentContainer.Children().ToList();
                // foreach (var c in children)
                // {
                //     root.Add(c);
                // }
                // root.Remove(panel);
                root.styleSheets.Add(Resources.Load<ThemeStyleSheet>("ExampleTheme"));
                Samples.Examples.SetupDataBinding(root);
                return root;
            }));
        }
    }
}

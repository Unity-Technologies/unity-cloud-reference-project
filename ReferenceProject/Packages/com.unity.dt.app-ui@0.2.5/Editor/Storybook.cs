using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Editor
{
    /// <summary>
    /// This class defines a property for a StoryBookComponent.
    /// </summary>
    public class StoryBookComponentProperty
    {
        /// <summary>
        /// The name of the property.
        /// </summary>
        public string name { get; protected set; }
    }

    /// <summary>
    /// This class defines a boolean property for a StoryBookComponent.
    /// </summary>
    public class StoryBookBooleanProperty : StoryBookComponentProperty
    {
        /// <summary>
        /// The getter of the property.
        /// </summary>
        public Func<VisualElement, bool> getter { get; set; }

        /// <summary>
        /// The setter of the property.
        /// </summary>
        public Action<VisualElement, bool> setter { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <param name="getter"> The getter of the property. </param>
        /// <param name="setter"> The setter of the property. </param>
        public StoryBookBooleanProperty(string name, Func<VisualElement, bool> getter, Action<VisualElement, bool> setter)
        {
            this.name = name;
            this.getter = getter;
            this.setter = setter;
        }
    }

    /// <summary>
    /// This class defines a string property for a StoryBookComponent.
    /// </summary>
    public class StoryBookStringProperty : StoryBookComponentProperty
    {
        /// <summary>
        /// The getter of the property.
        /// </summary>
        public Func<VisualElement, string> getter { get; set; }

        /// <summary>
        /// The setter of the property.
        /// </summary>
        public Action<VisualElement, string> setter { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <param name="getter"> The getter of the property. </param>
        /// <param name="setter"> The setter of the property. </param>
        public StoryBookStringProperty(string name, Func<VisualElement, string> getter, Action<VisualElement, string> setter)
        {
            this.name = name;
            this.getter = getter;
            this.setter = setter;
        }
    }

    /// <summary>
    /// A StoryBookComponent is a component that can be used as a StoryBookPage.
    /// </summary>
    public abstract class StoryBookComponent
    {
        /// <summary>
        /// The type of the UI element.
        /// </summary>
        public virtual Type uiElementType { get; }

        /// <summary>
        /// The properties of the component.
        /// </summary>
        public IEnumerable<StoryBookComponentProperty> properties => m_Properties;

        /// <summary>
        /// The list of properties (used internally).
        /// </summary>
        protected readonly List<StoryBookComponentProperty> m_Properties = new List<StoryBookComponentProperty>();
    }

    /// <summary>
    /// A StoryBookStory is a story that can be used inside a StoryBookPage.
    /// </summary>
    public sealed class StoryBookStory
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"> The name of the story. </param>
        /// <param name="createGUI"> The function that creates the GUI of the story. </param>
        public StoryBookStory(string name, Func<VisualElement> createGUI)
        {
            this.name = name;
            this.createGUI = createGUI;
        }

        /// <summary>
        /// The name of the story.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// The function that creates the GUI of the story.
        /// </summary>
        public Func<VisualElement> createGUI { get; }
    }

    /// <summary>
    /// A StoryBookPage is a page that can be used inside a StoryBook.
    /// </summary>
    public abstract class StoryBookPage
    {
        /// <summary>
        /// The name of the page.
        /// </summary>
        public virtual string displayName { get; }

        /// <summary>
        /// The type of the component.
        /// </summary>
        public virtual Type componentType { get; }

        /// <summary>
        /// The stories of the page.
        /// </summary>
        public IEnumerable<StoryBookStory> stories => m_Stories;

        /// <summary>
        /// The list of stories (used internally).
        /// </summary>
        protected readonly List<StoryBookStory> m_Stories = new List<StoryBookStory>();
    }

    /// <summary>
    /// A StoryBook is a window that allows to preview the UI elements of the App UI package.
    /// </summary>
    public class Storybook : EditorWindow
    {
        const string k_DefaultTheme = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss";

        List<Type> m_ComponentTypes = new List<Type>();

        List<StoryBookPage> m_StoriesList = new List<StoryBookPage>();

        TwoPaneSplitView m_SplitView;

        ListView m_ListView;

        VisualElement m_Preview;

        ListView m_StoryListView;

        ScrollView m_Inspector;

        TwoPaneSplitView m_VerticalPane;

        /// <summary>
        /// Open the StoryBook window.
        /// </summary>
        [UnityEditor.MenuItem("Window/App UI/ Storybook")]
        public static void OpenStoryBook()
        {
            var window = GetWindow<Storybook>("App UI - StoryBook");
            window.Show();
        }

        static IEnumerable<Type> GetComponents()
        {
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if ((type.Namespace?.StartsWith("UnityEngine.Dt.App.UI") ?? false)
                        && !type.IsAbstract && type.IsClass && type.IsPublic && !type.IsGenericType)
                    {
                        types.Add(type);
                    }
                }
            }
            
            return types;
        }

        static IEnumerable<StoryBookPage> GetStories()
        {
            var stories = new List<StoryBookPage>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(typeof(StoryBookPage)))
                    {
                        stories.Add(Activator.CreateInstance(type) as StoryBookPage);
                    }
                }
            }
            
            return stories;
        }

        void CreateGUI()
        {
            var root = rootVisualElement;

            m_SplitView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            m_ComponentTypes = new List<Type>(GetComponents());
            m_StoriesList = new List<StoryBookPage>(GetStories());
            var listPane = new TwoPaneSplitView(0, 100, TwoPaneSplitViewOrientation.Horizontal);
            m_ListView = new ListView(m_StoriesList, -1f, MakeListVIewItem, BindListViewItem);
            m_StoryListView = new ListView(null, -1f, MakeListVIewItem, BindStoryListViewItem);
            m_Preview = CreateDetailPage();
#if UNITY_2022_2_SIC
            m_ListView.selectedIndicesChanged += OnSelectionChanged;
            m_StoryListView.selectedIndicesChanged += OnStorySelectionChanged;
#else
            m_ListView.onSelectedIndicesChange += OnSelectionChanged;
            m_StoryListView.onSelectedIndicesChange += OnStorySelectionChanged;
#endif
            listPane.Add(m_ListView);
            listPane.Add(m_StoryListView);
            m_SplitView.Add(listPane);
            m_SplitView.Add(m_Preview);

            root.Add(m_SplitView);

            root.Bind(new SerializedObject(this));
        }

        void BindStoryListViewItem(VisualElement ve, int idx)
        {
            ((Label)ve).text = ((List<StoryBookStory>)m_StoryListView.itemsSource)[idx].name;
        }

        void OnSelectionChanged(IEnumerable<int> indices)
        {
            var indicesList = new List<int>(indices);
            if (indicesList.Count > 0)
                RefreshStoryList(m_StoriesList[indicesList[0]]);
        }

        void RefreshStoryList(StoryBookPage page)
        {
            var items = new List<StoryBookStory>(page.stories);
            if (page.componentType != null)
                items.Insert(0, new StoryBookStory("Canvas", null));
            m_StoryListView.itemsSource = items;
            m_StoryListView.RefreshItems();
        }

        void OnStorySelectionChanged(IEnumerable<int> indices)
        {
            var indicesList = new List<int>(indices);
            if (indicesList.Count > 0)
                RefreshDetailPage(((List<StoryBookStory>)m_StoryListView.itemsSource)[indicesList[0]]);
        }

        VisualElement CreateDetailPage()
        {
            m_VerticalPane = new TwoPaneSplitView(1, 150, TwoPaneSplitViewOrientation.Vertical);

            var canvas = new VisualElement();
            canvas.styleSheets.Add(AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(k_DefaultTheme));
            var panel = new Panel();
            var container = new VisualElement { name = "canvas-container" };
            container.style.alignItems = Align.Center;
            canvas.Add(panel);
            panel.Add(container);
            container.StretchToParentSize();
            m_VerticalPane.Add(canvas);

            m_Inspector = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    paddingBottom = 8,
                    paddingLeft = 8,
                    paddingTop = 8,
                    paddingRight = 8,
                }
            };
            var inspectorTitle = new Label("Properties")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            };
            m_Inspector.Add(inspectorTitle);
            var inspectorContainer = new VisualElement { name = "inspector-container" };
            m_Inspector.Add(inspectorContainer);
            m_VerticalPane.Add(m_Inspector);

            return m_VerticalPane;
        }

        void RefreshDetailPage(StoryBookStory story)
        {
            var container = rootVisualElement.Q<VisualElement>("canvas-container");
            container.Clear();

            var inspectorContainer = rootVisualElement.Q<VisualElement>("inspector-container");
            inspectorContainer.Clear();

            if (story == null)
            {
                m_VerticalPane.CollapseChild(1);
                return;
            }

            if (story.createGUI == null)
            {
                m_VerticalPane.UnCollapse();

                // Update component page
                var component = (StoryBookComponent)Activator.CreateInstance(m_StoriesList[m_ListView.selectedIndex].componentType);
                var uiElement = (VisualElement)Activator.CreateInstance(component.uiElementType);

                container.Add(uiElement);

                foreach (var prop in component.properties)
                {
                    VisualElement field = null;
                    switch (prop)
                    {
                        case StoryBookBooleanProperty boolProp:
                            var toggle = new UIElements.Toggle(boolProp.name);
                            toggle.SetValueWithoutNotify(boolProp.getter?.Invoke(uiElement) ?? false);
                            toggle.RegisterValueChangedCallback(evt => boolProp.setter?.Invoke(uiElement, evt.newValue));
                            field = toggle;
                            break;
                        case StoryBookStringProperty strProp:
                            var textField = new UIElements.TextField(strProp.name);
                            textField.SetValueWithoutNotify(strProp.getter?.Invoke(uiElement));
                            textField.RegisterValueChangedCallback(evt => strProp.setter?.Invoke(uiElement, evt.newValue));
                            field = textField;
                            break;
                        default:
                            break;
                    }

                    if (field != null)
                    {
                        inspectorContainer.Add(field);
                    }
                }
            }
            else
            {
                m_VerticalPane.CollapseChild(1);
                var uiElement = story.createGUI();
                container.Add(uiElement);
            }

        }

        void RefreshPreview(Type componentType)
        {
            var instance = Activator.CreateInstance(componentType) as VisualElement;
            m_Preview.Clear();
            m_Preview.Add(new Label($"Properties for {componentType.Name}"));
            foreach (var propertyInfo in componentType.GetProperties(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public /*| BindingFlags.DeclaredOnly*/))
            {
                if (!propertyInfo.CanRead || !propertyInfo.CanWrite)
                    continue;

                VisualElement prop = null;
                switch (propertyInfo.PropertyType.Name)
                {
                    case "Boolean":
                        var toggle = new UIElements.Toggle(propertyInfo.Name);
                        toggle.RegisterValueChangedCallback((evt) => propertyInfo.SetValue(instance, evt.newValue));
                        toggle.SetValueWithoutNotify((bool)propertyInfo.GetValue(instance));
                        prop = toggle;
                        break;
                    case "String":
                        var textField = new UIElements.TextField(propertyInfo.Name);
                        textField.RegisterValueChangedCallback((evt) => propertyInfo.SetValue(instance, evt.newValue));
                        textField.SetValueWithoutNotify((string)propertyInfo.GetValue(instance));
                        prop = textField;
                        break;
                    case "HeaderSize":
                        var enumField = new EnumField(propertyInfo.Name, HeaderSize.M);
                        enumField.RegisterValueChangedCallback((evt) => propertyInfo.SetValue(instance, evt.newValue));
                        enumField.SetValueWithoutNotify((HeaderSize)propertyInfo.GetValue(instance));
                        prop = enumField;
                        break;
                    case "Size":
                        var enum2Field = new EnumField(propertyInfo.Name, Size.M);
                        enum2Field.RegisterValueChangedCallback((evt) => propertyInfo.SetValue(instance, evt.newValue));
                        enum2Field.SetValueWithoutNotify((Size)propertyInfo.GetValue(instance));
                        prop = enum2Field;
                        break;
                    default:
                        Debug.Log($"Ignore {propertyInfo.PropertyType.Name}");
                        break;
                }

                if (prop != null)
                {
                    m_Preview.Add(prop);
                }
            }

            var viewport = new VisualElement();
            viewport.style.flexGrow = 1;
            viewport.styleSheets.Add(AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>("Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
            var panel = new Panel();
            var container = new VisualElement();
            container.style.paddingBottom = 16;
            container.style.paddingLeft = 16;
            container.style.paddingRight = 16;
            container.style.paddingTop = 16;
            container.style.alignItems = Align.FlexStart;
            viewport.Add(panel);
            panel.Add(container);
            container.Add(instance);
            m_Preview.Add(viewport);
        }

        void BindListViewItem(VisualElement ve, int idx)
        {
            var label = (Label)ve;
            label.text = m_StoriesList[idx].displayName;
        }

        VisualElement MakeListVIewItem()
        {
            return new Label()
            {
                style = {
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8,
                }
            };
        }
    }
}

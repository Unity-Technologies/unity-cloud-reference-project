using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Zenject;
using Object = UnityEngine.Object;

namespace Unity.ReferenceProject.UIPanel.Tests
{
    public class MainUIPanelEntryTests
    {
        readonly TestGameObjects m_TestGameObjects = new();
        PanelSettings m_PanelSettings;
        DiContainer m_DiContainer;

        VisualTreeAsset m_VisualTreeAsset;

        [OneTimeSetUp]
        public void OnTimeSetup()
        {
            m_VisualTreeAsset = Resources.Load<VisualTreeAsset>("DummyTemplate");
            m_PanelSettings = Resources.Load<PanelSettings>("DummyPanelSettings");
        }

        [TearDown]
        public void TearDown()
        {
            m_TestGameObjects?.Cleanup();
        }

        [SetUp]
        public void CommonInstall()
        {
            m_DiContainer = new DiContainer();
            m_DiContainer.Bind<IMainUIPanel>().FromInstance(m_TestGameObjects.NewMainUIPanel(m_PanelSettings)).AsSingle();
        }

        [UnityTest]
        public IEnumerator MainUIPanelEntry_SortingOrder_UsesUIDocument()
        {
            // First entry
            var entry0 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry0", 0);
            yield return null;

            var root = entry0.GetComponent<UIDocument>().rootVisualElement.parent;
            AssertContent(root, entry0); // 0

            // Higher order
            var entry1 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry1", 10);
            yield return null;
            AssertContent(root, entry0, entry1); // 0, 10

            // Lower order
            var entry2 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry2", -10);
            yield return null;
            AssertContent(root, entry2, entry0, entry1); // -10, 0, 10

            // In between order
            var entry3 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry3", 5);
            yield return null;
            AssertContent(root, entry2, entry0, entry3, entry1); // -10, 0, 5, 10

            // In between order
            var entry4 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry4", -5);
            yield return null;
            AssertContent(root, entry2, entry4, entry0, entry3, entry1); // -10, -5, 0, 5, 10

            // In between duplicate order
            var entry5 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry5", 0);
            yield return null;
            AssertContent(root, entry2, entry4, entry0, entry5, entry3, entry1); // -10, -5, 0, 0, 5, 10

            // Lower duplicate order
            var entry6 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry6", -10);
            yield return null;
            AssertContent(root, entry2, entry6, entry4, entry0, entry5, entry3, entry1); // -10, -10, -5, 0, 0, 5, 10

            // Higher duplicate order
            var entry7 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry7", 10);
            yield return null;
            AssertContent(root, entry2, entry6, entry4, entry0, entry5, entry3, entry1, entry7); // -10, -10, -5, 0, 0, 5, 10, 10
        }

        [UnityTest] // UCRP-137
        public IEnumerator MainUIPanelEntry_SortingOrder_IgnoreManuallyAddingElements()
        {
            // First entry
            var entry0 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry0", 0);
            var entry1 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry1", 10);
            var entry2 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry2", -10);
            yield return null;

            var root = entry0.GetComponent<UIDocument>().rootVisualElement.parent;
            AssertContent(root, entry2, entry0, entry1); // -10, 0, 10

            // Manually add new elements
            var e0 = NewGameObjectWithUIDocument();
            root.Add(e0.GetComponent<UIDocument>().rootVisualElement);

            var e1 = NewGameObjectWithUIDocument();
            root.Add(e1.GetComponent<UIDocument>().rootVisualElement);

            var e2 = NewGameObjectWithUIDocument();
            root.Insert(2, e2.GetComponent<UIDocument>().rootVisualElement);

            // Manually added elements are not associated with a sorting order and should stay last
            AssertContent(root, entry2, entry0, e2, entry1, e0, e1); // -10, 0, X, 10, X, X

            var entry3 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry3", 20);
            yield return null;

            AssertContent(root, entry2, entry0, e2, entry1, e0, e1, entry3); // -10, 0, X, 10, X, X, 20

            var entry4 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry4", 15);
            yield return null;

            AssertContent(root, entry2, entry0, e2, entry1, e0, e1, entry4, entry3); // -10, 0, X, 10, X, X, 15, 20
        }

        [UnityTest] // UCRP-137
        public IEnumerator MainUIPanelEntry_SortingOrder_WorksAfterDestroyingElements()
        {
            // First entry
            var entry0 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry0", 0);
            var entry1 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry1", 10);
            var entry2 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry2", -10);
            var entry3 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry3", 5);
            var entry4 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry4", -5);
            var entry5 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry5", 0);
            var entry6 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry6", -10);
            var entry7 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry7", 10);
            yield return null;

            var root = entry0.GetComponent<UIDocument>().rootVisualElement.parent;
            AssertContent(root, entry2, entry6, entry4, entry0, entry5, entry3, entry1, entry7); // -10, -10, -5, 0, 0, 5, 10, 10

            // Destroy some elements
            Object.DestroyImmediate(entry0);
            Object.DestroyImmediate(entry2);
            Object.DestroyImmediate(entry6);
            Object.DestroyImmediate(entry7);

            AssertContent(root, entry4, entry5, entry3, entry1); // -5, 0, 5, 10

            var entry8 = NewGameObjectWithUIDocumentAndMainUIPanelEntry("entry8", 0);
            yield return null;
            AssertContent(root, entry4, entry5, entry8, entry3, entry1); // -5, 0, 0, 5, 10
        }

        static void AssertContent(VisualElement root, params GameObject[] items)
        {
            Assert.AreEqual(items.Length, root.childCount);

            for (var i = 0; i < items.Length; ++i)
            {
                Assert.AreSame(items[i].GetComponent<UIDocument>().rootVisualElement, root.ElementAt(i), $"item {items[i]?.name} at index {i} does not match");
            }
        }

        GameObject NewGameObjectWithUIDocumentAndMainUIPanelEntry(string name, int sortingOrder)
        {
            var go = NewGameObjectWithUIDocument(name, sortingOrder);

            m_DiContainer.InstantiateComponent<MainUIPanelEntry>(go);

            return go;
        }

        GameObject NewGameObjectWithUIDocument(string name = null, int sortingOrder = 0)
        {
            var go = m_TestGameObjects.NewGameObject(name);

            var uiDocument = go.AddComponent<UIDocument>();
            uiDocument.visualTreeAsset = m_VisualTreeAsset;
            uiDocument.panelSettings = m_PanelSettings;
            uiDocument.sortingOrder = sortingOrder;

            return go;
        }
    }
}

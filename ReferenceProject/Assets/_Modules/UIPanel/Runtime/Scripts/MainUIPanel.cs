using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.UIPanel
{
    public partial interface IMainUIPanel
    {
        public Panel Panel { get; }
        public void Add(UIDocument uiDocument);
    }

    public partial class MainUIPanel : IMainUIPanel
    {
        readonly Panel m_Panel;

        readonly List<UIDocument> m_AttachedDocuments = new ();

        readonly string k_PanelName = "panel-appUI";

        public Panel Panel => m_Panel;

        internal UIDocument UIDocument { get; }

        public MainUIPanel(PanelSettings panelSettings)
        {
            var go = new GameObject("MainUIPanel (Dynamic)");
            Object.DontDestroyOnLoad(go);
            UIDocument = go.AddComponent<UIDocument>();
            UIDocument.panelSettings = panelSettings;

            m_Panel = new Panel
            {
                name = k_PanelName,
                style = { backgroundColor = new StyleColor(new Color(0.0f, 0.0f, 0.0f, 0.0f)) },
                pickingMode = PickingMode.Position
            };

            UIDocument.rootVisualElement.Add(m_Panel);
        }

        public void Add(UIDocument uiDocument)
        {
            // Try to sync with the actual Panel content. Note. VisualElements added outside of this method will be ignored in the sorting.
            foreach (var document in m_AttachedDocuments.ToArray())
            {
                if (!m_Panel.Contains(document.rootVisualElement))
                    m_AttachedDocuments.Remove(document);
            }

            var insertAtElement = m_AttachedDocuments.FirstOrDefault(doc => uiDocument.sortingOrder < doc.sortingOrder);

            if (insertAtElement != null)
            {
                var index = m_AttachedDocuments.IndexOf(insertAtElement);
                m_AttachedDocuments.Insert(index, uiDocument);

                index = m_Panel.IndexOf(insertAtElement.rootVisualElement);
                m_Panel.Insert(index, uiDocument.rootVisualElement);
            }
            else
            {
                m_AttachedDocuments.Add(uiDocument);
                m_Panel.Add(uiDocument.rootVisualElement);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public class ProjectUIController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_ProjectListItemTemplate;

        readonly string k_ProjectGroup = "ProjectGroup";

        ActionGroup m_ProjectList;
        ActionButton m_AllProjectButton;

        public event Action<IAssetProject> ProjectSelected;
        public event Action AllProjectSelected;

        void Awake()
        {
            var allProjectButton = m_ProjectListItemTemplate.CloneTree();
            m_AllProjectButton = allProjectButton.Q<ActionButton>();
            m_AllProjectButton.label = "@AssetList:AllAssets";
            m_AllProjectButton.clicked += () => AllProjectSelected?.Invoke();
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_ProjectList = root.Q<ActionGroup>(k_ProjectGroup);
        }

        public void Clear()
        {
            m_ProjectList.Clear();
            m_ProjectList.ClearSelectionWithoutNotify();
        }

        public void Populate(IEnumerable<IAssetProject> projects)
        {
            Clear();

            if (projects.Count() > 1)
            {
                m_ProjectList.Add(m_AllProjectButton);
            }

            foreach (var project in projects)
            {
                var clone = m_ProjectListItemTemplate.CloneTree();
                var projectButton = clone.Q<ActionButton>();
                projectButton.label = project.Name;
                projectButton.clicked += () => ProjectSelected?.Invoke(project);
                m_ProjectList.Add(projectButton);
            }
        }

        public void SelectProject(IAssetProject project)
        {
            if (project == null)
            {
                m_ProjectList.ClearSelectionWithoutNotify();
                return;
            }

            var projectButton = m_ProjectList.Children().OfType<ActionButton>().FirstOrDefault(x => x.label == project.Name);
            if (projectButton != null)
            {
                int index = m_ProjectList.IndexOf(projectButton);
                m_ProjectList.SetSelectionWithoutNotify(new[] { index });
            }
        }

        public void SelectAllProjectButton()
        {
            m_ProjectList.SetSelectionWithoutNotify(new[] { 0 });
        }
    }
}

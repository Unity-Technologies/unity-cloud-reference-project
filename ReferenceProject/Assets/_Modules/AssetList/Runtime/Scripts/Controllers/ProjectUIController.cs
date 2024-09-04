using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.AssetList
{
    public class ProjectUIController : MonoBehaviour
    {
        [SerializeField]
        VisualTreeAsset m_ProjectListItemTemplate;

        readonly string k_ProjectListContainer = "ProjectListContainer";
        readonly string k_CaretClose = "caret-right";
        readonly string k_CaretOpen = "caret-down";

        VisualElement m_ProjectList;
        ActionButton m_AllProjectButton;

        readonly Dictionary<ProjectId, VisualElement> m_ProjectItems = new();
        readonly Dictionary<CollectionPath, ActionButton> m_Collections = new();

        public event Action<IAssetProject> ProjectSelected;
        public event Action<IAssetCollection> CollectionSelected;

        void Awake()
        {
            var allProjectButton = m_ProjectListItemTemplate.CloneTree();
            m_AllProjectButton = allProjectButton.Q<ActionButton>();
            m_AllProjectButton.label = "@AssetList:AllAssets";
            var icon = allProjectButton.Q<Icon>();
            icon.iconName = "stack";
            icon.style.backgroundColor = Color.clear;
            m_AllProjectButton.clicked += () => ProjectSelected?.Invoke(null);
        }

        public void InitUIToolkit(VisualElement root)
        {
            m_ProjectList = root.Q(k_ProjectListContainer);
        }

        public void Clear()
        {
            m_AllProjectButton.selected = false;
            m_ProjectList.Clear();
            m_ProjectItems.Clear();
            m_Collections.Clear();
        }

        public void Populate(IEnumerable<IAssetProject> projects)
        {
            Clear();

            if (projects == null || !projects.Any())
                return;

            if (projects.Count() > 1)
            {
                m_ProjectList.Add(m_AllProjectButton);
            }

            foreach (var project in projects)
            {
                var projectItem = m_ProjectListItemTemplate.CloneTree().Q("Root");
                var projectButton = projectItem.Q<ActionButton>();
                m_ProjectItems.Add(project.Descriptor.ProjectId, projectItem);
                projectButton.tooltip = project.Name;
                var projectIcon = projectButton.Q<Icon>();

                var projectId = project.Descriptor.ProjectId.ToString();
                _ = TextureController.GetProjectIcon(projectId, texture =>
                {
                    if (texture != null)
                    {
                        projectIcon.image = texture;
                        projectIcon.AddToClassList("button__project-list-item-icon");
                    }
                    else
                    {
                        projectIcon.style.paddingTop = projectIcon.style.paddingBottom = projectIcon.style.paddingLeft = projectIcon.style.paddingRight = 3;
                        projectIcon.style.backgroundColor = TextureController.GetProjectIconColor(projectId);
                    }
                });

                projectButton.label = project.Name;
                projectButton.clicked += () => ProjectSelected?.Invoke(project);
                m_ProjectList.Add(projectItem);
            }

            _ = PopulateCollections(projects);
        }

        public async Task PopulateCollections(IEnumerable<IAssetProject> projects)
        {
            foreach (var project in projects)
            {
                var projectItem = m_ProjectItems[project.Descriptor.ProjectId];
                var projectButton = projectItem.Q<ActionButton>();
                var collectionsContainer = projectItem.Q("CollectionsContainer");
                var collectionsList = collectionsContainer.Q("CollectionsList");

                var isInitialized = false;

                await foreach (var collection in project.ListCollectionsAsync(Range.All, CancellationToken.None))
                {
                    if (!isInitialized)
                    {
                        isInitialized = true;
                        var caret = new Button
                        {
                            quiet = true,
                            leadingIcon = k_CaretClose
                        };
                        caret.AddToClassList("button__project-list-item-caret");
                        projectButton.hierarchy.Add(caret);
                        caret.clicked += () => OnCaretClicked(caret, collectionsContainer);
                        projectButton.clicked += () => OnCaretClicked(caret, collectionsContainer);
                    }

                    var collectionButton = new ActionButton();
                    collectionButton.quiet = true;
                    collectionButton.AddToClassList("button__project-list-item-collection");
                    collectionButton.label = collection.Name;
                    collectionButton.tooltip = collection.Name;

                    collectionButton.clicked += () => CollectionSelected?.Invoke(collection);
                    collectionsList.Add(collectionButton);
                    m_Collections.TryAdd(collection.Descriptor.Path, collectionButton);
                }
            }
        }

        public void SelectProject(IAssetProject project)
        {
            // Unselect all project buttons
            ClearProjectSelection();

            if (project == null)
            {
                m_AllProjectButton.selected = true;
                return;
            }

            m_AllProjectButton.selected = false;

            if (m_ProjectItems.TryGetValue(project.Descriptor.ProjectId, out var pi) && pi != null)
            {
                var projectButton = pi.Q<ActionButton>();
                if (projectButton != null)
                {
                    projectButton.selected = true;
                }
            }
        }

        public void SelectCollection(IAssetCollection collection)
        {
            ClearProjectSelection();
            foreach (var collectionButton in m_Collections.Values)
            {
                collectionButton.selected = false;
            }

            if (collection == null)
                return;

            if (m_Collections.TryGetValue(collection.Descriptor.Path, out var button))
            {
                button.selected = true;
            }
        }

        void ClearProjectSelection()
        {
            foreach (var projectItem in m_ProjectItems.Values)
            {
                var projectButton = projectItem.Q<ActionButton>();
                if (projectButton != null)
                {
                    projectButton.selected = false;
                }
            }
        }

        void OnCaretClicked(Button caret, VisualElement collectionsContainer)
        {
            caret.leadingIcon = caret.leadingIcon == k_CaretClose ? k_CaretOpen : k_CaretClose;
            Common.Utils.SetVisible(collectionsContainer, caret.leadingIcon == k_CaretOpen);
        }
    }
}

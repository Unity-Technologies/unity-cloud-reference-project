using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using Unity.Cloud.Common;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasureListUIController : MonoBehaviour
    {
        [SerializeField]
        Color m_SavedMeasureColor;

        [SerializeField]
        Color m_SelectedMeasureColor;

        [SerializeField]
        MeasurementToolController m_MeasurementToolController;

        [SerializeField]
        VisualTreeAsset m_MeasureListItemTemplate;

        [SerializeField]
        VisualTreeAsset m_SaveDialogTemplate;

        readonly int k_MaxNameLength = 64;

        VisualElement m_ScrollView;
        VisualElement m_ConfirmationContainer;
        VisualElement m_EditConfirmationContainer;
        ActionButton m_SaveButton;
        ActionButton m_ClearButton;
        ActionButton m_ApplyButton;
        ActionButton m_DiscardButton;
        VisualElement m_NameContainer;
        TextField m_NameField;
        VisualElement m_LineListContainer;
        readonly List<IMeasureListItem> m_MeasureListItems = new();

        IAppMessaging m_Messaging;
        IAssetEvents m_AssetEvents;

        MeasureLinePersistence m_Persistence;
        MeasureLineData m_SelectedLine;
        MeasureLineData m_LastEditedLineData;

        IDataset m_CurrentDataset;

        public string Name
        {
            get => m_NameField?.value;
            set => m_NameField?.SetValueWithoutNotify(value);
        }

        [Inject]
        public void Setup(MeasureLinePersistence persistence, IAssetEvents assetEvents, IAppMessaging messaging)
        {
            m_Persistence = persistence;
            m_AssetEvents = assetEvents;
            m_Messaging = messaging;
        }

        void Awake()
        {
            m_MeasurementToolController.MeasureDataChanged += OnMeasureDataChanged;
            m_AssetEvents.AssetLoaded += OnAssetLoaded;
            m_AssetEvents.AssetUnloaded += OnAssetUnloaded;
        }

        void OnDestroy()
        {
            m_AssetEvents.AssetLoaded -= OnAssetLoaded;
            m_AssetEvents.AssetUnloaded -= OnAssetUnloaded;
            m_MeasurementToolController.MeasureDataChanged -= OnMeasureDataChanged;
            m_SaveButton.clicked -= OnSave;
            m_ClearButton.clicked -= OnClear;
            m_ApplyButton.clicked -= OnApply;
            m_DiscardButton.clicked -= OnDiscard;
            m_NameField.validateValue -= ValidateLength;
        }

        public void InitializeUI(VisualElement rootVisualElement)
        {
            m_ConfirmationContainer = rootVisualElement.Q("line_creator_confirmation_container");
            m_EditConfirmationContainer = rootVisualElement.Q("line_creator_edit_confirmation_container");
            m_LineListContainer = rootVisualElement.Q("line_list_container");
            m_NameContainer = rootVisualElement.Q("line_creator_name_container");
            m_ScrollView = rootVisualElement.Q("line_list_scrollview");
            m_SaveButton = rootVisualElement.Q<ActionButton>("line_creator_confirmation_save");
            m_ClearButton = rootVisualElement.Q<ActionButton>("line_creator_confirmation_clear");
            m_ApplyButton = rootVisualElement.Q<ActionButton>("line_creator_edit_confirmation_apply");
            m_DiscardButton = rootVisualElement.Q<ActionButton>("line_creator_edit_confirmation_discard");
            m_NameField = rootVisualElement.Q<TextField>("line_creator_name_value");

            m_SaveButton.clicked += OnSave;
            m_ClearButton.clicked += OnClear;
            m_ApplyButton.clicked += OnApply;
            m_DiscardButton.clicked += OnDiscard;
            m_NameField.validateValue += ValidateLength;
            m_NameField.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue != m_SelectedLine.Name)
                {
                    EnableApplyButton(true);
                }
            });

            ResetDialog();
        }

        public void OnToolOpen()
        {
            OnClear();

            // Reset shown lines
            foreach (var item in m_MeasureListItems)
            {
                if (item.IsShown)
                {
                    m_MeasurementToolController.Add(item.Data);
                }
            }
        }

        public void OnToolClose()
        {
            m_SelectedLine = null;

            // In case the user closes the tool while editing a line, we need to reset the UI
            UnselectAll();
            ResetDialog();
            m_LastEditedLineData = null;
        }

        async void OnAssetLoaded(IAsset asset, IDataset dataset)
        {
            m_CurrentDataset = dataset;
            var response = await m_Persistence.GetLines(m_CurrentDataset.Descriptor);
            if (response.Data != null)
            {
                CollectionChanged(response.Data.ToList());
            }
        }

        void OnAssetUnloaded()
        {
            m_CurrentDataset = null;
        }

        void EnableClearButton(bool value)
        {
            m_ClearButton.SetEnabled(value);
        }

        void EnableSaveButton(bool value)
        {
            m_SaveButton.SetEnabled(value);
        }

        void EnableApplyButton(bool value)
        {
            m_ApplyButton.SetEnabled(value);
        }

        void OnMeasureDataChanged(MeasureLineData lineData)
        {
            if (lineData == null || lineData.Anchors.Count == 0)
            {
                var oldSelectedItem = m_MeasureListItems.Find(item => item.Id == m_SelectedLine?.Id);
                if (oldSelectedItem != null && oldSelectedItem.IsShown)
                {
                    m_MeasurementToolController.Add(oldSelectedItem.Data);
                }

                UnselectAll();
                EnableSaveButton(false);
                Name = string.Empty;
            }
            else
            {
                Name = lineData.Name;
            }

            m_SelectedLine = lineData;
        }

        void AddMeasureLine(MeasureLineData lineData)
        {
            var item = new MeasureItem(lineData, m_MeasureListItemTemplate);
            item.OnClick += Select;
            item.OnView += OnView;
            item.OnEdit += OnEdit;
            item.OnDelete += OnDelete;

            m_MeasureListItems.Add(item);

            m_ScrollView.contentContainer.Add(item.Root);
        }

        void CollectionChanged(List<MeasureLineData> lineCollection)
        {
            // Keep information about shown items
            var oldShownItems = new List<string>();
            if (m_MeasureListItems.Any())
            {
                foreach (var item in m_MeasureListItems)
                {
                    if (item.IsShown && lineCollection.Any(lineData => lineData.Id == item.Id))
                    {
                        oldShownItems.Add(item.Id);
                    }
                }
            }

            Clear();

            foreach (var lineData in lineCollection)
            {
                AddMeasureLine(lineData);
            }

            if (oldShownItems.Any())
            {
                var shownItems = m_MeasureListItems.Where(item => oldShownItems.Contains(item.Id));
                foreach (var item in shownItems)
                {
                    item.Show(true);
                }
            }
        }

        void Select(MeasureLineData lineData)
        {
            UnselectAll();
            EnableClearButton(false);
            var oldSelectedItem = m_MeasureListItems.Find(item => item.Id == m_SelectedLine?.Id);

            if (lineData.Id == m_SelectedLine?.Id)
            {
                m_MeasurementToolController.StartNewLine();
            }
            else
            {
                var lineClone = MeasureLineData.Clone(lineData);
                lineClone.Color = m_SelectedMeasureColor;
                m_MeasurementToolController.Select(lineClone);

                // Select the item with the matching data
                var selectedItem = m_MeasureListItems.Find(item => item.Id == lineData.Id);
                if (selectedItem != null)
                {
                    selectedItem.Select(true);
                }
            }

            if (oldSelectedItem != null && oldSelectedItem.IsShown)
            {
                m_MeasurementToolController.Add(oldSelectedItem.Data);
            }
        }

        async Task Apply(DatasetDescriptor descriptor)
        {
            if (m_SelectedLine == null)
                return;

            m_SelectedLine.Name = GetMeasureName();
            m_SelectedLine.Color = m_SavedMeasureColor;

            await m_Persistence.SaveLine(descriptor, m_SelectedLine);
            var lineList = await m_Persistence.GetLines(descriptor);

            CollectionChanged(lineList.Data.ToList());
        }

        async Task Save(DatasetDescriptor descriptor)
        {
            if (m_SelectedLine == null)
                return;

            var line = new MeasureLineData(m_SelectedLine.Anchors)
            {
                Name = GetMeasureName(),
                Color = m_SavedMeasureColor
            };

            await m_Persistence.SaveLine(descriptor, line);
            var lineList = await m_Persistence.GetLines(descriptor);
            CollectionChanged(lineList.Data.ToList());

            var listItem = m_MeasureListItems.Find(item => item.Id == line.Id);
            m_MeasurementToolController.Clear();
            listItem?.Show(true);
        }

        string GetMeasureName()
        {
            return string.IsNullOrEmpty(Name) ? "Measure Line" : Name;
        }

        async Task OnDeleteLine(DatasetDescriptor descriptor, MeasureLineData lineData)
        {
            if (lineData == null)
                return;

            m_MeasurementToolController.Delete(lineData);

            await m_Persistence.DeleteLine(descriptor, lineData);
            var lineList = await m_Persistence.GetLines(descriptor);

            CollectionChanged(lineList.Data.ToList());
        }

        void Clear()
        {
            m_ScrollView?.Clear();
            m_MeasureListItems.Clear();
        }

        void OnView(MeasureLineData lineData, bool show)
        {
            if (show)
            {
                m_MeasurementToolController.Add(lineData);
            }
            else
            {
                m_MeasurementToolController.Delete(lineData);
            }
        }

        void OnEdit(MeasureLineData lineData)
        {
            m_LastEditedLineData = lineData;

            var clone = MeasureLineData.Clone(lineData);
            clone.Color = m_SelectedMeasureColor;
            m_MeasurementToolController.Edit(clone, allowCreateNewAnchor: false);
            EnableApplyButton(false);

            Common.Utils.SetVisible(m_ConfirmationContainer, false);
            Common.Utils.SetVisible(m_LineListContainer, false);
            Common.Utils.SetVisible(m_NameContainer, true);
            Common.Utils.SetVisible(m_EditConfirmationContainer, true);
        }

        async void OnDelete(MeasureLineData lineData)
        {
            await OnDeleteLine(m_CurrentDataset.Descriptor, lineData);
        }

        void OnSave()
        {
            var dialog = m_SaveDialogTemplate.Instantiate();
            var nameField = dialog.Q<TextField>("SaveDialogNameField");
            var saveButton = dialog.Q<ActionButton>("SaveDialogFooterSaveButton");
            var cancelButton = dialog.Q<ActionButton>("SaveDialogFooterCancelButton");

            nameField.SetValueWithoutNotify(m_SelectedLine.Name);
            nameField.validateValue += newValue =>
            {
                if (newValue.Length > k_MaxNameLength)
                {
                    nameField.value = newValue.Substring(0, k_MaxNameLength);
                    return false;
                }

                return true;
            };

            var modal = m_Messaging.ShowCustomDialog(dialog);

            saveButton.clicked += async () =>
            {
                m_NameField.SetValueWithoutNotify(nameField.value);
                await Save(m_CurrentDataset.Descriptor);
                modal.Dismiss();
            };

            cancelButton.clicked += () =>
            {
                modal.Dismiss();
            };
        }

        void OnClear()
        {
            var oldSelectedItem = m_MeasureListItems.Find(item => item.Id == m_SelectedLine?.Id);

            m_MeasurementToolController.StartNewLine();

            if (oldSelectedItem != null && oldSelectedItem.IsShown)
            {
                m_MeasurementToolController.Add(oldSelectedItem.Data);
            }

            EnableClearButton(false);
            EnableSaveButton(false);
            UnselectAll();
        }

        async void OnApply()
        {
            ResetDialog();
            await Apply(m_CurrentDataset.Descriptor);

            UnselectAll();

            // Use a clone here because if selected line is not null, the SetSelectedMeasureLineData function
            // will think we toggle off the selected item.
            var clone = MeasureLineData.Clone(m_SelectedLine);
            m_SelectedLine = null;
            Select(clone);
        }
        
        void OnDiscard()
        {
            UnselectAll();
            ResetDialog();

            if (m_LastEditedLineData != null)
            {
                Select(m_LastEditedLineData);
                m_LastEditedLineData = null;
            }
        }

        void ResetDialog()
        {
            Common.Utils.SetVisible(m_NameContainer, false);
            Common.Utils.SetVisible(m_EditConfirmationContainer, false);
            Common.Utils.SetVisible(m_ConfirmationContainer, true);
            Common.Utils.SetVisible(m_LineListContainer, true);
        }

        void UnselectAll()
        {
            // Unselect all items
            foreach (var item in m_MeasureListItems)
            {
                item.Select(false);
            }
        }

        bool ValidateLength(string newValue)
        {
            if (newValue.Length > k_MaxNameLength)
            {
                m_NameField.value = newValue.Substring(0, k_MaxNameLength);
                return false;
            }

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.ObjectSelection;
using Unity.ReferenceProject.UIPanel;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.InputDisabling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;
using Unity.ReferenceProject.InputSystem;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasurementToolController : MonoBehaviour, IInputDisablingOverride
    {
        [SerializeField, Tooltip("Input action asset for the measure tool")]
        InputActionAsset m_InputActionAsset;

        [SerializeField, Tooltip("Template for the draggable pad")]
        VisualTreeAsset m_PadTemplate;

        [SerializeField, Tooltip("Offset of the cursor from the anchor position")]
        Vector2 m_CursorOffset;

        [SerializeReference]
        CursorsController m_CursorsController = new TwoPointMeasurementCreator();

        [SerializeField]
        MeasurementViewer m_MeasurementViewer;

        public event Action<MeasureLineData> MeasureDataChanged;

        readonly double m_Tolerance = 0.1f;

        Dropdown m_UnitDropdown;
        Text m_PanelMeasureText;
        ActionButton m_ApplyButton;
        ActionButton m_SaveButton;
        ActionButton m_DiscardButton;
        ActionButton m_ClearButton;
        DraggableButton m_DraggableButton;

        const string k_MeasureToolSelectInputPath = "Select";
        readonly string[] k_UnitNames = { "@MeasureTool:Meter", "@MeasureTool:Centimeter", "@MeasureTool:Foot", "@MeasureTool:Inch", "@MeasureTool:FootInch" };

        bool m_OnDrag;
        MeasureLineData m_LastEditedLine;
        MeasureLineData m_SelectedLine;
        ICameraProvider m_CameraProvider;

        PropertyValue<GameObject> m_SelectedCursor;

        void SetMeasureData(MeasureLineData data)
        {
            m_SelectedLine = data;
            MeasureDataChanged?.Invoke(data);
        }

        IMainUIPanel m_MainUIPanel;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        IObjectPicker m_Picker;
        PropertyValue<MeasureFormat> m_MeasureFormat;
        PropertyValue<IObjectSelectionInfo> m_SelectionInfo;

        IInputManager m_InputManager;
        InputScheme m_InputScheme;
        bool m_MeasureSelectStarted = false;
        Vector2 m_PointerPosition;

        public GameObject GameObject => gameObject;

        bool m_DestroyWhenDoneEditing;

        [Inject]
        public void Setup(ICameraProvider cameraProvider, MeasureToolDataStore dataStore, IMainUIPanel mainUIPanel,
            ObjectSelectionActivator objectSelectionActivator, PropertyValue<IObjectSelectionInfo> selectionInfo,
            IObjectPicker picker, IInputManager inputManager)
        {
            m_CameraProvider = cameraProvider;
            m_MainUIPanel = mainUIPanel;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_SelectionInfo = selectionInfo;
            m_Picker = picker;
            m_InputManager = inputManager;

            m_SelectedCursor = dataStore.GetProperty<GameObject>(nameof(MeasureToolViewModel.SelectedCursor));
            m_MeasureFormat = dataStore.GetProperty<MeasureFormat>(nameof(MeasureToolViewModel.MeasureFormat));

            var anchorPicker = new ObjectPickerAnchorSelector();
            m_CursorsController.Setup(m_SelectedCursor, anchorPicker, selectionInfo);
        }

        void Awake()
        {
            m_SelectedCursor.ValueChanged += OnSelectedCursorChanged;
            InitializeInputs();
        }

        void OnDestroy()
        {
            m_UnitDropdown.UnregisterValueChangedCallback(OnUnitChanged);
            m_DiscardButton.clickable.clicked -= OnDiscard;

            m_SelectedCursor.ValueChanged -= OnSelectedCursorChanged;
            m_InputScheme.Dispose();
        }

        void Update()
        {
            var cursor = m_SelectedCursor?.GetValue();

            if (cursor)
            {
                var cursorScreenPoint = m_CameraProvider.Camera.WorldToScreenPoint(cursor.transform.position);
                if (m_DraggableButton != null &&
                    (m_DraggableButton.transform.position - cursorScreenPoint).magnitude >= m_Tolerance)
                {
                    // z position is checked to avoid back camera view
                    if (cursorScreenPoint.z <= 0 && m_DraggableButton.visible)
                    {
                        m_DraggableButton.style.display = DisplayStyle.None;
                    }
                    else if (cursorScreenPoint.z > 0 && !m_OnDrag)
                    {
                        if (!m_DraggableButton.visible)
                            m_DraggableButton.style.display = DisplayStyle.Flex;

                        cursorScreenPoint.x += m_CursorOffset.x;
                        cursorScreenPoint.y += m_CursorOffset.y;
                        SetDraggablePosition(cursorScreenPoint);
                    }
                }
            }

            UpdatePanelText();
        }

        public void InitializeUI(VisualElement root)
        {
            m_UnitDropdown = root.Q<Dropdown>("line_creator_unit_dropdown");
            m_ClearButton = root.Q<ActionButton>("line_creator_confirmation_clear");
            m_SaveButton = root.Q<ActionButton>("line_creator_confirmation_save");
            m_ApplyButton = root.Q<ActionButton>("line_creator_edit_confirmation_apply");
            m_DiscardButton = root.Q<ActionButton>("line_creator_edit_confirmation_discard");

            m_PanelMeasureText = root.Q<Text>("line_creator_distance_value");

            m_UnitDropdown.RegisterValueChangedCallback(OnUnitChanged);

            m_DiscardButton.clickable.clicked += OnDiscard;

            m_DraggableButton = new DraggableButton(m_PadTemplate, OnDrag, OnDown, OnUp)
            {
                style =
                {
                    display = DisplayStyle.None,
                    position = Position.Absolute
                }
            };

            m_MainUIPanel.Panel.Insert(0, m_DraggableButton);

            SetupOptions();
        }

        public void Clear()
        {
            m_CursorsController.Clear();
            m_SelectedCursor.SetValue((GameObject)null);
            m_SelectionInfo.ValueChanged -= OnSelectionInfoChangedWhileSelecting;
        }

        void SetupUnitDropdown()
        {
            for (int i = 0; i < k_UnitNames.Length; i++)
            {
                m_UnitDropdown.bindItem = (item, i) => item.label = k_UnitNames[i];
                m_UnitDropdown.sourceItems = k_UnitNames;
            }

            m_UnitDropdown.value = new[] { 0 };
        }

        void SetupOptions()
        {
            SetupUnitDropdown();
        }

        void OnUnitChanged(ChangeEvent<IEnumerable<int>> changeEvent)
        {
            m_MeasureFormat.SetValue((MeasureFormat)changeEvent.newValue.First());
            UpdatePanelText();
        }

        void OnDiscard()
        {
            if (m_LastEditedLine != null)
            {
                m_MeasurementViewer.AddMeasureLineData(m_LastEditedLine);
                m_LastEditedLine = null;
            }

            Edit(new MeasureLineData());
        }

        public void StartNewLine()
        {
            Edit(new MeasureLineData());
            Clear();
        }

        public void Select(MeasureLineData lineData)
        {
            StopEditing();

            SetMeasureData(lineData);
            m_MeasurementViewer.AddMeasureLineData(lineData);
            UpdatePanelText();

            m_ObjectSelectionActivator.Subscribe(this);
            m_SelectionInfo.ValueChanged += OnSelectionInfoChangedWhileSelecting;
        }

        void OnSelectionInfoChangedWhileSelecting(IObjectSelectionInfo selectionInfo)
        {
            if (selectionInfo == null || float.IsNaN(selectionInfo.SelectedPosition.x))
                return;

            var selectedPosition = selectionInfo.SelectedPosition;

            StartNewLine();

            m_CursorsController.CreateAnchorAtWorldPosition(selectedPosition, selectionInfo.SelectedNormal);
        }

        public void Edit(MeasureLineData lineData, bool allowCreateNewAnchor = true, bool destroyWhenDoneEditing = true)
        {
            StopEditing();

            m_DestroyWhenDoneEditing = destroyWhenDoneEditing;

            if (!m_DestroyWhenDoneEditing)
            {
                m_LastEditedLine = MeasureLineData.Clone(lineData);
            }
            else
            {
                m_LastEditedLine = null;
            }

            SetMeasureData(lineData);
            m_MeasurementViewer.AddMeasureLineData(lineData);
            if (allowCreateNewAnchor)
            {
                m_ObjectSelectionActivator.Subscribe(this);
            }

            m_CursorsController.Open(lineData);
            UpdatePanelText();

            m_InputScheme.SetEnable(true);
        }

        void InitializeInputs()
        {
            m_InputScheme = m_InputManager.GetOrCreateInputScheme(InputSchemeType.Measurement, InputSchemeCategory.Tools, m_InputActionAsset);
            m_InputScheme[k_MeasureToolSelectInputPath].OnStarted += MeasureSelectStarted;
            m_InputScheme[k_MeasureToolSelectInputPath].OnPerformed += OnPointerUp;
            m_InputScheme[k_MeasureToolSelectInputPath].RegisterValidationPeformedFunc(IsMeasureSelectPerformValid);
            m_InputScheme[k_MeasureToolSelectInputPath].OnCanceled += MeasureSelectCanceled;
            m_InputScheme[k_MeasureToolSelectInputPath].IsUISelectionCheckEnabled = true;
        }

        public void Add(MeasureLineData lineData)
        {
            m_MeasurementViewer.AddMeasureLineData(lineData);
        }

        public void Delete(MeasureLineData lineData)
        {
            m_MeasurementViewer.RemoveMeasureLineData(lineData);
        }

        public void RemoveAll()
        {
            m_MeasurementViewer.RemoveAllMeasureLineData();
        }

        public void StopEditing()
        {
            if (m_DestroyWhenDoneEditing && m_SelectedLine != null)
            {
                m_SelectionInfo.ValueChanged -= OnSelectionInfoChangedWhileSelecting;
                m_MeasurementViewer.RemoveMeasureLineData(m_SelectedLine);
            }

            SetMeasureData(null);

            if (m_DraggableButton != null)
            {
                Common.Utils.SetVisible(m_DraggableButton, false);
            }

            m_ObjectSelectionActivator.Unsubscribe(this);
            m_CursorsController.Close();

            m_InputScheme.SetEnable(false);
        }

        void MeasureSelectStarted(InputAction.CallbackContext _)
        {
            m_MeasureSelectStarted = true;
            m_PointerPosition = Pointer.current.position.ReadValue();
        }

        void MeasureSelectCanceled(InputAction.CallbackContext _)
        {
            m_MeasureSelectStarted = false;
        }

        bool IsMeasureSelectPerformValid(InputAction.CallbackContext _)
        {
            return m_MeasureSelectStarted;
        }

        void OnPointerUp(InputAction.CallbackContext input)
        {
            if (m_CameraProvider != null)
            {
                var ray = m_CameraProvider.Camera.ScreenPointToRay(m_PointerPosition);
                if (Physics.Raycast(ray, out var hitInfo))
                {
                    var go = hitInfo.collider.gameObject;
                    if (m_CursorsController.IsCursor(go))
                    {
                        m_SelectedCursor.SetValue(go);
                    }
                }
            }

            m_MeasureSelectStarted = false;
        }

        async void OnDragPositionDataChanged(Vector3 dragChange)
        {
            Vector2 draggablePoint = dragChange;
            SetDraggablePosition(draggablePoint);

            draggablePoint.x -= m_CursorOffset.x;
            draggablePoint.y -= m_CursorOffset.y;

            var ray = m_CameraProvider.Camera.ScreenPointToRay(draggablePoint);
            var result = await m_Picker.RaycastAsync(ray);

            if (result.HasIntersected)
            {
                m_CursorsController.UpdateDraggableWorldPosition(result.Point, result.Normal);
            }
        }

        void OnSelectedCursorChanged(GameObject cursor)
        {
            if (m_DraggableButton == null)
                return;

            Common.Utils.SetVisible(m_DraggableButton, cursor != null);

            if (cursor != null)
            {
                m_DraggableButton.style.top = new StyleLength(m_DraggableButton.style.top.value.value - cursor.transform.position.x);
            }

            if (m_SelectedLine != null && m_SelectedLine.Anchors != null)
            {
                m_ClearButton.SetEnabled(m_SelectedLine.Anchors.Count > 0);
                m_SaveButton.SetEnabled(m_SelectedLine.Anchors.Count > 1);
            }
            else
            {
                m_ClearButton.SetEnabled(false);
                m_SaveButton.SetEnabled(false);
            }
        }

        void OnDown(Vector3 position)
        {
            m_OnDrag = true;
            OnDragPositionDataChanged(position);
            m_ApplyButton.SetEnabled(true);
        }

        void OnDrag(Vector3 position)
        {
            OnDragPositionDataChanged(position);
        }

        void OnUp(Vector3 position)
        {
            m_OnDrag = false;
            OnDragPositionDataChanged(position);
        }

        void SetDraggablePosition(Vector2 screenPos)
        {
            var localPoint = new Vector2(screenPos.x / Screen.width, 1 - screenPos.y / Screen.height);

            localPoint *= m_DraggableButton.parent.layout.size;
            localPoint -= m_DraggableButton.parent.layout.position;

            m_DraggableButton.style.left = localPoint.x - (m_DraggableButton.layout.width / 2);
            m_DraggableButton.style.top = localPoint.y - (m_DraggableButton.layout.height / 2);
        }

        void UpdatePanelText()
        {
            m_PanelMeasureText.text = m_SelectedLine != null ? m_SelectedLine.GetFormattedDistanceString(m_MeasureFormat.GetValue()) : string.Empty;
        }
    }
}

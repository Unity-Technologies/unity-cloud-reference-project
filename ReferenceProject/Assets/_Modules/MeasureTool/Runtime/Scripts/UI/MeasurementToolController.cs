using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using Unity.ReferenceProject.Messaging;

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
        public event Action<MeasureFormat, MeasureLineData> SystemUnitChanged;

        readonly double m_Tolerance = 0.1f;

        Dropdown m_UnitDropdown;
        Dropdown m_ModeDropdown;
        Text m_PanelMeasureText;
        ActionButton m_ApplyButton;
        ActionButton m_SaveButton;
        ActionButton m_DiscardButton;
        ActionButton m_ClearButton;
        DraggableButton m_DraggableButton;

        const string k_MeasureToolSelectInputPath = "Select";
        readonly List<string> k_ModeNames = new List<string>();
        
        const string k_InfiniteOrthogonalWarning = "@MeasureTool:InfiniteOrthogonal";
        
        bool m_OnDrag;
        MeasureLineData m_LastEditedLine;
        MeasureLineData m_SelectedLine;
        ICameraProvider m_CameraProvider;
        IAppMessaging m_AppMessaging;

        PropertyValue<GameObject> m_SelectedCursor;

        void SetMeasureData(MeasureLineData data)
        {
            m_SelectedLine = data;
            MeasureDataChanged?.Invoke(data);
        }

        IMainUIPanel m_MainUIPanel;
        ObjectSelectionActivator m_ObjectSelectionActivator;
        IObjectPicker m_Picker;
        PropertyValue<IObjectSelectionInfo> m_SelectionInfo;

        IInputManager m_InputManager;
        IAppUnit m_AppUnit;
        InputScheme m_InputScheme;
        bool m_MeasureSelectStarted = false;
        Vector2 m_PointerPosition;

        public GameObject GameObject => gameObject;

        bool m_DestroyWhenDoneEditing;
        
        MeasureFormat[] m_MeasureFormats;
        MeasureFormat m_CurrentMeasureFormat;
        public MeasureFormat CurrentMeasureFormat => m_CurrentMeasureFormat;
        bool m_HasMeasureFormatOverride;
        public bool hasMeasureFormatOverride => m_HasMeasureFormatOverride;
        string[] m_DropdownOptions;
        
        [Inject]
        public void Setup(ICameraProvider cameraProvider, MeasureToolDataStore dataStore, IMainUIPanel mainUIPanel, //NOSONAR
            ObjectSelectionActivator objectSelectionActivator, PropertyValue<IObjectSelectionInfo> selectionInfo, //NOSONAR
            IObjectPicker picker, IInputManager inputManager, IAppMessaging appMessaging, IAppUnit appUnit) //NOSONAR
        {
            m_CameraProvider = cameraProvider;
            m_MainUIPanel = mainUIPanel;
            m_ObjectSelectionActivator = objectSelectionActivator;
            m_SelectionInfo = selectionInfo;
            m_Picker = picker;
            m_InputManager = inputManager;
            m_AppMessaging = appMessaging;

            m_SelectedCursor = dataStore.GetProperty<GameObject>(nameof(MeasureToolViewModel.SelectedCursor));

            var anchorPicker = new ObjectPickerAnchorSelector();
            m_CursorsController.Setup(m_SelectedCursor, anchorPicker, selectionInfo);
            
            m_AppUnit = appUnit;
        }

        void Awake()
        {
            m_SelectedCursor.ValueChanged += OnSelectedCursorChanged;
            m_AppUnit.SystemUnitChanged += OnGlobalSystemUnitChanged;
            m_MeasureFormats = m_AppUnit.GetMeasureFormat();
            
            InitializeInputs();
        }

        void OnDestroy()
        {
            m_UnitDropdown.UnregisterValueChangedCallback(OnUnitChanged);
            m_ModeDropdown.UnregisterValueChangedCallback(OnModeChanged);
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
            m_ModeDropdown = root.Q<Dropdown>("line_creator_mode_dropdown");
            m_ClearButton = root.Q<ActionButton>("line_creator_confirmation_clear");
            m_SaveButton = root.Q<ActionButton>("line_creator_confirmation_save");
            m_ApplyButton = root.Q<ActionButton>("line_creator_edit_confirmation_apply");
            m_DiscardButton = root.Q<ActionButton>("line_creator_edit_confirmation_discard");

            m_PanelMeasureText = root.Q<Text>("line_creator_distance_value");

            m_UnitDropdown.RegisterValueChangedCallback(OnUnitChanged);
            m_ModeDropdown.RegisterValueChangedCallback(OnModeChanged);

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
            var unitFormat = m_AppUnit.GetMeasureFormat();
            m_DropdownOptions = new string[unitFormat.Length + 1];
            m_DropdownOptions[0] = "@MeasureTool:AsSystem";

            for (var i = 1; i <= unitFormat.Length; i++)
            {
                m_DropdownOptions[i] = $"@MeasureTool:{unitFormat[i-1]}";
            }

            foreach (var unit in m_DropdownOptions)
            {
                m_UnitDropdown.bindItem = (item, index) => item.label = m_DropdownOptions[index];
                m_UnitDropdown.sourceItems = m_DropdownOptions;
            }

            m_UnitDropdown.value = new[] { Array.IndexOf(m_DropdownOptions,"@MeasureTool:" + m_AppUnit.GetSystemUnit()) };
        }
        
        void SetupModeDropdown()
        {
            foreach (var mode in Enum.GetValues(typeof(MeasureMode)))
            {
                var modeName = "@MeasureTool:" + Enum.GetName(typeof(MeasureMode), mode);
                k_ModeNames.Add(modeName);
            }
            
            for (int i = 0; i < k_ModeNames.Count; i++)
            {
                m_ModeDropdown.bindItem = (item, i) => item.label = k_ModeNames[i];
                m_ModeDropdown.sourceItems = k_ModeNames;
            }
            
            m_ModeDropdown.value = new[] { 0 };
        }

        void SetupOptions()
        {
            SetupUnitDropdown();
            SetupModeDropdown();
        }

        void OnUnitChanged(ChangeEvent<IEnumerable<int>> changeEvent)
        {
            m_ApplyButton.SetEnabled(true);
            MeasureFormat newMeasureFormat;
            
            if (changeEvent.newValue.First() == 0)
            {
                newMeasureFormat = m_AppUnit.GetSystemUnit();
                m_HasMeasureFormatOverride = false;
            }
            else
            { 
                newMeasureFormat = m_MeasureFormats[changeEvent.newValue.First()-1];
                m_HasMeasureFormatOverride = true;
            }
            
            m_CurrentMeasureFormat = m_SelectedLine.MeasureFormat = newMeasureFormat;
            m_SelectedLine.HasMeasureFormatOverride = m_HasMeasureFormatOverride;
            
            UpdatePanelText();
        }
        
        void OnGlobalSystemUnitChanged(MeasureFormat measureFormat)
        { 
            m_CurrentMeasureFormat = measureFormat;
            SystemUnitChanged?.Invoke(measureFormat, m_SelectedLine);
            if (m_SelectedLine != null)
            {
                m_UnitDropdown.SetValueWithoutNotify(new[] { Array.IndexOf(m_MeasureFormats, m_SelectedLine.
                MeasureFormat)});
                UpdatePanelText();
            }
            UpdateDropdown();
        }

        async void OnModeChanged(ChangeEvent<IEnumerable<int>> changeEvent)
        {
            var lineData = m_SelectedLine;
            lineData.MeasureMode = (MeasureMode)changeEvent.newValue.First();
            SetMeasureData(lineData);
            await HandleOrthogonalMode();
        }

        MeasureMode GetModeDropdownValue()
        {
            return (MeasureMode)m_ModeDropdown.value.First();
        }

        void OnDiscard()
        {
            if (m_LastEditedLine != null)
            {
                m_MeasurementViewer.AddMeasureLineData(m_LastEditedLine);
                m_LastEditedLine = null;
            }

            Edit(new MeasureLineData(m_AppUnit.GetSystemUnit(), true));
        }

        public void StartNewLine()
        {
            m_UnitDropdown.SetValueWithoutNotify(new[] {0}); 
            Edit(new MeasureLineData(m_AppUnit.GetSystemUnit(), true));
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
            UpdateDropdown();
        }

        void UpdateDropdown()
        {
            if(m_SelectedLine == null || m_UnitDropdown == null)
                return;
                
            if (!m_SelectedLine.HasMeasureFormatOverride)
            {
                m_UnitDropdown.SetValueWithoutNotify(new[] { 0 });
            }
            else
            {
                m_UnitDropdown.SetValueWithoutNotify(new[] { Array.IndexOf(m_DropdownOptions, "@MeasureTool:" + m_SelectedLine.
                MeasureFormat)}); 
            }
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
            var result = await m_Picker.PickAsync(ray);

            if (result.HasIntersected)
            {
                m_CursorsController.UpdateDraggableWorldPosition(result.Point, result.Normal);
            }
        }

        async void OnSelectedCursorChanged(GameObject cursor)
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
                m_UnitDropdown.SetEnabled(m_SelectedLine.Anchors.Count > 0);
                
                await HandleOrthogonalMode();
            }
            else
            {
                m_ClearButton.SetEnabled(false);
                m_SaveButton.SetEnabled(false);
                m_UnitDropdown.SetEnabled(false);
            }
        }
        
        async Task HandleOrthogonalMode()
        {
            var dropdownVal = GetModeDropdownValue();
            if (m_SelectedLine.MeasureMode != dropdownVal)
            {
                m_SelectedLine.MeasureMode = dropdownVal;
                SetMeasureData(m_SelectedLine);
            }
            
            var anchors = m_SelectedLine.Anchors;
            if (anchors.Count == 1 && m_SelectedLine.MeasureMode == MeasureMode.Orthogonal)
            {
                var raycast = await m_Picker.PickAsync(new Ray(anchors[0].Position, anchors[0].Normal));
                if (raycast.HasIntersected)
                {
                    m_CursorsController.CreateAnchorAtWorldPosition(raycast.Point, raycast.Normal);
                }
                else
                {
                    m_AppMessaging.ShowWarning(k_InfiniteOrthogonalWarning);
                }
            }
            
            m_ModeDropdown.SetEnabled(anchors.Count < 2); 
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

        async void OnUp(Vector3 position)
        {
            m_OnDrag = false;
            OnDragPositionDataChanged(position);
            await HandleOrthogonalMode();
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
            m_PanelMeasureText.text = m_SelectedLine != null ? m_SelectedLine.
            GetFormattedDistanceString(m_SelectedLine.MeasureFormat) : string.Empty;
        }

        public void UpdateLines(List<MeasureLineData> lines)
        {
            foreach (var measureLineData in lines)
            {
                m_MeasurementViewer.UpdateLines(measureLineData);
            }
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.Dt.App.UI.Button;

namespace Unity.ReferenceProject.Instructions
{
    public class InstructionsUIController : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;
        
        [SerializeField]
        VisualTreeAsset m_InstructionsPanelAsset;

        [SerializeField]
        string m_HeaderText;

        [SerializeField]
        string m_PlayerPrefsKey;

        public event Action<bool> InstructionPanelEnabled;
        public event Action<bool> InstructionsAvailable;
        public bool IsInstructionsEnabled => m_UIDocument && m_UIDocument.rootVisualElement.style.display == DisplayStyle.Flex;

        Checkbox m_CheckboxDontShowAgain;

        static readonly string k_CheckboxDontShowAgain = "Checkbox_dont-show-again";
        static readonly string k_ButtonClose = "Button-close";
        static readonly string k_TextHeader = "Text-header";
        static readonly string k_InstructionsContainer = "Instructions-container";

        void Start()
        {
            InitializeUI(m_UIDocument);
        }
        
        public void InitializeUI(UIDocument uiDocument)
        {
            if(uiDocument == null)
                return;

            m_UIDocument = uiDocument;

            var root = uiDocument.rootVisualElement;
            var panel = root.Q<Panel>();
            if (panel != null)
            {
                root = panel;
            }
            
            var template = m_InstructionsPanelAsset.Instantiate();
            template.pickingMode = PickingMode.Ignore;
            template.style.flexGrow = 1;
            template.style.flexShrink = 0;
            
            root.Add(template);

            m_CheckboxDontShowAgain = root.Q<Checkbox>(k_CheckboxDontShowAgain);
            var buttonClose = root.Q<Button>(k_ButtonClose);
            var textHeader = root.Q<Text>(k_TextHeader);
            var instructionPageView = template.Q<VisualElement>(k_InstructionsContainer);
            
            if (buttonClose != null)
            {
                buttonClose.clicked += OnButtonCloseClicked;
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(Button)} {nameof(buttonClose)} has not been found by key {k_ButtonClose} at {nameof(UIDocument)}. Close panel functionality is disabled.");
            }

            if (m_CheckboxDontShowAgain != null)
            {
                m_CheckboxDontShowAgain.RegisterValueChangedCallback(OnToggleValueChanged);
                UpdateCheckboxDontShowAgain();
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(Checkbox)} {nameof(m_CheckboxDontShowAgain)} has not been found by key {k_CheckboxDontShowAgain} at {nameof(UIDocument)}. Don't show functionality is disabled.");
            }

            if (textHeader != null)
            {
                textHeader.text = m_HeaderText;
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(Text)} {nameof(textHeader)} has not been found by key {k_TextHeader} at {nameof(UIDocument)}.");
            }
            
            if (instructionPageView != null)
            {
                InitializeEntries(instructionPageView);
            }
            else
            {
                Debug.LogWarning(
                    $"{nameof(VisualElement)} {nameof(instructionPageView)} has not been found by key {k_InstructionsContainer} at {nameof(UIDocument)}.");
            }
        }
        
        void InitializeEntries(VisualElement container)
        {
            if (container != null)
            {
                var entries = GetComponents<InstructionUIEntry>();
                foreach (var entry in entries)
                {
                    entry.AddInstructions(container);
                }
            }

            if (container == null || container.childCount == 0)
            {
                SetVisiblePanel(false);
                InstructionsAvailable?.Invoke(false);
            }
            else
            {
                SetVisiblePanel(GetCheckboxValue() == CheckboxState.Unchecked);
                InstructionsAvailable?.Invoke(true);
            }
        }

        public void SetVisiblePanel(bool isVisible)
        {
            if (m_UIDocument)
            {
                m_UIDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
            }

            InstructionPanelEnabled?.Invoke(isVisible);

            if (isVisible)
            {
                UpdateCheckboxDontShowAgain();
            }
        }
        
        void UpdateCheckboxDontShowAgain()
        {
            if (m_CheckboxDontShowAgain != null)
            {
                m_CheckboxDontShowAgain.SetValueWithoutNotify( GetCheckboxValue() );
            }
        }
        
        public CheckboxState GetCheckboxValue() => PlayerPrefs.GetInt(m_PlayerPrefsKey, 0) == 0 ? CheckboxState.Unchecked : CheckboxState.Checked;
        
        void OnToggleValueChanged(ChangeEvent<CheckboxState> evt) => PlayerPrefs.SetInt(m_PlayerPrefsKey, evt.newValue == CheckboxState.Checked ? 1 : 0);

        void OnButtonCloseClicked() => SetVisiblePanel(false);
    }
}

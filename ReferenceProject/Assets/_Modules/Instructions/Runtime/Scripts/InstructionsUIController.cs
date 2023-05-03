using System;
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
        string m_HeaderText;

        [SerializeField]
        string m_PlayerPrefsKey;

        public event Action<bool> OnInstructionPanelEnabled;
        public bool IsInstructionsEnabled => m_UIDocument && m_UIDocument.rootVisualElement.style.display == DisplayStyle.Flex;

        Checkbox m_CheckboxDontShowAgain;

        static readonly string k_CheckboxDontShowAgain = "Checkbox_dont-show-again";
        static readonly string k_ButtonClose = "Button-close";
        static readonly string k_TextHeader = "Text-header";

        void Awake()
        {
            if (m_UIDocument != null)
            {
                InitializeUI(m_UIDocument);
                SetVisiblePanel(GetCheckboxValue() == CheckboxState.Unchecked);
            }
        }

        public void InitializeUI(UIDocument uiDocument)
        {
            var root = uiDocument.rootVisualElement;
            m_CheckboxDontShowAgain = root.Q<Checkbox>(k_CheckboxDontShowAgain);
            var buttonClose = root.Q<Button>(k_ButtonClose);
            var textHeader = root.Q<Text>(k_TextHeader);

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
        }

        public void SetVisiblePanel(bool isVisible)
        {
            if (m_UIDocument)
            {
                m_UIDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
            }

            OnInstructionPanelEnabled?.Invoke(isVisible);

            if (isVisible)
            {
                UpdateCheckboxDontShowAgain();
            }
        }

        public CheckboxState GetCheckboxValue()
        {
            return PlayerPrefs.GetInt(m_PlayerPrefsKey, 0) == 0 ? CheckboxState.Unchecked : CheckboxState.Checked;
        }

        void UpdateCheckboxDontShowAgain()
        {
            if (m_CheckboxDontShowAgain != null)
            {
                m_CheckboxDontShowAgain.SetValueWithoutNotify( GetCheckboxValue() );
            }
        }

        void OnToggleValueChanged(ChangeEvent<CheckboxState> evt) => PlayerPrefs.SetInt(m_PlayerPrefsKey, evt.newValue == CheckboxState.Checked ? 1 : 0);

        void OnButtonCloseClicked() => SetVisiblePanel(false);
    }
}

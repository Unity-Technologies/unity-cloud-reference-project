using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.CustomKeyboard
{
    public interface IKeyboardController
    {
        public event Action KeyboardOpened;
        public event Action KeyboardClosed;
        public VisualElement RootVisualElement { get; }
        public TextInputBaseField<string> TextField { get; }
        public void OpenKeyboard(TextInputBaseField<string> textField);
        public void ForceTextFieldFocus();
    }

    public class KeyboardController : IKeyboardController
    {
        public KeyboardLayout Layout { get; set; }
        public StyleSheet Style { get; set; }
        public TextInputBaseField<string> TextField => m_TextField;

        public VisualElement RootVisualElement => m_KeyboardRoot ?? CreateVisual();

        VisualElement m_KeyboardRoot;
        VisualElement m_MainLayout;
        VisualElement m_CurrentLayout;
        TextInputBaseField<string> m_TextField;

        bool m_ReturnMainLayout;
        bool m_HasPopoverShownDismissed;
        Popover m_ExtendedCharacterPopover;
        IVisualElementScheduledItem m_DeferLongPress;
        char m_SelectedExtendedCharacter;
        int m_StartIndex;
        int m_EndIndex;

        readonly Dictionary<KeyboardLayout, VisualElement> m_Layouts = new();

        readonly static int k_FirstLongPressDelay = 1000;
        readonly static int k_LongPressDelay = 100;
        readonly static int k_KeySize = 32;

        public event Action KeyboardOpened;
        public event Action KeyboardClosed;

        public void OpenKeyboard(TextInputBaseField<string> textField)
        {
            if (m_TextField != null)
            {
                m_TextField.UnregisterCallback<FocusOutEvent>(OnTextFieldFocusOut);
            }

            m_TextField = textField;
            textField.RegisterCallback<FocusOutEvent>(OnTextFieldFocusOut);
            KeyboardOpened?.Invoke();
        }

        public void ForceTextFieldFocus()
        {
            if (m_TextField == null)
                return;

            m_TextField.Focus();

            if (string.IsNullOrEmpty(m_TextField.text))
                return;

            int size = m_TextField.text.Length;

            SendKeyEvent(EventType.KeyDown, KeyCode.RightArrow, '\0', EventModifiers.FunctionKey);

            if (m_StartIndex == size)
                return;

            for(int i=size; i>m_EndIndex; i--)
            {
                SendKeyEvent(EventType.KeyDown, KeyCode.LeftArrow, '\0', EventModifiers.FunctionKey);
            }

            for(int i=m_EndIndex; i>m_StartIndex; i--)
            {
                SendKeyEvent(EventType.KeyDown, KeyCode.Backspace, '\0', EventModifiers.FunctionKey);
            }
        }

        void OnTextFieldFocusOut(FocusOutEvent evt)
        {
            m_StartIndex = Math.Min(m_TextField.cursorIndex, m_TextField.selectIndex);
            m_EndIndex = Math.Max(m_TextField.cursorIndex, m_TextField.selectIndex);

            if (m_HasPopoverShownDismissed)
            {
                m_HasPopoverShownDismissed = false;

                var deferForceFocus = m_TextField.schedule.Execute(() =>
                {
                    if (!Utils.IsFocused(m_TextField))
                    {
                        ForceTextFieldFocus();
                    }
                });
                deferForceFocus.ExecuteLater(1);
            }
            else
            {
                KeyboardClosed?.Invoke();
            }
        }

        VisualElement CreateVisual()
        {
            m_KeyboardRoot = new VisualElement();
            m_KeyboardRoot.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopImmediatePropagation();
                evt.PreventDefault();

                if(!Utils.IsFocused(m_TextField))
                {
                    ForceTextFieldFocus();
                }
            });
            m_KeyboardRoot.styleSheets.Add(Style);
            m_KeyboardRoot.AddToClassList("keyboard-background");

            m_MainLayout = CreateLayout(Layout);
            m_CurrentLayout = m_MainLayout;
            ShowLayout(m_MainLayout);

            return m_KeyboardRoot;
        }

        VisualElement CreateLayout(KeyboardLayout layout)
        {
            var visualElement = new VisualElement();
            m_Layouts.Add(layout, visualElement);

            // Create rows
            foreach (var keyRow in layout.KeyRows)
            {
                var row = new VisualElement();
                row.AddToClassList("keyboard-row");
                visualElement.Add(row);

                // Create keys
                foreach (var keyData in keyRow.Keys)
                {
                    var keyButton = new ActionButton();
                    keyButton.focusable = false;
                    keyButton.AddToClassList("keyboard-key");
                    keyButton.style.width = k_KeySize * keyData.Size;
                    row.Add(keyButton);

                    switch (keyData.Type)
                    {
                        case KeyType.Character:
                            CreateCharacterKey(keyData, keyButton);
                            break;
                        case KeyType.ExtendedCharacter:
                            CreateExtendedCharacterKey(keyData, keyButton);
                            break;
                        case KeyType.SwitchLayout:
                            CreateSwitchLayoutKey(keyData, keyButton);
                            break;
                        case KeyType.Ascii:
                            CreateAsciiCodeKey(keyData, keyButton);
                            break;
                        case KeyType.Functional:
                            CreateFunctionalKey(keyData, keyButton);
                            break;
                        case KeyType.EmptySpace:
                            CreateEmptySpace(keyButton);
                            break;
                    }
                }
            }

            m_KeyboardRoot.Add(visualElement);
            HideLayout(visualElement);

            return visualElement;
        }

        void CreateCharacterKey(KeyData data, ActionButton button)
        {
            button.label = data.Character.ToString();
            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.PreventDefault();

                SendKeyDownEvent(KeyCode.A);
                SendKeyDownEvent(data.Character);
                SendKeyUpEvent(KeyCode.A);

                if (m_ReturnMainLayout)
                {
                    m_ReturnMainLayout = false;
                    HideLayout(m_CurrentLayout);
                    ShowLayout(m_MainLayout);
                    m_CurrentLayout = m_MainLayout;
                }
            });
        }

        void CreateExtendedCharacterKey(KeyData data, ActionButton button)
        {
            button.label = data.Character.ToString();
            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.PreventDefault();
                m_SelectedExtendedCharacter = '\0';

                m_DeferLongPress = button.schedule.Execute(() =>
                {
                    OpenExtendedCharactersPopover(button, data);
                });
                m_DeferLongPress.ExecuteLater(k_FirstLongPressDelay);
            });

            button.RegisterCallback<PointerUpEvent>(evt =>
            {
                if(m_ExtendedCharacterPopover != null)
                {
                    m_ExtendedCharacterPopover.Dismiss();
                    m_HasPopoverShownDismissed = true;

                    SendKeyDownEvent(KeyCode.A);
                    SendKeyDownEvent(m_SelectedExtendedCharacter != '\0' ? m_SelectedExtendedCharacter : data.Character);
                    SendKeyUpEvent(KeyCode.A);
                }
                else
                {
                    m_DeferLongPress?.Pause();
                    m_DeferLongPress = null;

                    SendKeyDownEvent(KeyCode.A);
                    SendKeyDownEvent(data.Character);
                    SendKeyUpEvent(KeyCode.A);
                }

                if (m_ReturnMainLayout)
                {
                    m_ReturnMainLayout = false;
                    HideLayout(m_CurrentLayout);
                    ShowLayout(m_MainLayout);
                    m_CurrentLayout = m_MainLayout;
                }
            });
        }

        void OpenExtendedCharactersPopover(ActionButton button, KeyData data)
        {
            var extendedCharacterPanel = new VisualElement();
            extendedCharacterPanel.styleSheets.Add(Style);
            extendedCharacterPanel.AddToClassList("keyboard-extended-character-panel");
            var isMultiline = data.ExtendedCharacters.Contains('|');
            var nbLines = isMultiline ? data.ExtendedCharacters.Split('|').Length : 1;
            var lineLength = isMultiline ? (data.ExtendedCharacters.Length - (nbLines - 1)) / nbLines : data.ExtendedCharacters.Length;
            extendedCharacterPanel.style.width = k_KeySize * lineLength;
            extendedCharacterPanel.style.height = k_KeySize * nbLines;
            ActionButton selectedKeyButton = null;
            for (int i = 0; i < nbLines; i++)
            {
                var keyRow = new VisualElement();
                keyRow.AddToClassList("keyboard-extended-character-key-row");
                for (int j = 0; j < lineLength; j++)
                {
                    var keyButton = new ActionButton();
                    keyButton.focusable = false;
                    keyButton.AddToClassList("keyboard-extended-character-key");
                    keyButton.style.width = k_KeySize;
                    keyButton.label = data.ExtendedCharacters[i*lineLength + j + i].ToString();
                    keyRow.Add(keyButton);

                    keyButton.RegisterCallback<PointerEnterEvent>(evt =>
                    {
                        m_SelectedExtendedCharacter = keyButton.label[0];

                        if (selectedKeyButton != keyButton)
                        {
                            selectedKeyButton?.RemoveFromClassList("hover");
                            selectedKeyButton = keyButton;
                            keyButton.AddToClassList("hover");
                        }
                    });
                }
                extendedCharacterPanel.Add(keyRow);
            }
            m_ExtendedCharacterPopover = Popover.Build(button, extendedCharacterPanel)
                .SetPlacement(PopoverPlacement.Top);
            m_HasPopoverShownDismissed = true;
            m_SelectedExtendedCharacter = '\0';
            m_ExtendedCharacterPopover.Show();
            m_ExtendedCharacterPopover.dismissed += (_, _) =>
            {
                m_ExtendedCharacterPopover = null;
            };
        }

        void CreateSwitchLayoutKey(KeyData data, ActionButton button)
        {
            KeyIcon(data, button);

            VisualElement layout;
            if (!m_Layouts.TryGetValue(data.Layout, out layout))
            {
                layout = CreateLayout(data.Layout);
            }

            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.PreventDefault();

                HideLayout(m_CurrentLayout);
                ShowLayout(layout);
                m_CurrentLayout = layout;

                m_ReturnMainLayout = data.ReturnMainLayout;
            });
        }

        void CreateAsciiCodeKey(KeyData data, ActionButton button)
        {
            KeyIcon(data, button);

            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.PreventDefault();

                SendKeyDownEvent(KeyCode.A);
                SendKeyDownEvent((char)data.AsciiCode);
                SendKeyUpEvent(KeyCode.A);
            });
        }

        void CreateFunctionalKey(KeyData data, ActionButton button)
        {
            KeyIcon(data, button);

            var keyCode = GetKeyCode(data.FunctionalKeyType);

            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.PreventDefault();

                m_DeferLongPress = button.schedule.Execute(() =>
                {
                    LongPressRepeatFunctionKey(button, keyCode);
                });
                m_DeferLongPress.ExecuteLater(k_FirstLongPressDelay);

                SendKeyDownEvent(keyCode, EventModifiers.FunctionKey);
                SendKeyUpEvent(keyCode);
            });

            button.RegisterCallback<PointerUpEvent>(evt =>
            {
                m_DeferLongPress?.Pause();
                m_DeferLongPress = null;
            });

            button.RegisterCallback<PointerOutEvent>(evt =>
            {
                m_DeferLongPress?.Pause();
                m_DeferLongPress = null;
            });
        }

        void LongPressRepeatFunctionKey(ActionButton button, KeyCode keyCode)
        {
            SendKeyDownEvent(keyCode, EventModifiers.FunctionKey);
            SendKeyUpEvent(keyCode);

            m_DeferLongPress = button.schedule.Execute(() =>
            {
                LongPressRepeatFunctionKey(button, keyCode);
            });
            m_DeferLongPress?.ExecuteLater(k_LongPressDelay);
        }

        void CreateEmptySpace(ActionButton keyButton)
        {
            keyButton.visible = false;
        }

        void SendKeyDownEvent(KeyCode keyCode, EventModifiers modifiers = EventModifiers.None)
        {
            if (!Utils.IsFocused(m_TextField))
            {
                ForceTextFieldFocus();
            }

            SendKeyEvent(EventType.KeyDown, keyCode, '\0', modifiers);
        }

        void SendKeyDownEvent(char character)
        {
            SendKeyEvent(EventType.KeyDown, KeyCode.None, character);
        }

        void SendKeyUpEvent(KeyCode keyCode)
        {
            SendKeyEvent(EventType.KeyUp, keyCode);
        }

        void SendKeyEvent(EventType type, KeyCode keyCode, char character = '\0', EventModifiers modifiers = EventModifiers.None)
        {
            if (m_TextField != null)
            {
                var evt = new Event
                {
                    type = type,
                    character = character,
                    keyCode = keyCode,
                    modifiers = modifiers
                };

                switch (type)
                {
                    case EventType.KeyDown:
                        using (KeyDownEvent keyDownEvent = KeyDownEvent.GetPooled(evt))
                        {
                            m_TextField.SendEvent(keyDownEvent);
                        }

                        break;
                    case EventType.KeyUp:
                        using (KeyUpEvent keyUpEvent = KeyUpEvent.GetPooled(evt))
                        {
                            m_TextField.SendEvent(keyUpEvent);
                        }

                        break;
                }
            }
        }

        void KeyIcon(KeyData data, ActionButton button)
        {
            if (data.UseText)
            {
                var label = new Text(data.Text);
                label.size = TextSize.XS;

                button.hierarchy.Add(label);
            }
            else
            {
                var icon = new Icon
                {
                    sprite = data.Icon,
                    size = IconSize.M
                };

                button.hierarchy.Add(icon);
            }
        }

        static void ShowLayout(VisualElement layout)
        {
            layout.style.display = DisplayStyle.Flex;
        }

        static void HideLayout(VisualElement layout)
        {
            layout.style.display = DisplayStyle.None;
        }

        static KeyCode GetKeyCode(FunctionalKeyType functionalKeyType)
        {
            switch (functionalKeyType)
            {
                case FunctionalKeyType.Backspace:
                    return KeyCode.Backspace;
                case FunctionalKeyType.Delete:
                    return KeyCode.Delete;
                case FunctionalKeyType.Escape:
                    return KeyCode.Escape;
                case FunctionalKeyType.Insert:
                    return KeyCode.Insert;
                case FunctionalKeyType.Home:
                    return KeyCode.Home;
                case FunctionalKeyType.End:
                    return KeyCode.End;
                case FunctionalKeyType.PageUp:
                    return KeyCode.PageUp;
                case FunctionalKeyType.PageDown:
                    return KeyCode.PageDown;
                case FunctionalKeyType.NumLock:
                    return KeyCode.Numlock;
                case FunctionalKeyType.ScrollLock:
                    return KeyCode.ScrollLock;
                case FunctionalKeyType.PauseBreak:
                    return KeyCode.Pause;
                case FunctionalKeyType.F1:
                    return KeyCode.F1;
                case FunctionalKeyType.F2:
                    return KeyCode.F2;
                case FunctionalKeyType.F3:
                    return KeyCode.F3;
                case FunctionalKeyType.F4:
                    return KeyCode.F4;
                case FunctionalKeyType.F5:
                    return KeyCode.F5;
                case FunctionalKeyType.F6:
                    return KeyCode.F6;
                case FunctionalKeyType.F7:
                    return KeyCode.F7;
                case FunctionalKeyType.F8:
                    return KeyCode.F8;
                case FunctionalKeyType.F9:
                    return KeyCode.F9;
                case FunctionalKeyType.F10:
                    return KeyCode.F10;
                case FunctionalKeyType.F11:
                    return KeyCode.F11;
                case FunctionalKeyType.F12:
                    return KeyCode.F12;
            }

            return KeyCode.None;
        }
    }
}

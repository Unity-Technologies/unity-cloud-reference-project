using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.ReferenceProject.CustomKeyboard
{
    public enum KeyType
    {
        Character,
        SwitchLayout,
        Ascii,
        Functional,
        EmptySpace,
        ExtendedCharacter
    }

    public enum FunctionalKeyType
    {
        Backspace,
        Delete,
        Escape,
        Insert,
        Home,
        End,
        PageUp,
        PageDown,
        NumLock,
        ScrollLock,
        PauseBreak,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12
    }

    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public struct KeyRow
    {
        public KeyData[] Keys;
    }

    [Serializable]
    [StructLayout(LayoutKind.Auto)]
    public class KeyData
    {
        public KeyType Type;
        public char Character;
        public Sprite Icon;
        public float Size;
        public KeyboardLayout Layout;
        public int AsciiCode;
        public FunctionalKeyType FunctionalKeyType;
        public bool UseText;
        public string Text;
        public bool ReturnMainLayout;
        public string ExtendedCharacters;
    }
}

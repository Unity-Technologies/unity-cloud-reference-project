using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.CustomKeyboard
{
    [CreateAssetMenu(fileName = "KeyboardLayout", menuName = "ReferenceProject/CustomKeyboard/Layout", order = 0)]
    public class KeyboardLayout : ScriptableObject
    {
        [SerializeField]
        KeyRow[] m_KeyRows;

        public IEnumerable<KeyRow> KeyRows => m_KeyRows;
    }
}

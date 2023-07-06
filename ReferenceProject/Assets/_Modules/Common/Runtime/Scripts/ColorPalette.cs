using System;
using UnityEngine;

namespace Unity.ReferenceProject.Common
{
    [CreateAssetMenu(fileName = nameof(ColorPalette), menuName = "ReferenceProject/" + nameof(ColorPalette))]
    public class ColorPalette : ScriptableObject
    {
        [SerializeField]
        Color[] m_Colors;

        public Color GetColor(int index)
        {
            if (index < 0)
            {
                index *= -1;
            }

            return m_Colors[index % GetColorCount()];
        }

        public int GetColorCount()
        {
            return m_Colors.Length;
        }
    }
}

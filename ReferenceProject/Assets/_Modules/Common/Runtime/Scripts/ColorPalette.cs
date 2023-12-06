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
            // m_Colors can be null in case if it has been created ScriptableObject instance at runtime
            if (m_Colors == null || m_Colors.Length == 0)
            {
                Debug.LogWarning($"{nameof(ColorPalette)} doesn't contain any color. Return color: {nameof(Color.grey)}");
                return Color.grey;
            }

            if (index < 0)
            {
                index *= -1;
            }

            return m_Colors[index % Count];
        }

        public int Count => m_Colors.Length;
    }
}

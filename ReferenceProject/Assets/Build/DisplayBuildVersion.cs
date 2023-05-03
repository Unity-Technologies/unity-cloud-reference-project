using System;
using UnityEngine;

namespace Unity.DTReferenceProject
{
    [ExecuteInEditMode]
    public class DisplayBuildVersion : MonoBehaviour
    {
        GUIStyle m_Style;

        void OnGUI()
        {
            m_Style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };

            var versionStr = $"ver. {Application.version}";

            var rect = new Rect(0.0f, Screen.height - 20.0f, Screen.width - 10.0f, 20.0f);

            var currentColor = GUI.color;

            GUI.color = Color.black;
            GUI.Label(rect, versionStr, m_Style);

            rect.x -= 1.0f;
            rect.y -= 1.0f;

            GUI.color = Color.white;
            GUI.Label(rect, versionStr, m_Style);

            GUI.color = currentColor;
        }
    }
}

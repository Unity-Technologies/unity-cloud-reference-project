using System;
using System.Text;
using Unity.Cloud.ReferenceProject.Utils.Git;
using UnityEngine;

namespace Unity.DTReferenceProject
{
    [ExecuteInEditMode]
    public class DisplayBuildVersion : MonoBehaviour
    {
        GUIStyle m_Style;
        string m_VersionStr;
        string m_GitVersion;
        string m_Version;
        StringBuilder m_Sb = new StringBuilder();

        void OnGUI()
        {
            m_Style ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
            if (Application.version != m_Version)
            {
                m_Version = Application.version;
                m_Sb.Append("ver. ");
                m_Sb.Append(m_Version);
                m_Sb.Append("\n");
            }
#if USE_GIT_INFO
            if (VersionControlInformation.Instance.CommitHash != m_GitVersion)
            {
                m_GitVersion = VersionControlInformation.Instance.CommitHash;
                m_Sb.Append("hash. ");
                m_Sb.Append(m_GitVersion);
            }
#endif
            if (m_Sb.Length != 0)
            {
                m_VersionStr = m_Sb.ToString();
                m_Sb = m_Sb.Clear();
            }

            var rect = new Rect(0.0f, Screen.height - 40.0f, Screen.width - 10.0f, 60.0f);

            var currentColor = GUI.color;

            GUI.color = Color.black;
            GUI.Label(rect, m_VersionStr, m_Style);

            rect.x -= 1.0f;
            rect.y -= 1.0f;

            GUI.color = Color.white;
            GUI.Label(rect, m_VersionStr, m_Style);

            GUI.color = currentColor;
        }
    }
}

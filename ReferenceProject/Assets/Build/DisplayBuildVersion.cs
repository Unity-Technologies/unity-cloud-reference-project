using System;
using System.Text;
using UnityEngine;

#if USE_GIT_INFO
using Unity.Cloud.ReferenceProject.Utils.Git;
#endif

namespace Unity.DTReferenceProject
{
    [ExecuteInEditMode]
    public class DisplayBuildVersion : MonoBehaviour
    {
        [SerializeField]
        TextAsset m_ReferenceProjectVersion;
        
        [SerializeField]
        int m_FontSize = 16;

        [SerializeField]
        int m_Padding = 3;

        GUIStyle m_Style;
        string m_VersionStr;

        void Awake()
        {
            m_VersionStr = GenerateVersion();
        }

        void OnGUI()
        {
#if UNITY_EDITOR
            m_VersionStr = GenerateVersion();
#endif
            DrawUI();
        }
        
        string GenerateVersion()
        {
            var sb = new StringBuilder();
            
            sb.Append("ver. ");
            sb.Append(Application.version);
            
            if (m_ReferenceProjectVersion != null)
            {
                sb.Append(" ");
                sb.Append(m_ReferenceProjectVersion.text.Trim());
            }

#if USE_GIT_INFO
            if (!string.IsNullOrEmpty(VersionControlInformation.Instance.CommitHash))
            {
                sb.Append(" hash. ");
                sb.Append(VersionControlInformation.Instance.CommitHash);
            }
#endif
            return sb.ToString();
        }

        void DrawUI(){

            var fontHeight = Mathf.Min(Mathf.FloorToInt(Screen.width * m_FontSize / 1000f), Mathf.FloorToInt(Screen.height * m_FontSize / 1000f));
            
            m_Style ??= new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerRight,
                fontSize = fontHeight
            };

            var h = fontHeight + m_Padding + 1f; // +1f for drop shadow
            var rect = new Rect(m_Padding, Screen.height - h, Screen.width - 2f * m_Padding, h);

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

using System;

namespace Unity.ReferenceProject.UIPanel
{
    public partial interface IMainUIPanel
    {
        string Theme { get; set; }
    }

    public partial class MainUIPanel
    {
        public string Theme
        {
            get => m_Panel != null ? m_Panel.theme : string.Empty;
            set
            {
                if (m_Panel != null)
                {
                    m_Panel.theme = value;
                }
            }
        }
    }
}

using System;

namespace Unity.ReferenceProject.UIPanel
{
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

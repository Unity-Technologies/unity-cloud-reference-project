using System;
using Unity.ReferenceProject.DataStores;
using UnityEngine;

namespace Unity.ReferenceProject.Identity
{
    public class UserData : IUserData
    {
        string m_Id = "undefined";
        string m_Name = "undefined";
        Color m_Color = Color.grey;

        public string Id
        {
            get => m_Id;
            internal set
            {
                if(value != m_Id)
                {
                    IdChanged?.Invoke(m_Name);
                }
                m_Id = value;
            }
        }

        public string Name
        {
            get => m_Name;
            internal set
            {
                if(value != m_Name)
                {
                    NameChanged?.Invoke(m_Name);
                }
                m_Name = value;
            }
        }

        public Color BadgeColor
        {
            get => m_Color;
            internal set
            {
                if(value != m_Color)
                {
                    ColorChanged?.Invoke(value);
                }
                m_Color = value;
            }
        }

        public event Action<string> IdChanged;
        public event Action<string> NameChanged;
        public event Action<Color> ColorChanged;

        public UserData()
        {

        }

        public UserData(string id, string name, Color color)
        {
            m_Id = id;
            m_Name = name;
            m_Color = color;
        }

        public void UpdateBadgeColor(Color color)
        {
            BadgeColor = color;
        }
    }
}

using System;
using Unity.ReferenceProject.MeasureTool;
using Unity.ReferenceProject.Settings;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class AppUnitSetting : MonoBehaviour
    {
        IAppUnit m_AppUnit;
        IGlobalSettings m_GlobalSettings;
        DropdownSetting m_DropdownSetting;
        MeasureFormat[]  m_MeasureFormats;
        string[] m_MeasureFormatsNames;
        
        [Inject]
        void Setup(IGlobalSettings globalSettings, IAppUnit appUnit)
        {
            m_GlobalSettings = globalSettings;
            m_AppUnit = appUnit;
        }
        
        void Awake()
        {
            m_MeasureFormats = m_AppUnit.GetMeasureFormat();
            m_MeasureFormatsNames = new string[m_MeasureFormats.Length];
            
            for( var i = 0; i< m_MeasureFormats.Length; i++)
            {
                m_MeasureFormatsNames[i] = "@MeasureTool:" + m_MeasureFormats[i];
            }
            
            m_DropdownSetting = new DropdownSetting("@MeasureTool:Unit", m_MeasureFormatsNames, SettingChanged, 
            SelectedValue);
        }
        
        void OnEnable()
        {
            m_GlobalSettings?.AddSetting(m_DropdownSetting,1);
        }
        
        void OnDisable()
        {
            m_GlobalSettings?.RemoveSetting(m_DropdownSetting);
        }
        
        void SettingChanged(int index)
        {
            m_AppUnit.SetSystemUnit(m_MeasureFormats[index]);
        }

        int SelectedValue()
        {
            var currentSystemUnit = m_AppUnit.GetSystemUnit();
            m_DropdownSetting.SetValue(Array.IndexOf(m_MeasureFormatsNames, "@MeasureTool:" + currentSystemUnit));
            return Array.IndexOf(m_MeasureFormatsNames, "@MeasureTool:" + currentSystemUnit);
        }
    }
}

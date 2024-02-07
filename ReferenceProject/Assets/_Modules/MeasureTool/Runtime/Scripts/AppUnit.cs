using System;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public enum MeasureFormat
    {
        Meters = 0,
        Centimeters = 1,
        Feet = 2,
        Inches = 3,
        FeetAndInches = 4
    }
    
    public interface IAppUnit
    {
        public event Action<MeasureFormat> SystemUnitChanged;
        MeasureFormat GetSystemUnit();
        MeasureFormat[] GetMeasureFormat();
        void SetSystemUnit(MeasureFormat measureFormat);
    }
    
    public class AppUnit : IAppUnit
    {
        MeasureFormat m_SystemMeasureFormat = (MeasureFormat)PlayerPrefs.GetInt("MeasureFormat");
        public event Action<MeasureFormat> SystemUnitChanged;
        
        public MeasureFormat[] GetMeasureFormat()
        {
            return (MeasureFormat[])Enum.GetValues(typeof(MeasureFormat));
        }

        public MeasureFormat GetSystemUnit()
        {
            return m_SystemMeasureFormat;
        }
        
        public void SetSystemUnit(MeasureFormat measureFormat)
        {
            m_SystemMeasureFormat = measureFormat;
            PlayerPrefs.SetInt("MeasureFormat", (int)m_SystemMeasureFormat);
            SystemUnitChanged?.Invoke(measureFormat);
        }
    }
}

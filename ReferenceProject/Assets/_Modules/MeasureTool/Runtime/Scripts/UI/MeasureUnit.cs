using System;
using System.Collections.Generic;
using Unity.Geospatial.Unit;

namespace Unity.ReferenceProject.MeasureTool
{
    public static class MeasureUnit
    {
        static readonly Dictionary<MeasureFormat, Length> k_UnitsLookUp = new ()
        {
            { MeasureFormat.Meters, Si.Meter },
            { MeasureFormat.Centimeters, Si.Centimeter },
            { MeasureFormat.Feet, Imperial.Feet },
            { MeasureFormat.Inches, Imperial.Inch },
            { MeasureFormat.FeetAndInches, Imperial.Feet }
        };
        
        public static string GetDistanceFormattedString(float distanceMeter, MeasureFormat desiredFormat)
        {
            var meterValue = Unit.From($"{distanceMeter} m");
            var selectedValue = meterValue.To(k_UnitsLookUp[desiredFormat]);

            var textValue = desiredFormat switch
            {
                MeasureFormat.Inches => FormatUnitString(selectedValue, null, true, 16),
                
                MeasureFormat.FeetAndInches => FormatUnitString(selectedValue, new List<IUnitDef> { Imperial.Feet, Imperial.Inch }, true, 16),
                
                _ => FormatUnitString(selectedValue)
            };

            return textValue;
        }
       
        static string FormatUnitString(Unit unit, List<IUnitDef> outputUnits = null, bool fractional = false, int precision = 2)
        {
            // If there isn't any output units configured, assume input unit.
            if (outputUnits == null || outputUnits.Count == 0)
            {
                return unit.ToString(
                    (fractional) ? new FractionalUnitFormatter(precision: precision) : new UnitFormatter(decimalPrecision: precision));
            }

            return unit.ToString(
                (fractional) ? new MultiFractionalUnitFormatter(outputUnits.ToArray(), precision: precision) : new MultiUnitFormatter(outputUnits.ToArray(), decimalPrecision: precision));
        }
    }
}
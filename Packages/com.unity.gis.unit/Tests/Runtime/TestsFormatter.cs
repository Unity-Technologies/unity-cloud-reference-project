using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Unity.Geospatial.Unit.Tests
{
    public class TestsFormatter
    {
        readonly IUnitDef[] m_ImperialTestScales = new IUnitDef[]
            { Imperial.Mile, Imperial.Yard, Imperial.Feet, Imperial.Inch };
        readonly IUnitDef[] m_MetricTestScales = new IUnitDef[]
            { Si.Kilometer, Si.Meter, Si.Centimeter, Si.Millimeter };

        public enum ScaleGroup
        {
            Imperial,
            Metric
        };

        [TestCase(.7, 16, false, "11/16\"")]
        [TestCase(.25, 16, false, "1/4\"")]
        [TestCase(.25, 2, false, "0\"")]
        [TestCase(.285, 64, false, "9/32\"")]
        [TestCase(-.285, 64, false, "-9/32\"")]
        public void FractionalUnitFormatter(double input, int precision, bool fullString, string expectedResult)
        {
            var unit = new Unit(input, Imperial.Inch);
            string result = unit.ToString(new FractionalUnitFormatter(fullString, false, precision));
            Assert.True(result == expectedResult, result);
        }

        [TestCase(.7, 3, false, "0.7\"")]
        [TestCase(.78542, 3, false, "0.785\"")]
        [TestCase(.788888, 2, false, "0.79\"")]
        [TestCase(-.788888, 2, false, "-0.79\"")]
        public void DecimalUnitFormatter(double input, int precision, bool fullString, string expectedResult)
        {
            var unit = new Unit(input, Imperial.Inch);
            string result = unit.ToString(new UnitFormatter(fullString, false, precision));
            Assert.True(result == expectedResult, result);
        }


        [TestCase(0.3333, 0, 3, "0.333 m")]
        [TestCase(0.3333, 0, 2, "0.33 m")]
        [TestCase(0.1, 3, 3, "0.100 m")]
        [TestCase(1.1111, 0, 0, "1 m")]
        [TestCase(1, 0, 5, "1 m")]
        [TestCase(1, 5, 5, "1.00000 m")]
        [TestCase(1, int.MaxValue, 5, "1.0000000000 m")]
        [TestCase(1.1, int.MaxValue, 5, "1.1000000000 m")]
        public void TestPrecision(double input, int minimumPrecision, int precision, string expectedResult)
        {
            var unit = new Unit(input, Si.Meter);
            string result = unit.ToString(new UnitFormatter(decimalPrecision: precision, minimumPrecision: minimumPrecision));
            Assert.True(result == expectedResult, result);
        }


        [TestCase(20050608, true, 32, true, true, ScaleGroup.Imperial, "12458 miles 1531 yards 1 foot 7 3/4 inches")]
        [TestCase(20050608, false, 3, true, true,ScaleGroup.Imperial, "12458 miles 1531 yards 1 foot 7.748 inches")]
        [TestCase(20050608, true, 32, true, false, ScaleGroup.Imperial, "12458 mi 1531 yd 1' 7 3/4\"")]
        [TestCase(20050608, false, 3, true, false, ScaleGroup.Imperial, "12458 mi 1531 yd 1' 7.748\"")]
        [TestCase(.01, true, 32, false, true, ScaleGroup.Imperial, "0 miles 0 yards 0 feet 13/32 inches")]
        [TestCase(.2, true, 64, true, false, ScaleGroup.Imperial, "7 7/8\"")]
        [TestCase(3, true, 2, true, false, ScaleGroup.Imperial, "3 yd 10\"")]
        [TestCase(3, true, 16, true, false, ScaleGroup.Imperial, "3 yd 10 1/8\"")]
        [TestCase(3, true, 4, true, false, ScaleGroup.Imperial, "3 yd 10\"")]
        [TestCase(2654083, false, 2,true, true, ScaleGroup.Metric, "2654 kilometers 83 meters")]
        [TestCase(3, false, 2,true, true, ScaleGroup.Metric, "3 meters")]
        [TestCase(0.00153, false, 2,true, true, ScaleGroup.Metric, "1.53 millimeters")]
        [TestCase(0.00153, false, 2,false, true, ScaleGroup.Metric, "0 kilometers 0 meters 0 centimeters 1.53 millimeters")]
        [TestCase(3, false, 2,false, true, ScaleGroup.Metric, "0 kilometers 3 meters 0 centimeters 0 millimeters")]
        [TestCase(1, false, 2,false, true, ScaleGroup.Metric, "0 kilometers 1 meter 0 centimeters 0 millimeters")]
        [TestCase(0.00153, true, 8,true, true, ScaleGroup.Metric, "1 1/2 millimeters")]
        [TestCase(846521, false, 2,true, false, ScaleGroup.Metric, "846 km 521 m")]
        [TestCase(7.832, false, 2, true, false, ScaleGroup.Metric, "7 m 83 cm 2 mm")]
        [TestCase(0.0082, false, 2, true, false, ScaleGroup.Metric, "8.2 mm")]
        [TestCase(1135.592, false, 2, false, false, ScaleGroup.Metric, "1 km 135 m 59 cm 2 mm")]
        [TestCase(25000, false, 2, false, false, ScaleGroup.Metric,  "25 km 0 m 0 cm 0 mm")]
        [TestCase(0.00871, false, 2,false, false, ScaleGroup.Metric, "0 km 0 m 0 cm 8.71 mm")]
        [TestCase(0.008715, false, 3, false, false, ScaleGroup.Metric, "0 km 0 m 0 cm 8.715 mm")]
        [TestCase(0.00871, false, 0, false, false, ScaleGroup.Metric, "0 km 0 m 0 cm 9 mm")]
        [TestCase(.7, true, 8, true, false, ScaleGroup.Imperial, "2' 3 1/2\"")]
        [TestCase(-0.7, true, 8, true, false, ScaleGroup.Imperial, "-2' 3 1/2\"")]
        [TestCase(-0.008715, false, 3, false, false, ScaleGroup.Metric, "-0 km 0 m 0 cm 8.715 mm")]
        public void Format(double initialValue,
            bool fractional, int precision,
            bool dropZeroScales, bool fullString, ScaleGroup scaleGroup, string expectedResult)
        {
            var scales = scaleGroup == ScaleGroup.Imperial ? m_ImperialTestScales : m_MetricTestScales;
            var initalUnit = new Unit(initialValue, Si.Meter);
            IUnitFormatter provider = (fractional) ?
                new MultiFractionalUnitFormatter(scales, dropZeroScales, fullString, false, precision) :
                new MultiUnitFormatter(scales, dropZeroScales, fullString, false, precision);

            var formatted = initalUnit.ToString(provider);
            Debug.Log($"Formatted: [{formatted}] \nExpected Result: [{expectedResult}]");
            Assert.True(formatted == expectedResult);
        }
    }
}

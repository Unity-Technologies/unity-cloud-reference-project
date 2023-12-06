
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Unity.Geospatial.Unit.Tests
{
    [TestFixture]
    public class TestsMethod
    {
        private const double k_Tolerance = 0.0000001;
        
        private static readonly object[][] GetEquals =
        {
            new object[] {Si.Centimeter, 5, Si.Centimeter, 5, true},
            new object[] {Si.Centimeter, 5, Si.Decimeter, 0.5, true},
            new object[] {Si.Millimeter, 50, Si.Decimeter, 0.5, true},
            new object[] {Si.Centimeter, 5, Si.Centimeter, 10, false},
            new object[] {Si.Centimeter2, 5, Si.Centimeter2, 10, false},
            new object[] {Si.Centimeter3, 5, Si.Centimeter3, 10, false},
            new object[] {Si.Centimeter, 5, Imperial.Feet, 10, false},
            new object[] {Si.Centimeter2, 5, Imperial.Feet2, 10, false},
            new object[] {Si.Centimeter3, 5, Imperial.Feet3, 10, false},
        };
        
        private static readonly object[][] GetString =
        {
            new object[] {1, Si.Centimeter, "1 cm", "1 centimeter"},
            new object[] {1, Si.Centimeter2, "1 cm²", "1 square centimeter"},
            new object[] {1, Si.Centimeter3, "1 cm³", "1 cubic centimeter"},
        };

        [TestCaseSource(nameof(GetEquals))]
        public void Equals(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, bool expected)
        {
            bool result = unitDefFirst.From(valueFirst).Equals(unitDefSecond.From(valueSecond), k_Tolerance);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Hash()
        {
            Dictionary<IUnit, double> dict = new Dictionary<IUnit, double>
            {
                {Misc.Null, 0},
                {Si.Radian.From(1), 1},
                {Si.Degree.From(1), 1},
                {Si.HourAngle.From(1), 1},
                {Si.ArcMinute.From(1), 1},
                {Si.ArcSecond.From(1), 1},
                {Si.MilliArcSecond.From(1), 1},
                {Si.MicroArcSecond.From(1), 1},
                {Si.Steradian.From(1), 1},
                {Si.Mole.From(1), 1},
                {Si.Meter.From(1), 1},
                {Si.Yottameter.From(1), 1},
                {Si.Zettameter.From(1), 1},
                {Si.Exameter.From(1), 1},
                {Si.Petameter.From(1), 1},
                {Si.Terameter.From(1), 1},
                {Si.Gigameter.From(1), 1},
                {Si.Megameter.From(1), 1},
                {Si.Kilometer.From(1), 1},
                {Si.Hectometer.From(1), 1},
                {Si.Decameter.From(1), 1},
                {Si.Decimeter.From(1), 1},
                {Si.Centimeter.From(1), 1},
                {Si.Millimeter.From(1), 1},
                {Si.Micrometer.From(1), 1},
                {Si.Nanometer.From(1), 1},
                {Si.Picometer.From(1), 1},
                {Si.Femtometer.From(1), 1},
                {Si.Attometer.From(1), 1},
                {Si.Zeptometer.From(1), 1},
                {Si.Yoctometer.From(1), 1},
                {Si.Angstrom.From(1), 1},
                {Si.Meter2.From(1), 1},
                {Si.Yottameter2.From(1), 1},
                {Si.Zettameter2.From(1), 1},
                {Si.Exameter2.From(1), 1},
                {Si.Petameter2.From(1), 1},
                {Si.Terameter2.From(1), 1},
                {Si.Gigameter2.From(1), 1},
                {Si.Megameter2.From(1), 1},
                {Si.Kilometer2.From(1), 1},
                {Si.Hectometer2.From(1), 1},
                {Si.Decameter2.From(1), 1},
                {Si.Decimeter2.From(1), 1},
                {Si.Centimeter2.From(1), 1},
                {Si.Millimeter2.From(1), 1},
                {Si.Micrometer2.From(1), 1},
                {Si.Nanometer2.From(1), 1},
                {Si.Picometer2.From(1), 1},
                {Si.Femtometer2.From(1), 1},
                {Si.Attometer2.From(1), 1},
                {Si.Zeptometer2.From(1), 1},
                {Si.Yoctometer2.From(1), 1},
                {Si.Liter.From(1), 1},
                {Si.Yottaliter.From(1), 1},
                {Si.Zettaliter.From(1), 1},
                {Si.Exaliter.From(1), 1},
                {Si.Petaliter.From(1), 1},
                {Si.Teraliter.From(1), 1},
                {Si.Gigaliter.From(1), 1},
                {Si.Megaliter.From(1), 1},
                {Si.Kiloliter.From(1), 1},
                {Si.Hectoliter.From(1), 1},
                {Si.Decaliter.From(1), 1},
                {Si.Deciliter.From(1), 1},
                {Si.Centiliter.From(1), 1},
                {Si.Milliliter.From(1), 1},
                {Si.Microliter.From(1), 1},
                {Si.Nanoliter.From(1), 1},
                {Si.Picoliter.From(1), 1},
                {Si.Femtoliter.From(1), 1},
                {Si.Attoliter.From(1), 1},
                {Si.Zeptoliter.From(1), 1},
                {Si.Yoctoliter.From(1), 1},
                {Si.Gram.From(1), 1},
                {Si.Yottagram.From(1), 1},
                {Si.Zettagram.From(1), 1},
                {Si.Exagram.From(1), 1},
                {Si.Petagram.From(1), 1},
                {Si.Teragram.From(1), 1},
                {Si.Gigagram.From(1), 1},
                {Si.Megagram.From(1), 1},
                {Si.Kilogram.From(1), 1},
                {Si.Hectogram.From(1), 1},
                {Si.Decagram.From(1), 1},
                {Si.Decigram.From(1), 1},
                {Si.Centigram.From(1), 1},
                {Si.Milligram.From(1), 1},
                {Si.Microgram.From(1), 1},
                {Si.Nanogram.From(1), 1},
                {Si.Picogram.From(1), 1},
                {Si.Femtogram.From(1), 1},
                {Si.Attogram.From(1), 1},
                {Si.Zeptogram.From(1), 1},
                {Si.Yoctogram.From(1), 1},
                {Si.Celsius.From(1), 1},
                {Si.Kelvin.From(1), 1},
                {Si.Second.From(1), 1},
                {Si.PlanckTime.From(1), 1},
                {Si.Yottasecond.From(1), 1},
                {Si.Zettasecond.From(1), 1},
                {Si.Exasecond.From(1), 1},
                {Si.Petasecond.From(1), 1},
                {Si.Terasecond.From(1), 1},
                {Si.Gigasecond.From(1), 1},
                {Si.Megasecond.From(1), 1},
                {Si.Kilosecond.From(1), 1},
                {Si.Hectosecond.From(1), 1},
                {Si.Decasecond.From(1), 1},
                {Si.Decisecond.From(1), 1},
                {Si.Centisecond.From(1), 1},
                {Si.Millisecond.From(1), 1},
                {Si.Microsecond.From(1), 1},
                {Si.Nanosecond.From(1), 1},
                {Si.Picosecond.From(1), 1},
                {Si.Femtosecond.From(1), 1},
                {Si.Attosecond.From(1), 1},
                {Si.Zeptosecond.From(1), 1},
                {Si.Yoctosecond.From(1), 1},
                {Si.Minute.From(1), 1},
                {Si.Hour.From(1), 1},
                {Si.Day.From(1), 1},
                {Si.SiderealDay.From(1), 1},
                {Si.Week.From(1), 1},
                {Si.Fortnight.From(1), 1},
                {Si.Month.From(1), 1},
                {Si.Year.From(1), 1},
                {Si.SiderealYear.From(1), 1},
                {Si.Decade.From(1), 1},
                {Si.Century.From(1), 1},
                {Si.Millennium.From(1), 1},
                {Si.Megayear.From(1), 1},
                {Si.Billionyear.From(1), 1},
                {Si.Hertz.From(1), 1},
                {Si.Becquerel.From(1), 1},
                {Si.Becquerel.From(1.1), 5},
            };
            
            Assert.AreEqual(dict[Si.Becquerel.From(1.1)], 5);
        }
        
        [Test]
        public void Sorting()
        {
            IUnit[] array1 = {Si.Meter.From(1.1), Si.Centimeter.From(1.1), Imperial.Yard.From(1.1), Si.Kilometer.From(1.1), Imperial.Inch.From(1.1)};

            Array.Sort(array1);
            
            Assert.AreEqual(Si.Centimeter, array1[0].UnitDef);
            Assert.AreEqual(Imperial.Inch, array1[1].UnitDef);
            Assert.AreEqual(Imperial.Yard, array1[2].UnitDef);
            Assert.AreEqual(Si.Meter, array1[3].UnitDef);
            Assert.AreEqual(Si.Kilometer, array1[4].UnitDef);
        }

        [TestCaseSource(nameof(GetString))]
        public void StringConversion(double value, IUnitDef unitDef, string expectedString, string expectedFullString)
        {
            Unit unit = unitDef.From(value);
            
            Assert.AreEqual(expectedString, unit.ToString());
            Assert.AreEqual(expectedFullString, unit.ToFullString());
        }

        [Test]
        public void NullToString()
        {
            Assert.AreEqual((string)Misc.Null, "Unit null");
        }
    }
}

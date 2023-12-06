
using System;
using System.Linq;
using NUnit.Framework;

namespace Unity.Geospatial.Unit.Tests
{
    [TestFixture]
    public class TestsConstructor
    {
        private const double k_Tolerance = 1E-12;

        private sealed class UnitDefTest : UnitDef<UnitDefTest>
        {
            internal static readonly UnitDefTest DefTest = new("'''Test'''", "'''t'''");

            public UnitDefTest(string name, string symbol, params string[] alternativeNaming) : 
            base(new UnitNaming(name, symbol, alternativeNaming), 1, 0, true) { }
        }
        
        private static readonly object[][] GetConstructors =
        {
            new object[] {5.0, 1, Si.Centimeter},
            new object[] {32.5, 2, Si.Centimeter2},
            new object[] {-64.44, 3, Si.Centimeter3},
            new object[] {1.98, 1, Si.Becquerel},
            new object[] {0.85, 2, Si.Becquerel},
            new object[] {0.23, 3, Si.Becquerel},
        };
        
        private static readonly object[][] GetStrings =
        {
            new object[] {"5¢", 5, Money.UsdPenny, 1, null},
            new object[] {"5.¢", 5, Money.UsdPenny, 1, null},
            new object[] {".05$", 0.05, Money.Usd, 1, null},
            new object[] {"5$", 5.0, Money.Usd, 1, null},
            new object[] {"5 $", 5.0, Money.Usd, 1, null},
            new object[] {"5USD", 5.0, Money.Usd, 1, null},
            new object[] {"- 5 cm", -5.0, Si.Centimeter, 1, null},
            new object[] {"5 mm", 5.0, Si.Millimeter, 1, null},
            new object[] {"-_5_cm", -5.0, Si.Centimeter, 1, null},
            new object[] {"5 cm", 5.0, Si.Centimeter, 1, null},
            new object[] {"5cm", 5.0, Si.Centimeter, 1, null},
            new object[] {"5cm2", 5.0, Si.Centimeter2, 2, null},
            new object[] {"5cm3", 5.0, Si.Centimeter3, 3, null},
            new object[] {"5 000cm", 5000.0, Si.Centimeter, 1, null},
            new object[] {"-5Centimeter", -5.0, Si.Centimeter, 1, null},
            new object[] {"-5 000 Centimeter", -5000.0, Si.Centimeter, 1, null},
            new object[] {"5.0 g", 5.0, Si.Gram, 1, null},
            new object[] {"5 000.000 000 gram", 5000.0, Si.Gram, 1, null},
            new object[] {"-5.0 grammes", -5.0, Si.Gram, 1, null},
            new object[] {"-5 000.000 000 grams", -5000.0, Si.Gram, 1, null},
            new object[] {"5E2  ft", 5E2, Imperial.Feet, 1, null},
            new object[] {"5'", 5, Imperial.Feet, 1, typeof(Length)},
            new object[] {"5E-2'", 5E-2, Imperial.Feet, 1, typeof(Area)},
            new object[] {"5E-2'²", 5E-2, Imperial.Feet2, 2, typeof(Area)},
            new object[] {"5E-2' square", 5E-2, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"5E-2 sq feets", 5E-2, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"5E-2 sq feets", 5E-2, Imperial.Feet2, 2, typeof(Area)},
            new object[] {"5E-2 sq feets", 5E-2, Imperial.Feet2, 2, typeof(Volume)},
            new object[] {"5E-2 cubic feets", 5E-2, Imperial.Feet3, 3, typeof(Area)},
            new object[] {"5E-2 cubic feets", 5E-2, Imperial.Feet3, 3, typeof(Length)},
            new object[] {"5E-2 cubic feets", 5E-2, Imperial.Feet3, 3, typeof(Volume)},
            new object[] {"5E-2' cb", 5E-2, Imperial.Feet3, 3, typeof(Length)},
            new object[] {"5E-2'³", 5E-2, Imperial.Feet3, 3, typeof(Volume)},
            new object[] {"5E-2 '''t'''⁴", 5E-2, UnitDefTest.DefTest, 4, typeof(UnitDefTest)},
            new object[] {"5E-2'''t'''⁵", 5E-2, UnitDefTest.DefTest, 5, typeof(UnitDefTest)},
            new object[] {"5E-2 '''t'''⁶", 5E-2, UnitDefTest.DefTest, 6, typeof(UnitDefTest)},
            new object[] {"5E-2  '''t'''⁷", 5E-2, UnitDefTest.DefTest, 7, typeof(UnitDefTest)},
            new object[] {"5E-2 '''t'''⁸", 5E-2, UnitDefTest.DefTest, 8, typeof(UnitDefTest)},
            new object[] {"5E-2'''t'''⁹", 5E-2, UnitDefTest.DefTest, 9, typeof(UnitDefTest)},
            new object[] {"5E-2  '''t'''¹¹", 5E-2, UnitDefTest.DefTest, 11, typeof(UnitDefTest)},
            new object[] {"5E-2_'''t'''²⁴", 5E-2, UnitDefTest.DefTest, 24, typeof(UnitDefTest)},
            new object[] {"5E-2ft", 5E-2, Imperial.Feet, 1, null},
            new object[] {"5.2E2 ft", 5.2E2, Imperial.Feet, 1, null},
            new object[] {"5.2E-2feet", 5.2E-2, Imperial.Feet, 1, null},
            new object[] {"5.2E-2Feet", 5.2E-2, Imperial.Feet, 1, null},
            new object[] {"5.2E-2fEET", 5.2E-2, Imperial.Feet, 1, null},
            new object[] {"-5E2feets", -5E2, Imperial.Feet, 1, null},
            new object[] {"-5E-2 feet", -5E-2, Imperial.Feet, 1, null},
            new object[] {"-5.2E2 feets", -5.2E2, Imperial.Feet, 1, null},
            new object[] {"-5.2E-2 ft²", -5.2E-2, Imperial.Feet2, 2, null},
            new object[] {"-5FOOT²", -5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"5 000E2 feet²", 5000E2, Imperial.Feet2, 2, null},
            new object[] {"5 000E-2 feets²", 5000E-2, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"5 000.2E2feet²", 5000.2E2, Imperial.Feet2, 2, null},
            new object[] {"5 000.2E-2ft²", 5000.2E-2, Imperial.Feet2, 2, null},
            new object[] {"-5 000E2'²", -5000E2, Imperial.Feet2, 2, typeof(Area)},
            new object[] {"-5 000E2 '²", -5000E2, Imperial.Feet2, 2, typeof(Area)},
            new object[] {"-5 000E-2ft²", -5000E-2, Imperial.Feet2, 2, null},
            new object[] {"-5 000.2E2 ft", -5000.2E2, Imperial.Feet, 1, null},
            new object[] {"-5 000.2E-2ft", -5000.2E-2, Imperial.Feet, 1, null},
            new object[] {"5,000E2 ft", 5000E2, Imperial.Feet, 1, null},
            new object[] {"5,000E-2 ft", 5000E-2, Imperial.Feet, 1, null},
            new object[] {"5,000.2E2 ft", 5000.2E2, Imperial.Feet, 1, null},
            new object[] {"5,000.2E-2 ft", 5000.2E-2, Imperial.Feet, 1, null},
            new object[] {"5 stry", 5, Storey.Abstract, 1, null},
            new object[] {"1/4\"", 1.0/4.0, Imperial.Inch, 1, typeof(Length)},
            new object[] {"2 1/4\"", 2.0 + (1.0/4.0), Imperial.Inch, 1, typeof(Length)},
            new object[] {"2-1/4\"", 2.0 + (1.0/4.0), Imperial.Inch, 1, typeof(Length)},
            new object[] {"2-<sup>1</sup>/<sub>4</sub>\"", 2.0 + (1.0/4.0), Imperial.Inch, 1, typeof(Length)},
        };

        private static readonly object[][] GetMultiStrings =
        {
            new object[] {"5arcm30arcs", 5.5, Si.ArcMinute, 1, typeof(Angle)},
            new object[] {"5'30\"", 5.5, Si.ArcMinute, 1, typeof(Angle)},
            new object[] {"5'6\"", 5.5, Imperial.Feet, 1, typeof(Length)},
            new object[] {"-5 000E2'6\"", -5000E2 - 0.5, Imperial.Feet, 1, typeof(Length)},
            new object[] {"-5'6\"²", -5.5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"-5 feetss 6\"²", -5.5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"-5 ' 6\"²", -5.5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"-5 ' 6 inchess²", -5.5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"-5FOOT 6 inch²", -5.5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"-5ft 6 inch²", -5.5, Imperial.Feet2, 2, typeof(Length)},
            new object[] {"5.2'6\"²", 5.7, Imperial.Feet2, 2, typeof(Area)},
            new object[] {"7' 4 3/4\"", (7.0 + ((4.0 + (3.0/4.0)) / 12.0)), Imperial.Feet, 1, typeof(Length)},
        };

        private static Unit Construct(double value, int power, IUnitDef unitDef) =>
            unitDef.PowerMin == unitDef.PowerMax
                ? new Unit(value, unitDef)
                : new Unit(value, unitDef, (byte) power);

        [TestCaseSource(nameof(GetConstructors))]
        public void Constructors(double value, int power, IUnitDef unitDef)
        {
            Unit unit = Construct(value, power, unitDef);

            Assert.AreEqual(unit.UnitDef, unitDef);
            Assert.AreEqual(unit.Value, value, k_Tolerance);
            Assert.AreEqual(unit.Power, power);
        }
        
        [TestCaseSource(nameof(GetConstructors))]
        public void Duplicate(double value, int power, IUnitDef unitDef)
        {
            Unit unit = Construct(value, power, unitDef);

            Unit duplicate = new Unit(unit);
            
            Assert.AreEqual(unit, duplicate);
        }
        
        [TestCaseSource(nameof(GetConstructors))]
        public void Explicit(double value, int power, IUnitDef unitDef)
        {
            Unit unit = Construct(value, power, unitDef);
            
            Assert.AreEqual((double)unit, value, k_Tolerance);
            Assert.AreEqual((float)unit, (float)value, k_Tolerance);
            Assert.AreEqual((int)unit, (int)value);
            Assert.AreEqual((string)unit, unit.ToString());
        }

        [TestCaseSource(nameof(GetStrings))]
        public void FromStringDouble(string text, double expected, IUnitDef unitDef, int power = 0, Type parentType = null)
        {
            Unit unit = unitDef.From(text);
            
            Assert.AreEqual(expected, unit.Value, k_Tolerance);
        }

        [TestCaseSource(nameof(GetStrings))]
        [TestCaseSource(nameof(GetMultiStrings))]
        public void FromString(string text, double expected, IUnitDef unitDef, int power, Type parentType = null)
        {
            Unit unit = parentType is null
                ? Unit.From(text) 
                : Unit.From(text, parentType);
            
            Assert.AreSame(unitDef, unit.UnitDef);
            Assert.AreEqual(expected, unit.Value, k_Tolerance);
            Assert.AreEqual(power, unit.Power);
        }
        
        [Test]
        public void FromStringEnumerable()
        {
            string[] data = {"5cm", "6m2", "5feet 6 inch²"};
            Unit[] expected = {Si.Centimeter.From(5), Si.Meter2.From(6), Imperial.Feet2.From(5.5)};

            Unit[] result = Unit.From(data).ToArray();
            
            for (int index = 0; index < data.Length; index++)
            {
                Unit exp = expected[index];
                Unit rst = result[index];
                
                Assert.AreSame(exp.UnitDef, rst.UnitDef);
                Assert.AreEqual(exp.Value, rst.Value, k_Tolerance);
                Assert.AreEqual(exp.Power, rst.Power);
            }

            data = new[] {"5cm", "6m2", "5'6\"²"};
            result = Unit.From(data, typeof(Length), typeof(Area)).ToArray();
            
            for (int index = 0; index < data.Length; index++)
            {
                Unit exp = expected[index];
                Unit rst = result[index];
                
                Assert.AreSame(exp.UnitDef, rst.UnitDef);
                Assert.AreEqual(exp.Value, rst.Value, k_Tolerance);
                Assert.AreEqual(exp.Power, rst.Power);
            }
        }

        [TestCaseSource(nameof(GetStrings))]
        public void FromStringSingleUnit(string text, double expected, IUnitDef unitDef, int power, Type parentType = null)
        {
            Unit unit = parentType is null
                ? Unit.FromStringToSingleUnit(text) 
                : Unit.FromStringToSingleUnit(text, parentType);
            
            Assert.AreSame(unitDef, unit.UnitDef);
            Assert.AreEqual(expected, unit.Value, k_Tolerance);
            Assert.AreEqual(power, unit.Power);
        }

        [Test]
        public void Null()
        {
            Unit @null = new Unit();
            Assert.IsTrue(@null.IsNull);
            
            Unit value = new Unit(5, Si.Centimeter);
            Assert.IsFalse(value.IsNull);
        }
    }
}

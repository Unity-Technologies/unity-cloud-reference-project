
using System;
using NUnit.Framework;

namespace Unity.Geospatial.Unit.Tests
{
    [TestFixture]
    public class TestsOperator
    {
        private const double k_Tolerance = 0.000_000_1;

        private static readonly object[][] GetOpValues =
        {
            new object[] {Si.Centimeter, 5, Si.Centimeter, 10, 10},
            new object[] {Si.Centimeter, 5, Si.Decimeter, 0.5, 5},
            new object[] {Si.Millimeter, 50, Si.Decimeter, 0.5, 50},
            new object[] {Si.Centimeter2, 5, Si.Centimeter2, 10, 10},
            new object[] {Si.Centimeter3, 5, Si.Centimeter3, 10, 10},
            new object[] {Si.Centimeter, 5, Imperial.Feet, 10, 304.8},
            new object[] {Si.Centimeter2, 5, Imperial.Feet2, 9.999_995_69, 9290.299_995_879},
            new object[] {Si.Centimeter3, 5, Imperial.Feet3, 9.999_983_55, 283_168.000_107_9},
        };
        
        private static readonly object[][] GetMultValues =
        {
            new object[] {Si.Centimeter, 5, Si.Centimeter, 10, 10, Si.Centimeter2},
            new object[] {Si.Centimeter2, 5, Si.Centimeter, 10, 10, Si.Centimeter3},
            new object[] {Si.Centimeter, 5, Si.Centimeter2, 10, 10, Si.Centimeter3},
            new object[] {Si.Centimeter, 5, Imperial.Feet, 10, 304.8, Si.Centimeter2},
            new object[] {Si.Centimeter2, 5, Imperial.Feet, 10, 304.8, Si.Centimeter3},
            new object[] {Si.Centimeter, 5, Imperial.Feet2, 9.999_995_69, 9290.299_995_879, Si.Centimeter3},
        };
        
        private static readonly object[][] GetDivValues =
        {
            new object[] {Si.Centimeter2, 5, Si.Centimeter, 10, 10, Si.Centimeter},
            new object[] {Si.Centimeter3, 5, Si.Centimeter2, 10, 10, Si.Centimeter},
            new object[] {Si.Centimeter3, 5, Si.Centimeter, 10, 10, Si.Centimeter2},
            new object[] {Si.Centimeter2, 5, Imperial.Feet, 10, 304.8, Si.Centimeter},
            new object[] {Si.Centimeter3, 5, Imperial.Feet, 10, 304.8, Si.Centimeter2},
            new object[] {Si.Centimeter3, 5, Imperial.Feet2, 9.999_995_69, 9290.299_995_879, Si.Centimeter},
        };
        
        private static void OpUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted, Func<double, double, double> funcDouble, Func<Unit, Unit, Unit> funcUnit)
        {
            Unit first = new Unit(valueFirst, unitDefFirst);
            Unit second = new Unit(valueSecond, unitDefSecond);

            Unit result = funcUnit(first, second);
            
            Assert.AreSame(unitDefFirst, result.UnitDef);
            Assert.AreEqual(funcDouble(valueFirst, secondConverted), result.Value, k_Tolerance);
        }
        
        private static void OpUnits(IUnitDef unitDefFirst, double valueFirst, double valueSecond, Func<double, double, double> funcDouble, Func<Unit, double, Unit> funcUnit)
        {
            Unit first = new Unit(valueFirst, unitDefFirst);

            Unit result = funcUnit(first, valueSecond);
            
            Assert.AreSame(unitDefFirst, result.UnitDef);
            Assert.AreEqual(funcDouble(valueFirst, valueSecond), result.Value, k_Tolerance);
        }
        
        private static void OpUnits(double valueFirst, IUnitDef unitDefSecond, double valueSecond, Func<double, double, double> funcDouble, Func<double, Unit, Unit> funcUnit)
        {
            Unit second = new Unit(valueSecond, unitDefSecond);

            Unit result = funcUnit(valueFirst, second);
            
            Assert.AreSame(unitDefSecond, result.UnitDef);
            Assert.AreEqual(funcDouble(valueFirst, valueSecond), result.Value, k_Tolerance);
        }
        
        private static void OpUnits<T>(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted, Func<double, double, T> funcDouble, Func<Unit, Unit, T> funcUnit)
        {
            Unit first = new Unit(valueFirst, unitDefFirst);
            Unit second = new Unit(valueSecond, unitDefSecond);

            T result = funcUnit(first, second);
            
            Assert.AreEqual(funcDouble(valueFirst, secondConverted), result);
        }
        
        private static void OpUnits<T>(IUnitDef unitDefFirst, double valueFirst, double valueSecond, Func<double, double, T> funcDouble, Func<Unit, double, T> funcUnit)
        {
            Unit first = new Unit(valueFirst, unitDefFirst);

            T result = funcUnit(first, valueSecond);
            
            Assert.AreEqual(funcDouble(valueFirst, valueSecond), result);
        }
        
        private static void OpUnits<T>(double valueFirst, IUnitDef unitDefSecond, double valueSecond, Func<double, double, T> funcDouble, Func<double, Unit, T> funcUnit)
        {
            Unit second = new Unit(valueSecond, unitDefSecond);

            T result = funcUnit(valueFirst, second);
            
            Assert.AreEqual(funcDouble(valueFirst, valueSecond), result);
        }

        [TestCaseSource(nameof(GetOpValues))]
        public void AddUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x + y, (x, y) => x + y);

        [TestCaseSource(nameof(GetOpValues))]
        public void AddDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x + y, (x, y) => x + y);

        [TestCaseSource(nameof(GetOpValues))]
        public void AddDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x + y, (x, y) => x + y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void SubUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x - y, (x, y) => x - y);

        [TestCaseSource(nameof(GetOpValues))]
        public void SubDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x - y, (x, y) => x - y);

        [TestCaseSource(nameof(GetOpValues))]
        public void SubDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x - y, (x, y) => x - y);
        
        [TestCaseSource(nameof(GetMultValues))]
        public void MultUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted, IUnitDef unitDefResult)
        {
            Unit first = new Unit(valueFirst, unitDefFirst);
            Unit second = new Unit(valueSecond, unitDefSecond);

            Unit result = first * second;
            
            Assert.AreSame(unitDefResult, result.UnitDef);
            Assert.AreEqual(valueFirst * secondConverted, result.Value, k_Tolerance);
        }

        [TestCaseSource(nameof(GetOpValues))]
        public void MultDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x * y, (x, y) => x * y);

        [TestCaseSource(nameof(GetOpValues))]
        public void MultDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x * y, (x, y) => x * y);
        
        [TestCaseSource(nameof(GetDivValues))]
        public void DivUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted, IUnitDef unitDefResult)
        {
            Unit first = new Unit(valueFirst, unitDefFirst);
            Unit second = new Unit(valueSecond, unitDefSecond);

            Unit result = first / second;
            
            Assert.AreSame(unitDefResult, result.UnitDef);
            Assert.AreEqual(valueFirst / secondConverted, result.Value, k_Tolerance);
        }

        [TestCaseSource(nameof(GetOpValues))]
        public void DivDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x / y, (x, y) => x / y);

        [TestCaseSource(nameof(GetOpValues))]
        public void DivDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x / y, (x, y) => x / y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void ModuloUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x % y, (x, y) => x % y);

        [TestCaseSource(nameof(GetOpValues))]
        public void ModuloDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x % y, (x, y) => x % y);

        [TestCaseSource(nameof(GetOpValues))]
        public void ModuloDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x % y, (x, y) => x % y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void GreaterUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x > y, (x, y) => x > y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void GreaterDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x > y, (x, y) => x > y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void GreaterDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x > y, (x, y) => x > y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void GreaterEqualsUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x >= y, (x, y) => x >= y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void GreaterEqualsDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x >= y, (x, y) => x >= y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void GreaterEqualsDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x >= y, (x, y) => x >= y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void LessUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x < y, (x, y) => x < y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void LessDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x < y, (x, y) => x < y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void LessDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x < y, (x, y) => x < y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void LessEqualsUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x <= y, (x, y) => x <= y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void LessEqualsDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x <= y, (x, y) => x <= y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void LessEqualsDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x <= y, (x, y) => x <= y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void EqualsUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x == y, (x, y) => x == y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void EqualsDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x == y, (x, y) => x == y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void EqualsDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x == y, (x, y) => x == y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void NotEqualsUnits(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, unitDefSecond, valueSecond, secondConverted, (x, y) => x != y, (x, y) => x != y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void NotEqualsDouble1(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(valueFirst, unitDefSecond, valueSecond, (x, y) => x != y, (x, y) => x != y);
        
        [TestCaseSource(nameof(GetOpValues))]
        public void NotEqualsDouble2(IUnitDef unitDefFirst, double valueFirst, IUnitDef unitDefSecond, double valueSecond, double secondConverted) =>
            OpUnits(unitDefFirst, valueFirst, valueSecond, (x, y) => x != y, (x, y) => x != y);
    }
}

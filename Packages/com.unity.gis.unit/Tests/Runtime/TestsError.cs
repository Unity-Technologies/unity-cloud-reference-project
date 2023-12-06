
using System;
using NUnit.Framework;

namespace Unity.Geospatial.Unit.Tests
{
    [TestFixture]
    public class TestsError
    {
        [Test]
        public void ConvertAbstract()
        {
            Assert.Throws(
                typeof(ConvertAbstractException),
                () => { Unit _ = Storey.Abstract.From(1).To(Si.Meter); });
            
            Assert.Throws(
                typeof(ConvertAbstractException),
                () => { Unit _ = Storey.Abstract.From(1).To(Storey.Abstract); });
        }
        
        [Test]
        public void FromStringDouble()
        {
            Assert.Throws(
                typeof(DoublePatternException),
                () => { Si.Centimeter.From("hello world", 1); });
        }
        
        [Test]
        public void FromString()
        {
            Assert.Throws(
                typeof(UnitPatternException),
                () => { _ = (Unit)"hello world"; });
        }
        
        [Test]
        public void FromStringSingleUnit()
        {
            Assert.Throws(
                typeof(UnitPatternException),
                () => { Unit.FromStringToSingleUnit("hello world"); });
        }
        
        [Test]
        public void MultiExp()
        {
            Assert.Throws(
                typeof(ExponentPatternException),
                () => { _ = (Unit)"2m²3Centimeter³"; });
        }
        
        [Test]
        public void NullConvert()
        {
            Assert.Throws(
                typeof(NullUnitDefException),
                () => { Misc.Null.To(Si.Centimeter); });
        }
        
        [Test]
        public void NullConvertToBase()
        {
            Assert.Throws(
                typeof(NullUnitDefException),
                () => { Misc.Null.ToBaseUnit(); });
        }
        
        [Test]
        public void OpCorrespondingPower()
        {
            Assert.Throws(
                typeof(PowerOutOfRangeException),
                () => { _ = Si.Centimeter.From(1) / Si.Centimeter2.From(1); });
        }
        
        [Test]
        public void OpDifferentPower()
        {
            Assert.Throws(
                typeof(DifferentPowersException),
                () => { _ = Si.Centimeter.From(1) + Si.Centimeter2.From(1); });
        }
        
        [Test]
        public void OpDifferentTypes()
        {
            Assert.Throws(
                typeof(DifferentTypesException),
                () => { _ = Si.Centimeter.From(1) + Si.Celsius.From(1); });
        }
        
        [Test]
        public void OpIncompatibleTypes()
        {
            Assert.Throws(
                typeof(IncompatibleTypesException),
                () => { _ = Si.Centimeter.From(1) * Si.Gram.From(1); });
        }

        [Test]
        public void ToTime()
        {
            Assert.Throws(
                typeof(WrongUnitDefTypeException),
                () => { Si.Meter.From(1).ToTimeSpan(); });
        }

        [Test]
        public void TwoBaseUnit()
        {
            Assert.Throws(
                typeof(MultiBaseUnitException),
                () => { new Length(new UnitNaming("", "")).RegisterAsBaseUnit(); });
        }

        [Test]
        public void UnitMinPower()
        {
            Assert.Throws(
                typeof(PowerOutOfRangeException),
                () => { _ = new Unit(1, Si.Becquerel, 0); });
        }

        [Test]
        public void UnitMaxPower()
        {
            Assert.Throws(
                typeof(PowerOutOfRangeException),
                () => { _ = new Unit(1, Si.Becquerel, 250); });
        }
    }
}

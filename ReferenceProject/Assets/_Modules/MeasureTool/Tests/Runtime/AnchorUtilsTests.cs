using System.Globalization;
using NUnit.Framework;
using Unity.ReferenceProject.MeasureTool.Utils;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool.Tests
{
    public class AnchorUtilsTests
    {
        private IAnchor anchor1;
        private IAnchor anchor2;

        [SetUp]
        public void SetUp()
        {
            anchor1 = new PointAnchor(new Vector3(1f, 2f, 3f), Vector3.up);
            anchor2 = new PointAnchor(new Vector3(4f, 5f, 6f), Vector3.up);
        }

        [Test]
        public void GetDistanceBetweenAnchorsMeters_ReturnsCorrectDistance()
        {
            float expectedDistance = Vector3.Distance(anchor1.Position, anchor2.Position);
            float actualDistance = AnchorDistance.GetDistanceBetweenAnchorsMeters(anchor1, anchor2);
            Assert.AreEqual(expectedDistance, actualDistance, 0.001f);
        }

        [Test]
        public void GetDistanceBetweenAnchorsString_ReturnsDistanceString()
        {
            float distance = AnchorDistance.GetDistanceBetweenAnchorsMeters(anchor1, anchor2);
            string expectedString = distance.ToString(CultureInfo.InvariantCulture) + "m";
            string actualString = AnchorDistance.GetDistanceBetweenAnchorsString(anchor1, anchor2);
            Assert.AreEqual(expectedString, actualString);
        }
    }
}
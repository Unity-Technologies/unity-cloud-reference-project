using NUnit.Framework;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool.Tests
{
    public class PointAnchorTests
    {
        [Test]
        public void TestConstructor()
        {
            // Arrange
            var position = new Vector3(1, 2, 3);
            var normal = new Vector3(4, 5, 6);

            // Act
            var anchor = new PointAnchor(position, normal);

            // Assert
            Assert.AreEqual(position, anchor.Position);
            Assert.AreEqual(normal, anchor.Normal);
        }

        [Test]
        public void TestEquals_NullObject_ReturnsFalse()
        {
            // Arrange
            var anchor = new PointAnchor(new Vector3(1, 2, 3), new Vector3(4, 5, 6));

            // Act
            var result = anchor.Equals(null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestEquals_DifferentType_ReturnsFalse()
        {
            // Arrange
            var anchor = new PointAnchor (new Vector3(1, 2, 3), new Vector3(4, 5, 6));
            var other = new object();

            // Act
            var result = anchor.Equals(other);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void TestGetHashCode_PositionAndNormalDifferent_ReturnsDifferentHashCode()
        {
            // Arrange
            var anchor1 = new PointAnchor(new Vector3(1, 2, 3), new Vector3(4, 5, 6));
            var anchor2 = new PointAnchor(new Vector3(7, 8, 9), new Vector3(10, 11, 12));

            // Act
            var hash1 = anchor1.GetHashCode();
            var hash2 = anchor2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2);
        }
    }
}
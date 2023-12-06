using DataStructures.ViliWonka.KDTree;
using NUnit.Framework;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool.Tests
{
    public class KDTreeTests
    {
        [Test]
        public void Size_ReturnsCorrectValue()
        {
            // Arrange
            KDBounds bounds = new KDBounds
            {
                Min = new Vector3(0, 0, 0),
                Max = new Vector3(1, 2, 3)
            };

            // Act
            Vector3 size = bounds.Size;

            // Assert
            Assert.AreEqual(new Vector3(1, 2, 3), size);
        }

        [Test]
        public void Bounds_ReturnsCorrectValue()
        {
            // Arrange
            KDBounds bounds = new KDBounds
            {
                Min = new Vector3(-1, -2, -3),
                Max = new Vector3(1, 2, 3)
            };

            // Act
            Bounds unityBounds = bounds.Bounds;

            // Assert
            Assert.AreEqual(new Vector3(0, 0, 0), unityBounds.center);
            Assert.AreEqual(new Vector3(2, 4, 6), unityBounds.size);
        }

        [Test]
        public void ClosestPoint_ReturnsCorrectValue()
        {
            // Arrange
            KDBounds bounds = new KDBounds
            {
                Min = new Vector3(-1, -1, -1),
                Max = new Vector3(1, 1, 1)
            };
            Vector3 point = new Vector3(0, 2, 0);

            // Act
            Vector3 closestPoint = bounds.ClosestPoint(point);

            // Assert
            Assert.AreEqual(new Vector3(0, 1, 0), closestPoint);
        }

        [Test]
        public void Count_ReturnsCorrectValue()
        {
            // Arrange
            KDNode node = new KDNode
            {
                Start = 2,
                End = 7
            };

            // Act
            int count = node.Count;

            // Assert
            Assert.AreEqual(5, count);
        }

        [Test]
        public void Leaf_ReturnsTrueWhenPartitionAxisIsNegativeOne()
        {
            // Arrange
            KDNode leafNode = new KDNode
            {
                PartitionAxis = -1
            };
            KDNode nonLeafNode = new KDNode
            {
                PartitionAxis = 0
            };

            // Act
            bool isLeaf1 = leafNode.Leaf;
            bool isLeaf2 = nonLeafNode.Leaf;

            // Assert
            Assert.IsTrue(isLeaf1);
            Assert.IsFalse(isLeaf2);
        }

        [Test]
        public void NegativeChild_And_PositiveChild_CanBeSet()
        {
            // Arrange
            KDNode parentNode = new KDNode();
            KDNode negativeChildNode = new KDNode();
            KDNode positiveChildNode = new KDNode();

            // Act
            parentNode.NegativeChild = negativeChildNode;
            parentNode.PositiveChild = positiveChildNode;

            // Assert
            Assert.AreEqual(negativeChildNode, parentNode.NegativeChild);
            Assert.AreEqual(positiveChildNode, parentNode.PositiveChild);
        }
        
        [Test]
        public void BuildTree_SetsRootNode()
        {
            // Arrange
            var points = new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            var kdTree = new KDTree(points);

            // Assert
            Assert.IsNotNull(kdTree.RootNode);
        }

        [Test]
        public void BuildTree_SetsRootNodeBounds()
        {
            // Arrange
            var points = new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            var kdTree = new KDTree(points);

            // Assert
            Assert.AreEqual(new Vector3(1, 2, 3), kdTree.RootNode.Bounds.Min);
            Assert.AreEqual(new Vector3(7, 8, 9), kdTree.RootNode.Bounds.Max);
        }

        [Test]
        public void BuildTree_SetsLeafNodeBounds()
        {
            // Arrange
            var points = new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            var kdTree = new KDTree(points);

            // Assert
            Assert.AreEqual(new Vector3(1, 2, 3), kdTree.RootNode.NegativeChild.Bounds.Min);
            Assert.AreEqual(new Vector3(1, 8, 9), kdTree.RootNode.NegativeChild.Bounds.Max);
            Assert.AreEqual(new Vector3(1, 2, 3), kdTree.RootNode.PositiveChild.Bounds.Min);
            Assert.AreEqual(new Vector3(7, 8, 9), kdTree.RootNode.PositiveChild.Bounds.Max);
        }

        [Test]
        public void BuildTree_SetsLeafNodeIndices()
        {
            // Arrange
            var points = new Vector3[] { new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9) };
            var kdTree = new KDTree(points);

            // Assert
            Assert.AreEqual(0, kdTree.RootNode.NegativeChild.Start);
            Assert.AreEqual(0, kdTree.RootNode.NegativeChild.End);
            Assert.AreEqual(0, kdTree.RootNode.PositiveChild.Start);
            Assert.AreEqual(3, kdTree.RootNode.PositiveChild.End);
        }
    }
}
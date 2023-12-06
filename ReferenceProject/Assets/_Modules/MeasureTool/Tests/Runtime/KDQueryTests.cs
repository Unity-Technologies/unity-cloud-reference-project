using System.Collections.Generic;
using DataStructures.ViliWonka.KDTree;
using NUnit.Framework;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool.Tests
{
    public class KDQueryTests
    {

        [Test]
        public void PushToQueue_ShouldAddNodeToQueueArray()
        {
            // Arrange
            var query = new KDQuery();
            var node = new KDNode();
            var tempClosestPoint = new Vector3();

            // Act
            query.PushToQueue(node, tempClosestPoint);
            var popped = query.PopFromQueue();

            // Assert
            Assert.AreEqual(node, popped.Node);
            Assert.AreEqual(tempClosestPoint, popped.TempClosestPoint);
        }

        [Test]
        public void PushToHeap_ShouldAddNodeToMinHeap()
        {
            // Arrange
            var query = new KDQuery();
            var node = new KDNode();
            var tempClosestPoint = new Vector3();
            var queryPosition = new Vector3();

            // Act
            query.PushToHeap(node, tempClosestPoint, queryPosition);

            // Assert
            Assert.AreEqual(node, query.MinHeap.HeadHeapObject.Node);
            Assert.AreEqual(tempClosestPoint, query.MinHeap.HeadHeapObject.TempClosestPoint);
            Assert.AreEqual(Vector3.SqrMagnitude(tempClosestPoint - queryPosition), query.MinHeap.HeadHeapObject.Distance);
        }

        [Test]
        public void LeftToProcess_ShouldReturnCorrectCount()
        {
            // Arrange
            var query = new KDQuery();
            var initialCount = query.Count;
            var expectedCount = initialCount;

            // Act
            var actualCount = query.LeftToProcess;

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
        }

        [Test]
        public void PopFromQueue_ShouldReturnNextUnprocessedNode()
        {
            // Arrange
            var query = new KDQuery();
            var node1 = new KDNode();
            var node2 = new KDNode();
            var node3 = new KDNode();
            query.PushToQueue(node1, new Vector3());
            query.PushToQueue(node2, new Vector3());
            query.PushToQueue(node3, new Vector3());

            // Act
            var actualNode1 = query.PopFromQueue();
            var actualNode2 = query.PopFromQueue();

            // Assert
            Assert.AreEqual(node1, actualNode1.Node);
            Assert.AreEqual(node2, actualNode2.Node);
        }
    }
}
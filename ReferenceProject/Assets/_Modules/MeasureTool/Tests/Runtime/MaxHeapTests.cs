using System.Collections.Generic;
using DataStructures.ViliWonka.Heap;
using NUnit.Framework;

namespace Unity.ReferenceProject.MeasureTool.Tests
{
    public class MaxHeapTests
    {

        [Test]
        public void Count_OnCreation_ShouldBeZero()
        {
            // Arrange
            var heap = new MaxHeap();

            // Assert
            Assert.AreEqual(0, heap.Count);
        }

        [Test]
        public void Capacity_OnCreation_ShouldBe2049()
        {
            // Arrange
            var heap = new MaxHeap();

            // Assert
            Assert.AreEqual(2049, heap.Capacity);
        }

        [Test]
        public void PushValue_OnEmptyHeap_ShouldAddOneNode()
        {
            // Arrange
            var heap = new MaxHeap();

            // Act
            heap.PushValue(1.0f);

            // Assert
            Assert.AreEqual(1, heap.Count);
            Assert.AreEqual(1.0f, heap.HeadValue);
        }

        [Test]
        public void PushValue_OnFullHeap_ShouldUpsizeHeap()
        {
            // Arrange
            var heap = new MaxHeap(2);

            // Act
            heap.PushValue(1.0f);
            heap.PushValue(2.0f);
            heap.PushValue(3.0f);

            // Assert
            Assert.AreEqual(3, heap.Count);
            Assert.AreEqual(3.0f, heap.HeadValue);
        }

        [Test]
        public void PopValue_OnEmptyHeap_ShouldThrowArgumentException()
        {
            // Arrange
            var heap = new MaxHeap();

            // Assert
            Assert.Throws<System.ArgumentException>(() => heap.PopValue());
        }

        [Test]
        public void PopValue_OnHeapWithOneNode_ShouldRemoveNode()
        {
            // Arrange
            var heap = new MaxHeap();
            heap.PushValue(1.0f);

            // Act
            var result = heap.PopValue();

            // Assert
            Assert.AreEqual(0, heap.Count);
            Assert.AreEqual(1.0f, result);
        }

        [Test]
        public void PopValue_OnHeapWithMultipleNodes_ShouldRemoveAndReturnMaxValue()
        {
            // Arrange
            var heap = new MaxHeap();
            heap.PushValue(1.0f);
            heap.PushValue(3.0f);
            heap.PushValue(2.0f);

            // Act
            var result = heap.PopValue();

            // Assert
            Assert.AreEqual(2, heap.Count);
            Assert.AreEqual(2.0f, heap.HeadValue);
            Assert.AreEqual(3.0f, result);
        }

        [Test]
        public void Clear_OnHeapWithNodes_ShouldRemoveAllNodes()
        {
            // Arrange
            var heap = new MaxHeap();
            heap.PushValue(1.0f);
            heap.PushValue(3.0f);

            // Act
            heap.Clear();

            // Assert
            Assert.AreEqual(0, heap.Count);
        }
        
        [Test]
        public void TestFlushHeapResult()
        {
            var maxHeap = new MaxHeap(10);
            maxHeap.PushValue(1f);
            maxHeap.PushValue(3f);
            maxHeap.PushValue(2f);

            var heapList = new List<float>();
            maxHeap.FlushHeapResult(heapList);

            Assert.AreEqual(3, heapList.Count);
            Assert.AreEqual(3f, heapList[0]);
            Assert.AreEqual(1f, heapList[1]);
            Assert.AreEqual(2f, heapList[2]);
        }
    }
}
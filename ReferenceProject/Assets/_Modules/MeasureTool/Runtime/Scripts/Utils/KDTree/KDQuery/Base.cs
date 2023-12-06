/*MIT License

Copyright(c) 2018 Vili Volčini / viliwonka

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

/*
The object used for querying. This object should be persistent - re-used for querying.
Contains internal array for pooling, so that it doesn't generate (too much) garbage.
The array never down-sizes, only up-sizes, so the more you use this object, less garbage will it make over time.

Should be used only by 1 thread,
which means each thread should have it's own KDQuery object in order for querying to be thread safe.

KDQuery can query different KDTrees.
*/


using System.Collections.Generic;
using UnityEngine;
using System;
using DataStructures.ViliWonka.Heap;

namespace DataStructures.ViliWonka.KDTree {

    public partial class KDQuery {

        protected KDQueryNode[] queueArray;  // queue array
        protected Heap.MinHeap<KDQueryNode>  minHeap; //heap for k-nearest
        protected int count = 0;             // size of queue
        protected int queryIndex = 0;        // current index at stack

        /// <summary>
        /// Returns initialized node from stack that also acts as a pool
        /// The returned reference to node stays in stack
        /// </summary>
        /// <returns>Reference to pooled node</returns>
        private KDQueryNode PushGetQueue() {

            KDQueryNode node = null;

            if (Count < queueArray.Length) {

                if (queueArray[Count] == null)
                    queueArray[Count] = node = new KDQueryNode();
                else
                    node = queueArray[Count];
            }
            else {

                // automatic resize of pool
                Array.Resize(ref queueArray, queueArray.Length * 2);
                node = queueArray[Count] = new KDQueryNode();
            }

            Count++;

            return node;
        }

        public void PushToQueue(KDNode node, Vector3 tempClosestPoint) {

            var queryNode = PushGetQueue();
            queryNode.Node = node;
            queryNode.TempClosestPoint = tempClosestPoint;
        }

        public void PushToHeap(KDNode node, Vector3 tempClosestPoint, Vector3 queryPosition) {

            var queryNode = PushGetQueue();
            queryNode.Node = node;
            queryNode.TempClosestPoint = tempClosestPoint;

            var sqrDist = Vector3.SqrMagnitude(tempClosestPoint - queryPosition);
            queryNode.Distance = sqrDist;
            MinHeap.PushObj(queryNode, sqrDist);
        }

        public int LeftToProcess {

            get {
                return Count - queryIndex;
            }
        }

        public int Count
        {
            get => count;
            private set => count = value;
        }
        
        public int Capacity
        {
            get => minHeap.Capacity;
        }

        public MinHeap<KDQueryNode> MinHeap
        {
            get => minHeap;
            private set => minHeap = value;
        }

        // just gets unprocessed node from stack
        // increases queryIndex
        public KDQueryNode PopFromQueue() {

            var node = queueArray[queryIndex];
            queryIndex++;

            return node;
        }

        public KDQueryNode PopFromHeap() {

            var heapNode = MinHeap.PopObj();

            queueArray[queryIndex]= heapNode;
            queryIndex++;

            return heapNode;
        }

        protected void Reset() {

            Count = 0;
            queryIndex = 0;
            MinHeap.Clear();
        }

        public KDQuery(int queryNodesContainersInitialSize = 2048) {
            queueArray = new KDQueryNode[queryNodesContainersInitialSize];
            MinHeap = new Heap.MinHeap<KDQueryNode>(queryNodesContainersInitialSize);
        }
    }

}
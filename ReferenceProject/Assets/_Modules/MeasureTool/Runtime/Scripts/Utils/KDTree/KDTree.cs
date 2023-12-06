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

// change to !KDTREE_DUPLICATES
// if you know for sure you will not use duplicate coordinates (all unique)

#define KDTREE_DUPLICATES

using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.NetworkInformation;
using UnityEngine;

namespace DataStructures.ViliWonka.KDTree
{
    public class KDTree
    {
        public KDNode RootNode { get; private set; }

        public Vector3[] Points
        {
            get { return points; }
        } // points on which kd-tree will build on. This array will stay unchanged when re/building kdtree!

        private Vector3[] points;

        public int[] Permutation
        {
            get { return permutation; }
        } // index aray, that will be permuted

        private int[] permutation;

        public int Count { get; private set; }

        private int maxPointsPerLeafNode;

        private KDNode[] kdNodesStack;
        private int kdNodesCount = 0;

        public KDTree(int maxPointsPerLeafNode = 16)
        {
            Count = 0;
            points = new Vector3[0];
            permutation = new int[0];

            kdNodesStack = new KDNode[64];

            this.maxPointsPerLeafNode = maxPointsPerLeafNode;
        }

        public KDTree(Vector3[] points, int maxPointsPerLeafNode = 16)
        {
            this.points = points;
            this.permutation = new int[points.Length];

            Count = points.Length;
            kdNodesStack = new KDNode[64];

            this.maxPointsPerLeafNode = maxPointsPerLeafNode;

            Rebuild();
        }

        public void Build(Vector3[] newPoints, int maxPointsPerLeafNode = -1)
        {
            SetCount(newPoints.Length);

            for (int i = 0; i < Count; i++)
            {
                points[i] = newPoints[i];
            }

            Rebuild(maxPointsPerLeafNode);
        }

        public void Build(List<Vector3> newPoints, int maxPointsPerLeafNode = -1)
        {
            SetCount(newPoints.Count);

            for (int i = 0; i < Count; i++)
            {
                points[i] = newPoints[i];
            }

            Rebuild(maxPointsPerLeafNode);
        }

        public void Rebuild(int maxPointsPerLeafNode = -1)
        {
            SetCount(Count);

            for (int i = 0; i < Count; i++)
            {
                permutation[i] = i;
            }

            if (maxPointsPerLeafNode > 0)
            {
                this.maxPointsPerLeafNode = maxPointsPerLeafNode;
            }

            BuildTree();
        }

        public void SetCount(int newSize)
        {
            Count = newSize;
            // upsize internal arrays
            if (Count > points.Length)
            {
                Array.Resize(ref points, Count);
                Array.Resize(ref permutation, Count);
            }
        }

        void BuildTree()
        {
            ResetKDNodeStack();

            RootNode = GetKDNode();
            RootNode.Bounds = MakeBounds();
            RootNode.Start = 0;
            RootNode.End = Count;

            SplitNode(RootNode);
        }

        KDNode GetKDNode()
        {
            KDNode node = null;

            if (kdNodesCount < kdNodesStack.Length)
            {
                if (kdNodesStack[kdNodesCount] == null)
                {
                    kdNodesStack[kdNodesCount] = node = new KDNode();
                }
                else
                {
                    node = kdNodesStack[kdNodesCount];
                    node.PartitionAxis = -1;
                }
            }
            else
            {
                // automatic resize of KDNode pool array
                Array.Resize(ref kdNodesStack, kdNodesStack.Length * 2);
                node = kdNodesStack[kdNodesCount] = new KDNode();
            }

            kdNodesCount++;

            return node;
        }

        void ResetKDNodeStack()
        {
            kdNodesCount = 0;
        }

        /// <summary>
        /// For calculating root node bounds
        /// </summary>
        /// <returns>Boundary of all Vector3 points</returns>
        KDBounds MakeBounds()
        {
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            int even = Count & ~1; // calculate even Length

            // min, max calculations
            // 3n/2 calculations instead of 2n
            for (int i0 = 0; i0 < even; i0 += 2)
            {
                int i1 = i0 + 1;

                // X Coords
                min.x = ComparePoints(points[i0].x, points[i1].x, min.x, ref max.x);
                min.y = ComparePoints(points[i0].y, points[i1].y, min.y, ref max.y);
                min.z = ComparePoints(points[i0].z, points[i1].z, min.z, ref max.z);
            }

            // if array was odd, calculate also min/max for the last element
            if (even != Count)
            {
                ComparePoint(points[even].x, ref min.x, ref max.x);
                ComparePoint(points[even].y, ref min.y, ref max.y);
                ComparePoint(points[even].z, ref min.z, ref max.z);
            }

            KDBounds b = new KDBounds();
            b.Min = min;
            b.Max = max;

            return b;
        }

        private void ComparePoint(float i0, ref float min, ref float max)
        {
            if (min > i0)
                min = i0;

            if (max < i0)
                max = i0;
        }

        private float ComparePoints(float i0, float i1, float min, ref float max)
        {
            if (i0 > i1)
            {
                // i0 is bigger, i1 is smaller
                if (i1 < min)
                    min = i1;

                if (i0 > max)
                    max = i0;
            }
            else
            {
                // i1 is smaller, i0 is bigger
                if (i0 < min)
                    min = i0;

                if (i1 > max)
                    max = i1;
            }

            return min;
        }

        /// <summary>
        /// Recursive splitting procedure
        /// </summary>
        /// <param name="parent">This is where root node goes</param>
        /// <param name="depth"></param>
        ///
        void SplitNode(KDNode parent)
        {
            // center of bounding box
            KDBounds parentBounds = parent.Bounds;
            Vector3 parentBoundsSize = parentBounds.Size;

            // Find axis where bounds are largest
            int splitAxis = 0;
            float axisSize = parentBoundsSize.x;

            if (axisSize < parentBoundsSize.y)
            {
                splitAxis = 1;
                axisSize = parentBoundsSize.y;
            }

            if (axisSize < parentBoundsSize.z)
            {
                splitAxis = 2;
            }

            // Our axis min-max bounds
            float boundsStart = parentBounds.Min[splitAxis];
            float boundsEnd = parentBounds.Max[splitAxis];

            // Calculate the spliting coords
            float splitPivot = CalculatePivot(parent.Start, parent.End, boundsStart, boundsEnd, splitAxis);

            parent.PartitionAxis = splitAxis;
            parent.PartitionCoordinate = splitPivot;

            // 'Spliting' array to two subarrays
            int splittingIndex = Partition(parent.Start, parent.End, splitPivot, splitAxis);

            // Negative / Left node
            Vector3 negMax = parentBounds.Max;
            negMax[splitAxis] = splitPivot;

            KDNode negNode = GetKDNode();
            negNode.Bounds = parentBounds;
            var negNodeBounds = negNode.Bounds;
            negNodeBounds.Max = negMax;
            negNode.Bounds = negNodeBounds;
            negNode.Start = parent.Start;
            negNode.End = splittingIndex;
            parent.NegativeChild = negNode;

            // Positive / Right node
            Vector3 posMin = parentBounds.Min;
            posMin[splitAxis] = splitPivot;

            KDNode posNode = GetKDNode();
            posNode.Bounds = parentBounds;
            var posNodeBounds = posNode.Bounds;
            posNodeBounds.Min = posMin;
            posNode.Bounds = posNodeBounds;
            posNode.Start = splittingIndex;
            posNode.End = parent.End;
            parent.PositiveChild = posNode;

            // check if we are actually splitting it anything
            // this if check enables duplicate coordinates, but makes construction a bit slower
#if KDTREE_DUPLICATES
            if (negNode.Count != 0 && posNode.Count != 0)
            {
#endif
                // Constraint function deciding if split should be continued
                if (ContinueSplit(negNode))
                    SplitNode(negNode);


                if (ContinueSplit(posNode))
                    SplitNode(posNode);

#if KDTREE_DUPLICATES
            }
#endif
        }

        /// <summary>
        /// Sliding midpoint splitting pivot calculation
        /// 1. First splits node to two equal parts (midPoint)
        /// 2. Checks if elements are in both sides of splitted bounds
        /// 3a. If they are, just return midPoint
        /// 3b. If they are not, then points are only on left or right bound.
        /// 4. Move the splitting pivot so that it shrinks part with points completely (calculate min or max dependent) and return.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="boundsStart"></param>
        /// <param name="boundsEnd"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        float CalculatePivot(int start, int end, float boundsStart, float boundsEnd, int axis)
        {
            //! sliding midpoint rule
            float midPoint = (boundsStart + boundsEnd) / 2f;

            bool negative = false;
            bool positive = false;

            float negMax = Single.MinValue;
            float posMin = Single.MaxValue;

            // this for loop section is used both for sorted and unsorted data
            for (int i = start; i < end; i++)
            {
                negative = points[permutation[i]][axis] < midPoint;
                positive = points[permutation[i]][axis] > midPoint;

                if (negative && positive)
                    return midPoint;
            }

            if (negative)
            {
                for (int i = start; i < end; i++)
                    if (negMax < points[permutation[i]][axis])
                        negMax = points[permutation[i]][axis];

                return negMax;
            }

            for (int i = start; i < end; i++)
                if (posMin > points[permutation[i]][axis])
                    posMin = points[permutation[i]][axis];

            return posMin;
        }

        /// <summary>
        /// Similar to Hoare partitioning algorithm (used in Quick Sort)
        /// Modification: pivot is not left-most element but is instead argument of function
        /// Calculates splitting index and partially sorts elements (swaps them until they are on correct side - depending on pivot)
        /// Complexity: O(n)
        /// </summary>
        /// <param name="start">Start index</param>
        /// <param name="end">End index</param>
        /// <param name="partitionPivot">Pivot that decides boundary between left and right</param>
        /// <param name="axis">Axis of this pivoting</param>
        /// <returns>
        /// Returns splitting index that subdivides array into 2 smaller arrays
        /// left = [start, pivot),
        /// right = [pivot, end)
        /// </returns>
        int Partition(int start, int end, float partitionPivot, int axis)
        {
            // note: increasing right pointer is actually decreasing!
            int LP = start - 1; // left pointer (negative side)
            int RP = end; // right pointer (positive side)

            int temp; // temporary var for swapping permutation indexes

            while (true)
            {
                do
                {
                    // move from left to the right until "out of bounds" value is found
                    LP++;
                } while (LP < RP && points[permutation[LP]][axis] < partitionPivot);

                do
                {
                    // move from right to the left until "out of bounds" value found
                    RP--;
                } while (LP < RP && points[permutation[RP]][axis] >= partitionPivot);

                if (LP < RP)
                {
                    // swap
                    temp = permutation[LP];
                    permutation[LP] = permutation[RP];
                    permutation[RP] = temp;
                }
                else
                {
                    return LP;
                }
            }
        }

        /// <summary>
        /// Constraint function. You can add custom constraints here - if you have some other data/classes binded to Vector3 points
        /// Can hardcode it into
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool ContinueSplit(KDNode node)
        {
            return (node.Count > maxPointsPerLeafNode);
        }
    }
}
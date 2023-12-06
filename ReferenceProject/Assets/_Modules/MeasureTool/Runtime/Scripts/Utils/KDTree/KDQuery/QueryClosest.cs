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

using System.Collections.Generic;
using UnityEngine;
using System;

namespace DataStructures.ViliWonka.KDTree
{
    public partial class KDQuery
    {
        public void ClosestPoint(KDTree tree, Vector3 queryPosition, List<int> resultIndices,
            List<float> resultDistances = null)
        {
            Reset();

            var points = tree.Points;
            var permutation = tree.Permutation;
            var smallestIndex = 0;
            
            /// Smallest Squared Radius
            var SSR = Single.PositiveInfinity;
            var rootNode = tree.RootNode;
            var rootClosestPoint = rootNode.Bounds.ClosestPoint(queryPosition);

            PushToHeap(rootNode, rootClosestPoint, queryPosition);

            KDQueryNode queryNode = null;
            KDNode node = null;

            // searching
            while (MinHeap.Count > 0)
            {
                queryNode = PopFromHeap();

                if (queryNode.Distance > SSR)
                    continue;

                node = queryNode.Node;

                if (!node.Leaf)
                {
                    HandleHeap(queryPosition, node, queryNode);
                }
                else
                {
                    SSR = HandleLeaf(queryPosition, node, permutation, points, SSR, ref smallestIndex);
                }
            }

            resultIndices.Add(smallestIndex);
            resultDistances?.Add(SSR);
        }

        private void HandleHeap(Vector3 queryPosition, KDNode node, KDQueryNode queryNode)
        {
            var partitionAxis = node.PartitionAxis;
            var partitionCoord = node.PartitionCoordinate;

            var tempClosestPoint = queryNode.TempClosestPoint;

            var child = tempClosestPoint[partitionAxis] - partitionCoord < 0 ? node.NegativeChild : node.PositiveChild;
            var oppositeChild = tempClosestPoint[partitionAxis] - partitionCoord < 0 ? node.PositiveChild : node.NegativeChild;

            PushToHeap(child, tempClosestPoint, queryPosition);
            // project the tempClosestPoint to other bound
            tempClosestPoint[partitionAxis] = partitionCoord;

            if (node.PositiveChild.Count == 0)
                return;
            
            PushToHeap(oppositeChild, tempClosestPoint, queryPosition);
        }

        private static float HandleLeaf(Vector3 queryPosition, KDNode node, int[] permutation, Vector3[] points, float SSR,
            ref int smallestIndex)
        {
            // LEAF
            for (var i = node.Start; i < node.End; i++)
            {
                var index = permutation[i];

                var sqrDist = Vector3.SqrMagnitude(points[index] - queryPosition);

                if (sqrDist > SSR) continue;
                SSR = sqrDist;
                smallestIndex = index;
            }

            return SSR;
        }
    }
}
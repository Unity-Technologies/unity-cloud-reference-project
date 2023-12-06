using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public class PointAnchor : IAnchor
    {
        public Vector3 Position { get; }
        public Vector3 Normal { get; }

        public PointAnchor(Vector3 position, Vector3 normal, List<Vector3> triangleVertices = null)
        {
            Position = triangleVertices != null ? DetectBestPointSelection(position, triangleVertices) : position;
            Normal = normal;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PointAnchor)obj);
        }

        private bool Equals(PointAnchor other)
        {
            return Position.Equals(other.Position) && Normal.Equals(other.Normal);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Normal);
        }

        // Basic Vertices Snapping
        private Vector3 DetectBestPointSelection(Vector3 hitPoint, List<Vector3> triangleVertices)
        {
            triangleVertices.Sort((x, y) =>
            {
                return (hitPoint - x).sqrMagnitude.CompareTo((hitPoint - y).sqrMagnitude);
            });

            if (Vector3.Distance(triangleVertices[0], hitPoint) < 0.1f)
            {
                return triangleVertices[0];
            }

            return hitPoint;
        }
    }
}
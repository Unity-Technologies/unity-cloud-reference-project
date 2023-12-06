using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool.Utils
{
    public static class LineMath
    {
        public static bool IsLineGreaterThanMinScreenBounds(List<IAnchor> anchorPoints, Camera camera, float distance)
        {
            if (anchorPoints == null || anchorPoints.Count == 0)
                throw new ArgumentException("Anchor points is null or empty.");

            if (camera == null)
                throw new ArgumentException("Camera is null.");
            
            var screenCoord = camera.WorldToScreenPoint(anchorPoints[0].Position);
            var bounds = new Bounds(screenCoord, Vector3.zero);
            for (var i = 1; i < anchorPoints.Count; i++)
            {
                screenCoord = camera.WorldToScreenPoint(anchorPoints[i].Position);
                bounds.Encapsulate(screenCoord);
            }

            return bounds.size.sqrMagnitude >= distance * distance;
        }

        public static float GetLineMagnitude(List<IAnchor> anchorPoints)
        {
            if (anchorPoints == null || anchorPoints.Count == 0)
                throw new ArgumentException("Anchor points is null or empty.");
            
            var total = 0.0f;
            for (var i = 0; i < anchorPoints.Count-1; i++)
            {
                var sqrMagnitude = (anchorPoints[i].Position - anchorPoints[i+1].Position).sqrMagnitude;
                total += sqrMagnitude;
            }

            var sqrt = Mathf.Sqrt(total);
            return sqrt;
        }
        
        public static Vector3 FindCenter(List<IAnchor> anchorPoints)
        {
            if (anchorPoints == null || anchorPoints.Count == 0)
                throw new ArgumentException("Anchor points is null or empty.");
            
            var bound = new Bounds(anchorPoints[0].Position, Vector3.zero);
            for(var i = 1; i < anchorPoints.Count; i++)
                bound.Encapsulate(anchorPoints[i].Position);
            return bound.center;
        }
        
        public static Vector3 ProjectPoint(Vector3 point, Vector3 p1, Vector3 p2, float tMin, float tMax)
        {
            var d = p2 - p1;
            var dDot = Vector3.Dot(d, d);

            if (Mathf.Approximately(dDot, 0.0f))
                return point;

            var t = Vector3.Dot(point - p1, d) / dDot;
            return p1 + d * Mathf.Clamp(t, tMin, tMax);
        }
    }
}
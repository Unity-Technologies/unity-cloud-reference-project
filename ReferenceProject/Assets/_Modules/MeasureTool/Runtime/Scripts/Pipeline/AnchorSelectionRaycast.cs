using System;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public static class AnchorSelectionRaycast
    {
        public static IAnchor PostRaycast(Vector3 worldPos, Vector3 normal)
        {
            return new PointAnchor(worldPos, normal);
        }
    }
}

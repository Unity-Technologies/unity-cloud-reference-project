using System;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public interface IAnchorSelector
    {
        IAnchor Pick(Vector3 worldPosition, Vector3 normal);
    }
    
    public class ObjectPickerAnchorSelector : IAnchorSelector
    {
        public IAnchor Pick(Vector3 worldPosition, Vector3 normal)
        {
            return AnchorSelectionRaycast.PostRaycast(worldPosition, normal);
        }
    }
}
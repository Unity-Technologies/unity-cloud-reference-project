using System;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public interface IObjectSelectionInfo
    {
        public bool HasIntersected { get; }
        public InstanceId SelectedInstanceId { get; }
        public Vector3 SelectedPosition { get; }
        public Vector3 SelectedNormal { get; }
    }

    public class ObjectSelectionInfo : IObjectSelectionInfo
    {
        public bool HasIntersected { get; }
        public InstanceId SelectedInstanceId { get; }
        public Vector3 SelectedPosition { get; }
        public Vector3 SelectedNormal { get; }
        
        public static ObjectSelectionInfo NoIntersection => new (false, InstanceId.None, Vector3.zero, Vector3.zero);

        public ObjectSelectionInfo(bool hasIntersected, InstanceId selectedInstanceId, Vector3 selectedPosition, Vector3 selectedNormal)
        {
            HasIntersected = hasIntersected;
            SelectedInstanceId = selectedInstanceId;
            SelectedPosition = selectedPosition;
            SelectedNormal = selectedNormal;
        }
    }
}

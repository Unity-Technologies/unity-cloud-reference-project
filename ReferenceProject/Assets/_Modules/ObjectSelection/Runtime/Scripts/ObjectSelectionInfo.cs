using System;
using Unity.Cloud.Common;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public interface IObjectSelectionInfo
    {
        public InstanceId SelectedInstanceId { get; set; }
        public Vector3 SelectedPosition { get; set; }
    }

    public class ObjectSelectionInfo : IObjectSelectionInfo
    {
        public InstanceId SelectedInstanceId { get; set; }
        public Vector3 SelectedPosition { get; set; }
    }
}

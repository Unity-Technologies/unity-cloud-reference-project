using System;
using System.Threading.Tasks;
using Unity.Cloud.Common;
using Unity.Cloud.DataStreaming.Runtime;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public struct PathPickerResult
    {
        public int Index { get; set; }
        public PickerResult PickerResult { get; set; }
    }

    public readonly struct PickerResult
    {
        public static readonly PickerResult Invalid = new(RaycastResult.Invalid);
        
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
        public bool HasIntersected => m_HighPrecisionResult.HasIntersected;
        public InstanceId InstanceId => m_HighPrecisionResult.InstanceId;
        public float Distance => (float)m_HighPrecisionResult.Distance;

        readonly RaycastResult m_HighPrecisionResult;
        
        public PickerResult(RaycastResult raycastResult)
        {
            Point = new Vector3((float)raycastResult.Point.x, (float)raycastResult.Point.y, (float)raycastResult.Point.z);
            Normal = new Vector3(raycastResult.Normal.x, raycastResult.Normal.y, raycastResult.Normal.z);
            m_HighPrecisionResult = raycastResult;
        }
    }

    public interface IObjectPicker
    {
        Task<PickerResult> PickAsync(Ray ray, float maxDistance = 1000f);
        Task<PathPickerResult> PickFromPathAsync(Vector3[] points);
    }
}

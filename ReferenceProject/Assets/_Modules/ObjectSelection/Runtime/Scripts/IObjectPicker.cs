using System;
using System.Threading.Tasks;
using Unity.Cloud.DataStreaming.Runtime;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public struct PathRaycastResult
    {
        public int Index { get; set; }
        public RaycastResult RaycastResult { get; set; }
    }

    public interface IObjectPicker
    {
        Task<RaycastResult> RaycastAsync(Ray ray, float maxDistance = 1000f);
        Task<PathRaycastResult> PickFromPathAsync(Vector3[] points);
    }
}

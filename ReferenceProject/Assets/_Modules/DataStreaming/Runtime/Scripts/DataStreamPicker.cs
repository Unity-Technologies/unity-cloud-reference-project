using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;

namespace Unity.ReferenceProject.DataStreaming
{
    public class DataStreamPicker : IObjectPicker
    {
        public Task<List<(GameObject, RaycastHit)>> PickFromRayAsync(Ray ray)
        {
            return NotSupported();
        }

        public Task<List<(GameObject, RaycastHit)>> PickFromSphereAsync(Vector3 center, float radius)
        {
            return NotSupported();
        }

        public Task<List<(GameObject, RaycastHit)>> PickFromPathAsync(List<Vector3> path)
        {
            return NotSupported();
        }

        static Task<List<(GameObject, RaycastHit)>> NotSupported()
        {
            Debug.LogWarning("Picking not supported yet.");
            return Task.FromResult(new List<(GameObject, RaycastHit)>());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public interface IObjectPicker
    {
        Task<List<(GameObject gameObject, RaycastHit raycastHit)>> PickFromRayAsync(Ray ray);
        Task<List<(GameObject gameObject, RaycastHit raycastHit)>> PickFromSphereAsync(Vector3 center, float radius);
        Task<List<(GameObject gameObject, RaycastHit raycastHit)>> PickFromPathAsync(List<Vector3> path);
    }
}

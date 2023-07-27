using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.DataStreaming
{
    public class DataStreamPicker : IObjectPicker
    {
        static readonly SemaphoreSlim m_Semaphore = new SemaphoreSlim(1,1);
        IDataStreamerProvider m_DataStreamProvider;

        const float k_MaxDistance = 1000f;

        [Inject]
        void Setup(IDataStreamerProvider dataStreamerProvider)
        {
            m_DataStreamProvider = dataStreamerProvider;
        }

        public async Task<RaycastResult> RaycastAsync(Ray ray, float maxDistance = k_MaxDistance)
        {
            try
            {
                await m_Semaphore.WaitAsync();
                var raycastResult = await m_DataStreamProvider.DataStreamer.RaycastAsync(ray, maxDistance);
                return raycastResult;
            }
            finally
            {
                m_Semaphore.Release(1);
            }
        }

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
            Debug.LogWarning("Picking feature not supported yet.");
            return Task.FromResult(new List<(GameObject, RaycastHit)>());
        }
    }
}

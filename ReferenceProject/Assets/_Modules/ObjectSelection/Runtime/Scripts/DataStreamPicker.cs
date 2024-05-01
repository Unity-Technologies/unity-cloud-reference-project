using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.ReferenceProject.DataStreaming;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class DataStreamPicker : IObjectPicker
    {
        IStage m_Stage;

        const float k_MaxDistance = 1000f;

        struct RayData
        {
            public Ray Ray;
            public float Distance;
        }

        [Inject]
        void Setup(IDataStreamerProvider dataStreamerProvider)
        {
            dataStreamerProvider.DataStreamer.StageCreated.Subscribe(stage => m_Stage = stage);
            dataStreamerProvider.DataStreamer.StageDestroyed.Subscribe(() => m_Stage = null);
        }

        public async Task<IPickerResult> PickAsync(Ray ray, float maxDistance = k_MaxDistance)
        {
            var raycastResult = new PickerResult(await m_Stage.RaycastAsync(ray, maxDistance));
            return raycastResult;
        }

        public async Task<IPathPickerResult> PickFromPathAsync(Vector3[] points)
        {
            List<RayData> rays = new List<RayData>();
            for (int i = 0; i < points.Length - 2; i++)
            {
                var rayData = new RayData
                {
                    Ray = new Ray(points[i], points[i + 1] - points[i]),
                    Distance = Vector3.Distance(points[i], points[i + 1])
                };
                rays.Add(rayData);
            }

            var tasks = rays.Select(r => PickAsync(r.Ray, r.Distance));
            var results = await Task.WhenAll(tasks);
            for(int i=0; i<results.Length; i++)
            {
                if (results[i].HasIntersected)
                {
                    return new PathPickerResult
                    {
                        Index = i,
                        PickerResult = results[i]
                    };
                }
            }

            return new PathPickerResult {Index = -1, PickerResult = PickerResult.Invalid};
        }
    }
}

using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    [JsonConverter(typeof(AnchorConverter))]
    public interface IAnchor
    {
        Vector3 Position { get; }
        Vector3 Normal { get; }
    }
}
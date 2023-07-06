using System;
using Unity.Properties;
using Unity.ReferenceProject.DataStores;
using UnityEngine;

namespace Unity.ReferenceProject.WorldSpaceUIDocumentExtensions
{
    public class ControllerStore : DataStore<ControllerViewModel> { }

    [Serializable, GeneratePropertyBag]
    public struct ControllerViewModel : IEquatable<ControllerViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IControllerInfo ControllerInfo { get; set; }

        public bool Equals(ControllerViewModel other)
        {
            return Equals(ControllerInfo, other.ControllerInfo);
        }

        public override bool Equals(object obj)
        {
            return obj is ControllerViewModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (ControllerInfo != null ? ControllerInfo.GetHashCode() : 0);
        }
    }
}

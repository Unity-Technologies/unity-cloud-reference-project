using System;
using Unity.Properties;
using Unity.ReferenceProject.DataStores;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class ObjectSelectionStore : DataStore<ObjectSelectionViewModel> { }

    [Serializable, GeneratePropertyBag]
    public struct ObjectSelectionViewModel : IEquatable<ObjectSelectionViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IObjectSelectionInfo SelectionInfo { get; set; }

        public bool Equals(ObjectSelectionViewModel other)
        {
            return Equals(SelectionInfo, other.SelectionInfo);
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectSelectionViewModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (SelectionInfo != null ? SelectionInfo.GetHashCode() : 0);
        }
    }
}

using System;
using Unity.Cloud.Common;
using Unity.Properties;
using Unity.ReferenceProject.DataStores;
using UnityEngine;

namespace Unity.ReferenceProject.ScenesList
{
    public class SceneListStore : DataStore<ScenesListViewModel> { }

    [Serializable, GeneratePropertyBag]
    public struct ScenesListViewModel : IEquatable<ScenesListViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IScene SelectedScene { get; set; }

        public bool Equals(ScenesListViewModel other)
        {
            return SelectedScene.Id == other.SelectedScene.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is ScenesListViewModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return SelectedScene != null ? SelectedScene.GetHashCode() : 0;
        }
    }
}

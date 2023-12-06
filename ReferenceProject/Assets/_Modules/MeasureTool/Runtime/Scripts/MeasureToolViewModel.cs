using System;
using Unity.Properties;
using Unity.ReferenceProject.DataStores;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasureToolDataStore : DataStore<MeasureToolViewModel> { }

    [Serializable, GeneratePropertyBag]
    public struct MeasureToolViewModel : IEquatable<MeasureToolViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public MeasureFormat MeasureFormat { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject SelectedCursor { get; set; }

        public static readonly MeasureToolViewModel DefaultData = new ()
        {
            SelectedCursor = null,
            MeasureFormat = MeasureFormat.Meters,
        };

        public bool Equals(MeasureToolViewModel other)
        {
            return SelectedCursor == other.SelectedCursor;
        }

        public override bool Equals(object obj)
        {
            return obj is MeasureToolViewModel other && Equals(other);
        }

        public static bool operator ==(MeasureToolViewModel a, MeasureToolViewModel b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MeasureToolViewModel a, MeasureToolViewModel b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return SelectedCursor.GetHashCode();
        }
    }
}

using System;
using Unity.Properties;
using Unity.ReferenceProject.DataStores;
using Unity.ReferenceProject.WorldSpaceUIDocumentExtensions;
using UnityEngine;

namespace Unity.ReferenceProject.VR.RigUI
{
    public class MenuTypeStore : DataStore<MenuTypeViewModel> { }

    [Serializable, GeneratePropertyBag]
    public struct MenuTypeViewModel : IEquatable<MenuTypeViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public MenuType MenuType { get; set; }

        public bool Equals(MenuTypeViewModel other)
        {
            return Equals(MenuType, other.MenuType);
        }

        public override bool Equals(object obj)
        {
            return obj is ControllerViewModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (MenuType != null ? MenuType.GetHashCode() : 0);
        }
    }
}

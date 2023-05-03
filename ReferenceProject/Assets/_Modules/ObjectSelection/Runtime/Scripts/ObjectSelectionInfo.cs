using System;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public interface IObjectSelectionInfo
    {
        public GameObject SelectedGameObject { get; set; }
    }

    public class ObjectSelectionInfo : IObjectSelectionInfo
    {
        public GameObject SelectedGameObject { get; set; }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.MeasureTool
{
    [DisallowMultipleComponent]
    public class AnchorSelectionContext : MonoBehaviour
    {
        [Serializable]
        public struct SelectionContext
        {
            public IAnchor SelectedAnchor { get; set; }
        }

        private List<SelectionContext> _selectionContextList;
        public SelectionContext LastContext => _selectionContextList[_selectionContextList.Count - 1];
        public List<SelectionContext> SelectionContextList => _selectionContextList;

        void Awake()
        {
            _selectionContextList = new List<SelectionContext>();
        }
    }
}

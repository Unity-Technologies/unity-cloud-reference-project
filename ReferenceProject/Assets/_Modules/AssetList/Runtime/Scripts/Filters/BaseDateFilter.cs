using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.Cloud.Assets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.AssetList
{
    public abstract class BaseDateFilter : IFilter
    {
        public string Label { get; set; }
        public bool IsUsed { get; }

        protected BaseDateFilter(string label)
        {
            Label = label;
        }

        public void FillContent(VisualElement container, IEnumerable<IAsset> assets, AssetSearchFilter searchFilter)
        {
            container.Add(new Text("Coming soon!"));
        }

        public void ApplyFilter(AssetSearchFilter searchFilter, Chip filterChip)
        {
            ItemChosen?.Invoke();
        }

        public abstract void ClearFilter(AssetSearchFilter searchFilter);

        public event Action ItemChosen;
    }
}

using System;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.UI
{
    /// <summary>
    /// SearchBar UI element.
    /// </summary>
    public class SearchBar : TextField
    {
        /// <summary>
        /// The SearchBar main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-searchbar";

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SearchBar()
        {
            AddToClassList(ussClassName);
            leadingIconName = "magnifying-glass";
            placeholder = "Search...";
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="SearchBar"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<SearchBar, UxmlTraits> { }
    }
}

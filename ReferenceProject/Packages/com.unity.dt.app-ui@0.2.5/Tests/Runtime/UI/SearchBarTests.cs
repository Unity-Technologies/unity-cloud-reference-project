using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SearchBar))]
    class SearchBarTests : VisualElementTests<SearchBar>
    {
        protected override string mainUssClassName => SearchBar.ussClassName;
    }
}

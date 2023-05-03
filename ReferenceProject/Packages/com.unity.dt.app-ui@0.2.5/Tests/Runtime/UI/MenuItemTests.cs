using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuItem))]
    class MenuItemTests : VisualElementTests<MenuItem>
    {
        protected override string mainUssClassName => MenuItem.ussClassName;
    }
}

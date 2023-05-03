using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuDivider))]
    class MenuDividerTests : VisualElementTests<MenuDivider>
    {
        protected override string mainUssClassName => Divider.ussClassName;
    }
}

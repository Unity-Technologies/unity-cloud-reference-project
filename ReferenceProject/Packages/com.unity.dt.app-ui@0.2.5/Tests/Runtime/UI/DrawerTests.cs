using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Drawer))]
    class DrawerTests : VisualElementTests<Drawer>
    {
        protected override string mainUssClassName => Drawer.ussClassName;
    }
}

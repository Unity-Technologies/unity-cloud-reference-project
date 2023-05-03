using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Menu))]
    class MenuTests : VisualElementTests<Menu>
    {
        protected override string mainUssClassName => Menu.ussClassName;
    }
}

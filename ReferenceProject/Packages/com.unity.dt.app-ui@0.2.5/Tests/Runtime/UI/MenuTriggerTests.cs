using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuTrigger))]
    class MenuTriggerTests : VisualElementTests<MenuTrigger>
    {
        protected override string mainUssClassName => null;
    }
}

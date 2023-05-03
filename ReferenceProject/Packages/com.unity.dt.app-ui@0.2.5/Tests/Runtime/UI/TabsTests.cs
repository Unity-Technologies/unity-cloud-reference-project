using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Tabs))]
    class TabsTests : VisualElementTests<Tabs>
    {
        protected override string mainUssClassName => Tabs.ussClassName;
    }
}

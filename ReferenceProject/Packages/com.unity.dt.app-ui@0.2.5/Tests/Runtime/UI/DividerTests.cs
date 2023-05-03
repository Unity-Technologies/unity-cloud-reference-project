using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Divider))]
    class DividerTests : VisualElementTests<Divider>
    {
        protected override string mainUssClassName => Divider.ussClassName;
    }
}

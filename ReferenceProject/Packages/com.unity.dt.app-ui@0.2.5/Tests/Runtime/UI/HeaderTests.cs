using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Header))]
    class HeaderTests : VisualElementTests<Header>
    {
        protected override string mainUssClassName => Header.ussClassName;
    }
}

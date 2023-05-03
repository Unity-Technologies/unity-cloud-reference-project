using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PageView))]
    class PageViewTests : VisualElementTests<PageView>
    {
        protected override string mainUssClassName => PageView.ussClassName;
    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PageIndicator))]
    class PageIndicatorTests : VisualElementTests<PageIndicator>
    {
        protected override string mainUssClassName => PageIndicator.ussClassName;
    }
}

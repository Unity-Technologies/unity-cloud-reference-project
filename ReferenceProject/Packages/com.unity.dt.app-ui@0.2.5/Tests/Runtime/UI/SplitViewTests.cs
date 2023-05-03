using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SplitView))]
    class SplitViewTests : VisualElementTests<SplitView>
    {
        protected override string mainUssClassName => SplitView.ussClassName;
    }
}

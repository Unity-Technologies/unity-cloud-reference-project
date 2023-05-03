using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SwipeView))]
    class SwipeViewTests : VisualElementTests<SwipeView>
    {
        protected override string mainUssClassName => SwipeView.ussClassName;
    }
}

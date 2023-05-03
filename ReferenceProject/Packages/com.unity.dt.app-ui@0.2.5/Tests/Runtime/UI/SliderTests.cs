using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SliderFloat))]
    class SliderFloatTests : VisualElementTests<SliderFloat>
    {
        protected override string mainUssClassName => SliderFloat.ussClassName;
    }

    [TestFixture]
    [TestOf(typeof(SliderInt))]
    class SliderIntTests : VisualElementTests<SliderInt>
    {
        protected override string mainUssClassName => SliderInt.ussClassName;
    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorSlider))]
    class ColorSliderTests : VisualElementTests<ColorSlider>
    {
        protected override string mainUssClassName => ColorSlider.ussClassName;
    }
}

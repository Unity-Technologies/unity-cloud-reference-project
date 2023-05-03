using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorSwatch))]
    class ColorSwatchTests : VisualElementTests<ColorSwatch>
    {
        protected override string mainUssClassName => ColorSwatch.ussClassName;
    }
}

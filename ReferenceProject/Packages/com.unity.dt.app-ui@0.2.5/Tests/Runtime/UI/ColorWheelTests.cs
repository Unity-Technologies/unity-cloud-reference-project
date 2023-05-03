using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorWheel))]
    class ColorWheelTests : VisualElementTests<ColorWheel>
    {
        protected override string mainUssClassName => ColorWheel.ussClassName;
    }
}

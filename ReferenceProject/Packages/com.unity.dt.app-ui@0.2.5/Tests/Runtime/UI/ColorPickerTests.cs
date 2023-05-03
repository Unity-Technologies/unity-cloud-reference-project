using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorPicker))]
    class ColorPickerTests : VisualElementTests<ColorPicker>
    {
        protected override string mainUssClassName => ColorPicker.ussClassName;
    }
}

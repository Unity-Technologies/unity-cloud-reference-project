using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorField))]
    class ColorFieldTests : VisualElementTests<ColorField>
    {
        protected override string mainUssClassName => ColorField.ussClassName;
    }
}

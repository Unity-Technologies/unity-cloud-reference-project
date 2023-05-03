using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorToolbar))]
    class ColorToolbarTests : VisualElementTests<ColorToolbar>
    {
        protected override string mainUssClassName => ColorToolbar.ussClassName;

        protected override bool uxmlConstructable => false;
    }
}

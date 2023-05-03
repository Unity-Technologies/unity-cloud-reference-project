using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(IconButton))]
    class IconButtonTests : VisualElementTests<IconButton>
    {
        protected override string mainUssClassName => IconButton.ussClassName;
    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Button))]
    class ButtonTests : VisualElementTests<Button>
    {
        protected override string mainUssClassName => Button.ussClassName;
    }
}

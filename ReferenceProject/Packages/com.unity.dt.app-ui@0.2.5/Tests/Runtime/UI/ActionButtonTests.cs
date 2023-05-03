using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionButton))]
    class ActionButtonTests : VisualElementTests<ActionButton>
    {
        protected override string mainUssClassName => ActionButton.ussClassName;
    }
}

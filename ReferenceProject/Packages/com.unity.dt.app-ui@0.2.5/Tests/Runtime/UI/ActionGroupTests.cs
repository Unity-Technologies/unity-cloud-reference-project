using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionGroup))]
    class ActionGroupTests : VisualElementTests<ActionGroup>
    {
        protected override string mainUssClassName => ActionGroup.ussClassName;
    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionBar))]
    class ActionBarTests : VisualElementTests<ActionBar>
    {
        protected override string mainUssClassName => ActionBar.ussClassName;
    }
}

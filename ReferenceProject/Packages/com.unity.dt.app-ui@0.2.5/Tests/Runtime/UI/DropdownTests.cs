using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dropdown))]
    class DropdownTests : VisualElementTests<Dropdown>
    {
        protected override string mainUssClassName => Dropdown.ussClassName;
    }
}

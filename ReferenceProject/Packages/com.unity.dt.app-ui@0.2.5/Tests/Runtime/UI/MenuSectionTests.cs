using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuSection))]
    class MenuSectionTests : VisualElementTests<MenuSection>
    {
        protected override string mainUssClassName => MenuSection.ussClassName;
    }
}

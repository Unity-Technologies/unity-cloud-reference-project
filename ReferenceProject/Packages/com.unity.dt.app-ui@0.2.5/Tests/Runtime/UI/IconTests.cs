using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Icon))]
    class IconTests : VisualElementTests<Icon>
    {
        protected override string mainUssClassName => Icon.ussClassName;

    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Toggle))]
    class ToggleTests : VisualElementTests<Toggle>
    {
        protected override string mainUssClassName => Toggle.ussClassName;
    }
}

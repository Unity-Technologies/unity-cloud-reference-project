using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RectField))]
    class RectFieldTests : VisualElementTests<RectField>
    {
        protected override string mainUssClassName => RectField.ussClassName;
    }
}

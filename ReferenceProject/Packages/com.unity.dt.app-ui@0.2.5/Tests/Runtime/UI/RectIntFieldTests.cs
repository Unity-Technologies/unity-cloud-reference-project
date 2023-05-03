using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RectIntField))]
    class RectIntFieldTests : VisualElementTests<RectIntField>
    {
        protected override string mainUssClassName => RectIntField.ussClassName;
    }
}

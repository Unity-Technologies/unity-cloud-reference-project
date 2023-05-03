using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsIntField))]
    class BoundsIntFieldTests : VisualElementTests<BoundsIntField>
    {
        protected override string mainUssClassName => BoundsIntField.ussClassName;
    }
}

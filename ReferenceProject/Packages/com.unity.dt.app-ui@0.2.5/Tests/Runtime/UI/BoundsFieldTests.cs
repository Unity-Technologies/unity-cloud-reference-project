using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsField))]
    class BoundsFieldTests : VisualElementTests<BoundsField>
    {
        protected override string mainUssClassName => BoundsField.ussClassName;
    }
}

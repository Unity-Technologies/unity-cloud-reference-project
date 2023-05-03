using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Mask))]
    class MaskTests : VisualElementTests<Mask>
    {
        protected override string mainUssClassName => Mask.ussClassName;
    }
}

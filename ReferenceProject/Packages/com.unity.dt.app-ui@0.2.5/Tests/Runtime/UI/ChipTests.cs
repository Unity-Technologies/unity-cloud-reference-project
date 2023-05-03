using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Chip))]
    class ChipTests : VisualElementTests<Chip>
    {
        protected override string mainUssClassName => Chip.ussClassName;
    }
}

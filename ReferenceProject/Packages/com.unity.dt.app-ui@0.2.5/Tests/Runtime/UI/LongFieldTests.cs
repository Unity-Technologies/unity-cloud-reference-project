using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(LongField))]
    class LongFieldTests : VisualElementTests<LongField>
    {
        protected override string mainUssClassName => LongField.ussClassName;
    }
}

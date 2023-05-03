using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DoubleField))]
    class DoubleFieldTests : VisualElementTests<DoubleField>
    {
        protected override string mainUssClassName => DoubleField.ussClassName;
    }
}

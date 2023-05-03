using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FloatField))]
    class FloatFieldTests : NumericalFieldTests<FloatField, float>
    {
        protected override string mainUssClassName => FloatField.ussClassName;
    }
}

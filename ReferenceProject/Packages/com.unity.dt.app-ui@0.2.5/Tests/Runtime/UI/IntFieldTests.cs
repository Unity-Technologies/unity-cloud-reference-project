using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(IntField))]
    class IntFieldTests : NumericalFieldTests<IntField, int>
    {
        protected override string mainUssClassName => IntField.ussClassName;
    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Vector3IntField))]
    class Vector3IntFieldTests : VisualElementTests<Vector3IntField>
    {
        protected override string mainUssClassName => Vector3IntField.ussClassName;
    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Vector2IntField))]
    class Vector2IntFieldTests : VisualElementTests<Vector2IntField>
    {
        protected override string mainUssClassName => Vector2IntField.ussClassName;
    }
}

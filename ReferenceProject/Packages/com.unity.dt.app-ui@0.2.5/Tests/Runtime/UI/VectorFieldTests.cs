using System;
using NUnit.Framework;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    class Vector2FieldTests : VectorFieldTests<UnityEngine.Dt.App.UI.Vector2Field, Vector2>
    {
        protected override string mainUssClassName => UnityEngine.Dt.App.UI.Vector2Field.ussClassName;
    }

    [TestFixture]
    class Vector3FieldTests : VectorFieldTests<UnityEngine.Dt.App.UI.Vector3Field, Vector3>
    {
        protected override string mainUssClassName => UnityEngine.Dt.App.UI.Vector3Field.ussClassName;
    }

    [TestFixture]
    class Vector4FieldTests : VectorFieldTests<UnityEngine.Dt.App.UI.Vector4Field, Vector4>
    {
        protected override string mainUssClassName => UnityEngine.Dt.App.UI.Vector4Field.ussClassName;
    }

    class VectorFieldTests<T, U> : VisualElementTests<T>
        where T : VisualElement, new()
        where U : struct, IEquatable<U>, IFormattable
    {

    }
}

using System;
using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TouchSliderFloat))]
    class TouchSliderFloatTests : TouchSliderTests<TouchSliderFloat, float>
    {
        protected override string mainUssClassName => TouchSliderFloat.ussClassName;
    }

    [TestFixture]
    [TestOf(typeof(TouchSliderInt))]
    class TouchSliderIntTests : TouchSliderTests<TouchSliderInt, int>
    {
        protected override string mainUssClassName => TouchSliderInt.ussClassName;
    }

    class TouchSliderTests<T, U> : VisualElementTests<T>
        where T : TouchSlider<U>, new()
        where U : IComparable, IEquatable<U>
    {

    }
}

using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Radio))]
    class RadioTests : VisualElementTests<Radio>
    {
        protected override string mainUssClassName => Radio.ussClassName;
    }

    [TestFixture]
    [TestOf(typeof(RadioGroup))]
    class RadioGroupTests : VisualElementTests<RadioGroup>
    {
        protected override string mainUssClassName => RadioGroup.ussClassName;
    }
}

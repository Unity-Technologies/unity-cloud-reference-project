using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Checkbox))]
    class CheckboxTests : VisualElementTests<Checkbox>
    {
        protected override string mainUssClassName => Checkbox.ussClassName;
    }
}

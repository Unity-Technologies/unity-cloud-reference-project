using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Stepper))]
    class StepperTests : VisualElementTests<Stepper>
    {
        protected override string mainUssClassName => Stepper.ussClassName;
    }
}

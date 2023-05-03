using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(StackView))]
    class StackViewTests : VisualElementTests<StackView>
    {
        protected override string mainUssClassName => StackView.ussClassName;
    }
}

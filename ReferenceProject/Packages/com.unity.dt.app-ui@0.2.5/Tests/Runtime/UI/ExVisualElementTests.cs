using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ExVisualElement))]
    class ExVisualElementTests : VisualElementTests<ExVisualElement>
    {
        protected override string mainUssClassName => null;
    }
}

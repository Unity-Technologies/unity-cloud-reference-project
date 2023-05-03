using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Text))]
    class TextTests : VisualElementTests<Text>
    {
        protected override string mainUssClassName => Text.ussClassName;
    }
}

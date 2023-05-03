using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextArea))]
    class TextAreaTests : VisualElementTests<TextArea>
    {
        protected override string mainUssClassName => TextArea.ussClassName;
    }
}

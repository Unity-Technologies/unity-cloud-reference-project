using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextField))]
    class TextFieldTests : VisualElementTests<TextField>
    {
        protected override string mainUssClassName => TextField.ussClassName;
    }
}

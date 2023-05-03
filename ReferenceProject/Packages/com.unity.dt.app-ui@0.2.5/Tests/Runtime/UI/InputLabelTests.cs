using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(InputLabel))]
    class InputLabelTests : VisualElementTests<InputLabel>
    {
        protected override string mainUssClassName => InputLabel.ussClassName;
    }
}

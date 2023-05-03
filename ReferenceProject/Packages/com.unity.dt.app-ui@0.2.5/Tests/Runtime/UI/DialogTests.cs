using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dialog))]
    class DialogTests : VisualElementTests<Dialog>
    {
        protected override string mainUssClassName => BaseDialog.ussClassName;
    }
}

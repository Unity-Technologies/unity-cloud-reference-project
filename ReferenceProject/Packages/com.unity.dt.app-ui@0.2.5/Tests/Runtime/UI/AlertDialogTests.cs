using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AlertDialog))]
    class AlertDialogTests : VisualElementTests<AlertDialog>
    {
        protected override string mainUssClassName => BaseDialog.ussClassName;
    }
}

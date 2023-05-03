using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(DialogTrigger))]
    class DialogTriggerTests : VisualElementTests<DialogTrigger>
    {
        protected override string mainUssClassName => null;
    }
}

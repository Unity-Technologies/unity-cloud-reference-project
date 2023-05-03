using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(LinearProgress))]
    class LinearProgressTests : VisualElementTests<LinearProgress>
    {
        protected override string mainUssClassName => LinearProgress.ussClassName;
    }
}

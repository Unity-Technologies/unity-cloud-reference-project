using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CircularProgress))]
    class CircularProgressTests : VisualElementTests<CircularProgress>
    {
        protected override string mainUssClassName => CircularProgress.ussClassName;
    }
}

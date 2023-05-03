using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SVSquare))]
    class SVSquareTests : VisualElementTests<SVSquare>
    {
        protected override string mainUssClassName => SVSquare.ussClassName;
    }
}

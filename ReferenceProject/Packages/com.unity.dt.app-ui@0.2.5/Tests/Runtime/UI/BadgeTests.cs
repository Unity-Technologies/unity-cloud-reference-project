using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Badge))]
    class BadgeTests : VisualElementTests<Badge>
    {
        protected override string mainUssClassName => Badge.ussClassName;
    }
}

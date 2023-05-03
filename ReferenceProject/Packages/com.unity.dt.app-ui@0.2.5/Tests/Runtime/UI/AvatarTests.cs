using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(App.UI.Avatar))]
    class AvatarTests : VisualElementTests<App.UI.Avatar>
    {
        protected override string mainUssClassName => App.UI.Avatar.ussClassName;
    }
}

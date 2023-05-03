using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(App.UI.Panel))]
    class ApplicationTests : VisualElementTests<App.UI.Panel>
    {
        protected override string mainUssClassName => ContextProvider.ussClassName;
    }
}

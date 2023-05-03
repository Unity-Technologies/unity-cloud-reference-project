using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ContextProvider))]
    class ContextProviderTests : VisualElementTests<ContextProvider>
    {
        protected override string mainUssClassName => ContextProvider.ussClassName;
    }
}

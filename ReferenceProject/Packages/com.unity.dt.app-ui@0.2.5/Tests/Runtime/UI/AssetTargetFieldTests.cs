using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AssetTargetField))]
    class AssetTargetFieldTests : VisualElementTests<AssetTargetField>
    {
        protected override string mainUssClassName => AssetTargetField.ussClassName;
    }
}

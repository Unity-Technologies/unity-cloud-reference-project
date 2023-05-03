using NUnit.Framework;
using UnityEngine.Dt.App.UI;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    class KeyCodeExtensionsTests
    {
        [Test]
        [TestCase(KeyCode.A, false)]
        [TestCase(KeyCode.KeypadEnter, true)]
        [TestCase(KeyCode.Return, true)]
        [TestCase(KeyCode.Space, true)]
        [TestCase(KeyCode.Backspace, false)]
        [TestCase(KeyCode.LeftControl, false)]
        public void KeyCodeExtensions_IsSubmitType_ShouldReturnCorrectValues(KeyCode key, bool expected)
        {
            Assert.AreEqual(expected, key.IsSubmitType());
        }
    }
}

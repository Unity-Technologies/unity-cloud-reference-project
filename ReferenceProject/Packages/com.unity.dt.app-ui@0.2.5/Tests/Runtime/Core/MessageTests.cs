using NUnit.Framework;
using UnityEngine.Dt.App.Core;

namespace UnityEngine.Dt.App.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(Message))]
    class MessageTests
    {
        [Test]
        [TestCase(1, 0, null)]
        [TestCase(2, 1, "")]
        [TestCase(3, 2, "a text")]
        [TestCase(3, 2, 42)]
        public void Message_Obtain_ShouldSetPropertiesProperly(int id, int arg1, object obj)
        {
            var msg = Message.Obtain(null, id, arg1, obj);
            Assert.AreEqual(id, msg.what);
            Assert.AreEqual(arg1, msg.arg1);
            Assert.AreEqual(obj, msg.obj);
            Assert.IsNull(msg.target);
            Assert.DoesNotThrow(msg.Recycle);
        }
    }
}

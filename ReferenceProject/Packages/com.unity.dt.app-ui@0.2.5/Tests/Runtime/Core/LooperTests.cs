using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.Dt.App.Core;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(Looper))]
    class LooperTests
    {
        UIDocument m_TestUI;

        Looper m_Looper;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_TestUI = Utils.ConstructTestUI();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            m_Looper?.Quit();
            m_Looper = null;

            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);
            m_TestUI = null;
        }

        [Test, Order(1)]
        public void Looper_Constructor_ShouldSucceed()
        {
            Assert.DoesNotThrow(() => m_Looper = new Looper(m_TestUI.rootVisualElement));
            Assert.IsNotNull(m_Looper);
        }

        [Test, Order(1)]
        public void Looper_Constructor_WithInvalidArgs_ShouldThrow()
        {
            Looper looper = null;
            Assert.Throws<ArgumentNullException>(() => looper = new Looper(null));
            Assert.IsNull(looper);
        }

        [UnityTest, Order(2)]
        public IEnumerator Looper_CanExecuteLoop()
        {
            Assert.IsNotNull(m_Looper);
            Assert.DoesNotThrow(m_Looper.Loop);

            var myMessage = 12345;
            var handled = false;
            var handler = new Handler(m_Looper, message =>
            {
                if (message.what == myMessage)
                {
                    handled = true;
                    return true;
                }
                return false;
            });

            var msg = handler.ObtainMessage(myMessage, null);

            Assert.AreEqual(0, m_Looper.queue.Count);

            handler.SendMessage(msg);

            yield return new WaitUntilOrTimeOut(() => handled);

            Assert.IsTrue(handled);

            msg = handler.ObtainMessage(myMessage, null);

            handled = false;
            handler.SendMessage(msg);
            Assert.DoesNotThrow(m_Looper.SafelyQuit);
            yield return new WaitUntilOrTimeOut(() => handled);
            Assert.IsTrue(handled);

            Assert.DoesNotThrow(m_Looper.Loop);
            handled = false;
            handler.SendMessage(msg);
            Assert.DoesNotThrow(m_Looper.Quit);
            yield return new WaitUntilOrTimeOut(() => handled, false, TimeSpan.FromSeconds(1));
            Assert.IsFalse(handled);
        }
    }
}

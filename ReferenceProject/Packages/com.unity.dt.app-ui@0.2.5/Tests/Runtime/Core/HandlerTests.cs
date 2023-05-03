using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.Dt.App.Core;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(Handler))]
    class HandlerTests
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

            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
        }

        [Test, Order(1)]
        public void Handler_Dependencies_CanBeCreated()
        {
            Assert.DoesNotThrow(() => m_Looper = new Looper(m_TestUI.rootVisualElement));
            Assert.IsNotNull(m_Looper);
            m_Looper.Loop();
        }

        [Test, Order(2)]
        public void Handler_Constructor_DoesNotThrow()
        {
            Handler handler = null;
            Assert.DoesNotThrow(() => handler = new Handler(m_Looper, Callback));
            Assert.IsNotNull(handler);
        }

        [Test, Order(2)]
        public void Handler_Constructor_ThrowsWithNullArgs()
        {
            Handler handler = null;
            Assert.Throws<ArgumentNullException>(() => handler = new Handler(m_Looper, null));
            Assert.Throws<ArgumentNullException>(() => handler = new Handler(null, Callback));
            Assert.IsNull(handler);
        }

        [UnityTest, Order(3)]
        public IEnumerator Handler_SendMessage_CanHandleReceivedMessage([Values(1, 2, 1234)] int messageId)
        {
            var handled = false;
            var handler = new Handler(m_Looper, message =>
            {
                if (message.what == messageId)
                {
                    handled = true;
                    return true;
                }
                return false;
            });
            handler.SendMessage(handler.ObtainMessage(messageId, null));

            yield return new WaitUntilOrTimeOut(() => handled);

            Assert.IsTrue(handled);
        }

        [UnityTest, Order(3)]
        public IEnumerator Handler_SendMessageDelayed_CanHandleReceivedMessage([Values(1, 2, 1234)] int messageId, [Values(16, 100)] int delay)
        {
            var handled = false;
            var handler = new Handler(m_Looper, message =>
            {
                if (message.what == messageId)
                {
                    handled = true;
                    return true;
                }
                return false;
            });
            handler.SendMessageDelayed(handler.ObtainMessage(messageId, null), delay);

            yield return new WaitUntilOrTimeOut(() => handled);

            Assert.IsTrue(handled);
        }

        bool Callback(Message arg)
        {
            return false;
        }
    }
}

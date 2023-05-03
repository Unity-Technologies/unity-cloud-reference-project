using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.Dt.App.Core;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(NotificationManager))]
    class NotificationManagerTests
    {
        UIDocument m_TestUI;

        UnityEngine.Dt.App.UI.Panel m_Panel;

        NotificationManager m_Manager;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_TestUI = Utils.ConstructTestUI();
            m_Panel = new UnityEngine.Dt.App.UI.Panel();
            m_TestUI.rootVisualElement.Add(m_Panel);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
        }

        [Test, Order(1)]
        public void NotificationManager_Constructor_ShouldSucceed()
        {
            Assert.IsNull(m_Manager);
            Assert.Throws<ArgumentNullException>(() => m_Manager = new NotificationManager(null));
            Assert.DoesNotThrow(() => m_Manager = new NotificationManager(AppUI.s_Manager));
            Assert.IsNotNull(m_Manager);
        }

        [UnityTest, Order(2)]
        public IEnumerator NotificationManager_Show_ShouldInvokeCallbacks()
        {
            var myCallback = new Callback(m_Manager);
            // Request the manager to queue the display of a new notification
            m_Manager.Show(NotificationDuration.Long, myCallback);
            yield return new WaitUntilOrTimeOut(() => myCallback.showCalled);
            Assert.IsTrue(myCallback.showCalled);
            Assert.IsFalse(myCallback.dismissCalled);
            // Wait for the timeout
            yield return new WaitUntilOrTimeOut(() => myCallback.dismissCalled, true,
                TimeSpan.FromMilliseconds((int)NotificationDuration.Long + 1000));
            Assert.IsTrue(myCallback.dismissCalled);
            Assert.AreEqual(DismissType.Timeout, myCallback.dismissReason);
            // Request again twice to trigger the consecutive dismiss type
            var cb1 = new Callback(m_Manager);
            var cb2 = new Callback(m_Manager);
            m_Manager.Show(NotificationDuration.Indefinite, cb1);
            yield return new WaitUntilOrTimeOut(() => cb1.showCalled);
            Assert.IsTrue(cb1.showCalled);
            Assert.IsFalse(cb1.dismissCalled);
            m_Manager.Show(NotificationDuration.Indefinite, cb2);
            yield return new WaitUntilOrTimeOut(() => cb2.showCalled);
            Assert.IsTrue(cb2.showCalled);
            Assert.IsTrue(cb1.dismissCalled);
            Assert.AreEqual(DismissType.Consecutive, cb1.dismissReason);
            yield return new WaitUntilOrTimeOut(() => cb2.dismissCalled, false);
            Assert.IsFalse(cb2.dismissCalled);
        }

        class Callback : NotificationManager.ICallback
        {
            public object obj { get; } = null;

            readonly NotificationManager m_Manager;

            public bool showCalled { get; private set; }

            public bool dismissCalled { get; private set; }

            public DismissType dismissReason { get; private set; }

            public void Show()
            {
                showCalled = true;
                m_Manager.OnShown(this); // it is mandatory to give feedbacks to the manager
            }

            public void Dismiss(DismissType reason)
            {
                dismissReason = reason;
                dismissCalled = true;
                m_Manager.OnDismissed(this); // it is mandatory to give feedbacks to the manager
            }

            public Callback(NotificationManager manager)
            {
                m_Manager = manager;
            }
        }
    }
}

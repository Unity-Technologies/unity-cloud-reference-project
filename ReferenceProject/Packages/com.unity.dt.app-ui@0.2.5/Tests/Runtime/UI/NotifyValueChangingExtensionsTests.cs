using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.Dt.App.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Tests.UI
{
    [TestFixture]
    class NotifyValueChangingExtensionsTests
    {
        class DummyElement : VisualElement, INotifyValueChanging<int>
        {
            int m_Value;

            public void SetValueWithoutNotify(int newValue)
            {
                using var evt = ChangingEvent<int>.GetPooled();
                evt.target = this;
                evt.previousValue = m_Value;
                m_Value = newValue;
                evt.newValue = newValue;
                SendEvent(evt);
            }

            public int value
            {
                get => m_Value;
                set
                {
                    if (m_Value == value)
                        return;
                    using var evt = ChangeEvent<int>.GetPooled(m_Value, value);
                    evt.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(evt);
                }
            }
        }

        class InvalidDummyType : INotifyValueChanging<int>
        {
            public void SetValueWithoutNotify(int newValue)
            {
            }

            public int value { get; set; }
        }

        bool m_Called;

        DummyElement m_Element;

        void Callback(ChangingEvent<int> evt) => m_Called = true;

        UIDocument m_TestUI;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            m_TestUI = Utils.ConstructTestUI();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
        }

        [Test]
        public void NotifyValueChangingExtensions_RegisterValueChangingCallback_ShouldReturnFalseWithInvalidArg()
        {
            var invalidElement = new InvalidDummyType();
            Assert.IsFalse(invalidElement.RegisterValueChangingCallback(evt => { }));
        }

        [UnityTest]
        public IEnumerator NotifyValueChangingExtensions_RegisterValueChangingCallback_ShouldRegisterCallback()
        {
            m_Element = new DummyElement();

            m_TestUI.rootVisualElement.Add(m_Element);

            yield return null;

            Assert.IsTrue(m_Element.RegisterValueChangingCallback(Callback));

            m_Called = false;

            m_Element.value = 123;

            yield return new WaitUntilOrTimeOut(() => m_Called, false);

            Assert.IsTrue(m_Called);

            m_Called = false;

            m_Element.SetValueWithoutNotify(1234);

            yield return new WaitUntilOrTimeOut(() => m_Called, false);

            Assert.IsTrue(m_Called);
        }

        [Test]
        public void NotifyValueChangingExtensions_UnregisterValueChangingCallback_ShouldReturnFalseWithInvalidArg()
        {
            var invalidElement = new InvalidDummyType();
            Assert.IsFalse(invalidElement.UnregisterValueChangingCallback(evt => { }));
        }

        [UnityTest]
        public IEnumerator NotifyValueChangingExtensions_UnregisterValueChangingCallback_ShouldUnregisterCallback()
        {
            yield return NotifyValueChangingExtensions_RegisterValueChangingCallback_ShouldRegisterCallback();

            m_Called = false;

            Assert.IsTrue(m_Element.UnregisterValueChangingCallback(Callback));

            m_Element.value = 456;

            yield return new WaitUntilOrTimeOut(() => m_Called, false, TimeSpan.FromSeconds(1));

            Assert.IsFalse(m_Called);

            m_Called = false;

            m_Element.SetValueWithoutNotify(4567);

            yield return new WaitUntilOrTimeOut(() => m_Called, false, TimeSpan.FromSeconds(1));

            Assert.IsFalse(m_Called);
        }
    }
}

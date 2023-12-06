using NUnit.Framework;
using NUnit.Framework.Internal;
using Unity.ReferenceProject.DataStores;
using Unity.Properties;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests.Runtime
{
    public class DataStoreTests
    {
        interface ITestInterface { }

        class TestClass : ITestInterface { }
        
        [GeneratePropertyBag]
        struct TestData
        {
            [CreateProperty]
            public int Number { get; set; }
            [CreateProperty]
            public TestClass Reference { get; set; }
            [CreateProperty]
            public ITestInterface InterfaceReference { get; set; }
        }

        class TestDataStore : DataStore<TestData> { }

        [Test]
        public void GetProperty()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();

            //Act
            var property = dataStore.GetProperty<int>(nameof(TestData.Number));

            //Assert
            Assert.IsNotNull(property);

            //Clean up
            Object.Destroy(go);
        }

        [Test]
        public void GetPropertyReturnsNullIfWrongPropertyTypeIsSpecified()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();

            //Act
            var property = dataStore.GetProperty<float>(nameof(TestData.Number));

            //Assert
            LogAssert.Expect(LogType.Warning, $"Property type does not match! {typeof(float)} != {typeof(int)}");
            Assert.IsNull(property);

            //Clean up
            Object.Destroy(go);
        }

        [Test]
        public void GetPropertyReturnsNullIfWrongPropertyNameIsSpecified()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();

            //Act
            var propertyName = "DoesNotExist";
            var property = dataStore.GetProperty<int>(propertyName);

            //Assert
            LogAssert.Expect(LogType.Warning, $"Failed to get property '{propertyName}': {VisitReturnCode.InvalidPath}");
            Assert.IsNull(property);

            //Clean up
            Object.Destroy(go);
        }

        [Test]
        public void SetAndGetPropertyValue()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();
            var property = dataStore.GetProperty<int>(nameof(TestData.Number));

            //Act
            var set = property.SetValue(1);
            var get = property.GetValue();

            //Assert
            Assert.IsTrue(set);
            Assert.AreEqual(1, get);
        }
        
        [Test]
        public void SetReferencePropertyValueToNull()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();
            var property = dataStore.GetProperty<TestClass>(nameof(TestData.Reference));
            property.SetValue(new TestClass());

            //Act
            var set = property.SetValue(default(TestClass));
            var get = property.GetValue();

            //Assert
            Assert.IsTrue(set);
            Assert.AreEqual(null, get);
        }
        
        [Test]
        public void SetInterfacePropertyValueToNull()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();
            var property = dataStore.GetProperty<ITestInterface>(nameof(TestData.InterfaceReference));
            property.SetValue(new TestClass());

            //Act
            var set = property.SetValue(default(ITestInterface));
            var get = property.GetValue();

            //Assert
            Assert.IsTrue(set);
            Assert.AreEqual(null, get);
        }

        [Test]
        public void SetPropertyValueWithAction()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();
            var property = dataStore.GetProperty<int>(nameof(TestData.Number));

            //Act
            var set = property.SetValue((ref int val) => { val++; });
            var get = property.GetValue();

            //Assert
            Assert.IsTrue(set);
            Assert.AreEqual(1, get);
        }

        [Test]
        public void ForceNotifyTriggersProperties()
        {
            //Arrange
            var go = new GameObject();
            var dataStore = go.AddComponent<TestDataStore>();
            var property = dataStore.GetProperty<int>(nameof(TestData.Number));
            var called = 0;
            property.ValueChanged += _ => { called++; };

            //Act
            property.SetValue(1, UpdateNotification.DoNotNotify);
            property.SetValue(2, UpdateNotification.DoNotNotify);
            property.SetValue(3, UpdateNotification.DoNotNotify);
            dataStore.ForceNotify();

            //Assert
            Assert.AreEqual(1, called);
        }
    }
}

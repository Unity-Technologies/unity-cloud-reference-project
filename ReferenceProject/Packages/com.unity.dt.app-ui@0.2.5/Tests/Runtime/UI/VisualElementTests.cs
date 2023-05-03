using System.Collections;
using NUnit.Framework;
using UnityEngine.Dt.App.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEngine.Dt.App.Tests.UI
{
    class VisualElementTests<T>
        where T : VisualElement, new()
    {
        VisualElement m_VisualElement;

        protected virtual string mainUssClassName => null;

        protected virtual bool uxmlConstructable => true;

        protected T element => m_VisualElement as T;

        protected UIDocument m_TestUI;

        protected Panel m_Panel;

        protected bool m_SetupDone;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!m_SetupDone)
            {
                // Load new scene
                var scene = SceneManager.CreateScene("ComponentTestScene-" + Random.Range(1, 1000000));
                while (!SceneManager.SetActiveScene(scene))
                {
                    yield return null;
                }
                m_TestUI = Utils.ConstructTestUI();
            }
            m_TestUI.rootVisualElement.Clear();
            m_SetupDone = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
            m_SetupDone = false;
#pragma warning disable CS0618
            SceneManager.UnloadScene(SceneManager.GetActiveScene());
#pragma warning restore CS0618
        }

        [Test]
        [Order(1)]
        public void Constructor_ShouldSucceed()
        {
            m_VisualElement = null;
            Assert.DoesNotThrow(() => m_VisualElement = new T());
            Assert.IsNotNull(m_VisualElement);
            if (!string.IsNullOrEmpty(mainUssClassName))
                Assert.IsTrue(m_VisualElement.ClassListContains(mainUssClassName));
        }

        [Test]
        [Order(2)]
        public void UxmlConstruction_ShouldSucceed()
        {
            if (!uxmlConstructable || !Application.isEditor)
            {
                // skip test and mark as ignored
                Assert.Ignore("UXML construction not supported for this type");
                return;
            }

            Assert.DoesNotThrow(() =>
            {
                var msg = "<appui:" + typeof(T).Name + " />";
                var content = "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:appui=\"UnityEngine.Dt.App.UI\" >" + msg + "</ui:UXML>";
                var asset = Utils.LoadUxmlTemplateFromString(content);
                m_TestUI.visualTreeAsset = asset;
            });
        }
    }
}

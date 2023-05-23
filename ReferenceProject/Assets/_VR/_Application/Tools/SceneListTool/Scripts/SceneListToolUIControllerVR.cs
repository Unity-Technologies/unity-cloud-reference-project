using System;
using Unity.Cloud.Common;
using Unity.ReferenceProject.AccessHistory;
using Unity.ReferenceProject.DataStreaming;
using Unity.ReferenceProject.ScenesList;
using Unity.ReferenceProject.DataStores;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.VR
{
    public class SceneListToolUIControllerVR : SceneListToolUIController
    {
        [SerializeField]
        InputSingleUnifiedPointer m_SingleUnifiedPointer;

        IDataStreamController m_DataStreamController;

        PropertyValue<IScene> m_ScenePropertyValue;

        [Inject]
        void Setup(PropertyValue<IScene> scenePropertyValue, IDataStreamController dataStreamController)
        {
            m_ScenePropertyValue = scenePropertyValue;
            m_DataStreamController = dataStreamController;
        }

        protected override void Awake()
        {
            base.Awake();
            SceneListUIController.ProjectSelected += OnSceneSelected;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var visualElement = base.CreateVisualTree(template);
            SceneListUIController.InitUIToolkit(visualElement);
            return visualElement;
        }

        public override void OnToolOpened()
        {
            SceneListUIController.Refresh();
            m_SingleUnifiedPointer.Enable();
        }

        public override void OnToolClosed()
        {
            m_SingleUnifiedPointer.Disable();
        }

        void OnSceneSelected(IScene scene)
        {
            CloseSelf();

            if (scene != m_ScenePropertyValue.GetValue())
            {
                m_ScenePropertyValue.SetValue(scene);
                m_DataStreamController.Open(scene);
            }
        }
    }
}

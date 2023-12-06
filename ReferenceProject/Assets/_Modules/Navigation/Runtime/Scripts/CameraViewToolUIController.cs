using System;
using System.Collections.Generic;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Navigation
{
    public class CameraViewToolUIController : ToolUIController
    {
        [SerializeField]
        VisualTreeAsset m_ButtonTemplate;
        
        INavigationManager m_NavigationManager;

        [Inject]
        void Setup(INavigationManager navigationManager)
        {
            m_NavigationManager = navigationManager;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);
            root.AddToClassList("panel-horizontal-display");
            var cameraViewData = m_NavigationManager.CameraViewModeData;
            
            for (var i = 0; i < cameraViewData.Length; i++)
            {
                if (cameraViewData[i] == null)
                    continue;
                
                var button = SetupVisualElement(cameraViewData[i]);

                if (i == 0)
                {
                    button.style.borderTopLeftRadius = button.style.borderTopRightRadius = 4;
                }
                else if (i == cameraViewData.Length - 1)
                {
                    button.style.borderBottomLeftRadius = button.style.borderBottomRightRadius = 4;
                }
                
                root.Add(button);
            }
            return root;
        }

        VisualElement SetupVisualElement(CameraViewData cameraViewData)
        {
            var buttonTemplate = m_ButtonTemplate.CloneTree();
            buttonTemplate.tooltip = cameraViewData.ModeName;
            
            var actionButton = buttonTemplate.Q<ActionButton>("ActionButton");
            actionButton.icon = cameraViewData.Icon.name;
            actionButton.clickable.clicked += () => OnModeClick(cameraViewData);
            actionButton.focusable = false;

            return buttonTemplate;
        }
        
        void OnModeClick(CameraViewData cameraViewData)
        {
            m_NavigationManager.ChangeCameraView(cameraViewData);
        }
    }
}

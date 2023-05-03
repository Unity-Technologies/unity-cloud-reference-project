using Unity.ReferenceProject.UIPanel;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject
{
    public class MainUIPanelInstaller : MonoInstaller
    {
        [SerializeField]
        PanelSettings m_PanelSettings;

        public override void InstallBindings()
        {
            Container.Bind<MainUIPanel>().FromInstance(MainUIPanel.CreateInstance(m_PanelSettings)).AsSingle();
        }
    }
}

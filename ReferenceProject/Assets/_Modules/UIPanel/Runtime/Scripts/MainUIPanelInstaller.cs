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
            Container.Bind<IMainUIPanel>().FromInstance(new MainUIPanel(m_PanelSettings)).AsSingle();
        }
    }
}

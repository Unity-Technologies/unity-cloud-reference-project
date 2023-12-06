using System;
using Unity.ReferenceProject.Tools;
using Zenject;

namespace Unity.ReferenceProject
{
    public class ToolUIManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var toolUIManager = gameObject.AddComponent<ToolUIManager>();

            Container.Bind<IToolUIManager>().FromInstance(toolUIManager);
        }
    }
}

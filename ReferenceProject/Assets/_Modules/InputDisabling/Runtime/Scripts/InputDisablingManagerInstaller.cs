using System;
using Unity.ReferenceProject.InputDisabling;
using Zenject;

namespace Unity.ReferenceProject
{
    public class InputDisablingManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var inputDisablingManager = gameObject.AddComponent<InputDisablingManager>();

            Container.Bind<IInputDisablingManager>().FromInstance(inputDisablingManager).AsSingle();
        }
    }
}

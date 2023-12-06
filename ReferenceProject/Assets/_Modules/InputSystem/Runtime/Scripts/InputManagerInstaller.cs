using Unity.ReferenceProject.InputSystem.VR;
using Zenject;

namespace Unity.ReferenceProject.InputSystem
{
    public class InputManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IInputManager>().FromInstance(GetComponent<InputManager>()).AsSingle();
            Container.Bind<IVRControllerList>().FromInstance(null).AsSingle();
        }
    }
}
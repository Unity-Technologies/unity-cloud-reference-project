using Unity.ReferenceProject.CustomKeyboard;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject
{
    public class KeyboardInstaller : MonoInstaller
    {
        [SerializeField]
        KeyboardLayout m_KeyboardLayout;

        [SerializeField]
        StyleSheet m_KeyboardStyle;

        public override void InstallBindings()
        {
            var keyboard = new KeyboardController()
            {
                Layout = m_KeyboardLayout,
                Style = m_KeyboardStyle
            };

            Container.Bind<IKeyboardController>().FromInstance(keyboard).AsSingle();

            // Necessary for VR keyboard
            Container.Bind<Mouse>().FromInstance(null).AsSingle();
        }
    }
}

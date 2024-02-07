using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using UnityInputSystem = UnityEngine.InputSystem.InputSystem;

namespace Unity.ReferenceProject
{
    public class CameraInstaller : MonoInstaller
    {
        [SerializeField]
        Camera m_Camera;

        public override void InstallBindings()
        {
            if (m_Camera)
            {
                Container.Bind<ICameraProvider>().FromInstance(new CameraProvider(m_Camera)).AsSingle();
            }
            
            // Necessary for Gamepad UI
            try
            {
                var gamepad = UnityInputSystem.AddDevice<Gamepad>();
                Container.Bind<Gamepad>().FromInstance(gamepad).AsSingle();
            }
            catch (InvalidOperationException e)
            {
                Container.Bind<Gamepad>().FromInstance(null).AsSingle();
                Debug.LogError(e);
            }
        }
    }
}

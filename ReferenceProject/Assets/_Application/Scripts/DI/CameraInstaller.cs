using System;
using Unity.ReferenceProject.Common;
using UnityEngine;
using Zenject;

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
        }
    }
}

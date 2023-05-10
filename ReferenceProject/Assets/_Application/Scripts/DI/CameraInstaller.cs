using System;
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
                Container.Bind<Camera>().FromInstance(m_Camera).AsSingle();
        }
    }
}

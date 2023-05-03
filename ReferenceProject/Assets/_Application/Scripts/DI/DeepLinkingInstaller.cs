﻿using System;
using Unity.ReferenceProject.DeepLinking;
using Zenject;

namespace Unity.ReferenceProject
{
    public class DeepLinkingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IDeepLinkingController>().To<DeepLinkingController>().AsSingle();
        }
    }
}

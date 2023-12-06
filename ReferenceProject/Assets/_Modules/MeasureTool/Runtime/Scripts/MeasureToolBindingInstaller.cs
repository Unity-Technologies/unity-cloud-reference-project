using System;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.MeasureTool
{
    public class MeasureToolBindingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var dataStore = gameObject.AddComponent<MeasureToolDataStore>();
            Container.Bind<MeasureToolDataStore>().FromInstance(dataStore).AsSingle();

            var persistence = new MeasureLinePersistence($"{UnityEngine.Application.persistentDataPath}/api");
            Container.Bind<MeasureLinePersistence>().FromInstance(persistence);
        }
    }
}

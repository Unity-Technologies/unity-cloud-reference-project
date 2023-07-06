using Unity.ReferenceProject.Annotation;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public class AnnotationInstaller : MonoInstaller
    {
        [SerializeField]
        AnnotationIndicatorController m_AnnotationIndicatorPrefab;

        public override void InstallBindings()
        {
            Container.Bind<IAnnotationController>().To<AnnotationController>().AsSingle();
            var annotationIndicatorManager = new AnnotationIndicatorManager(m_AnnotationIndicatorPrefab);
            annotationIndicatorManager.Setup(Container);
            Container.Bind<IAnnotationIndicatorManager>().FromInstance(annotationIndicatorManager).AsSingle();
        }
    }
}

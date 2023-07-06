using System;
using System.Collections.Generic;
using Unity.Cloud.Annotation;
using UnityEngine.Pool;
using Zenject;
using Object = UnityEngine.Object;

namespace Unity.ReferenceProject.Annotation
{
    public interface IAnnotationIndicatorManager
    {
        void SetIndicators(IEnumerable<ITopic> topics);
        void ClearIndicators();
        void SetIndicatorsVisible(bool visible=true);
        void AddIndicator(ITopic topic);
        void RemoveIndicator(Guid topicId);
        AnnotationIndicatorController GetIndicator(Guid topicId);
    }

    public class AnnotationIndicatorManager : IAnnotationIndicatorManager
    {
        readonly AnnotationIndicatorController m_AnnotationIndicatorPrefab;
        readonly ObjectPool<AnnotationIndicatorController> m_Pool;
        readonly Dictionary<Guid, AnnotationIndicatorController> m_ActiveIndicators = new ();

        DiContainer m_DiContainer;
        bool m_IsShowing;

        [Inject]
        public void Setup(DiContainer diContainer)
        {
            m_DiContainer = diContainer;
        }

        public AnnotationIndicatorManager(AnnotationIndicatorController annotationIndicatorPrefab)
        {
            m_AnnotationIndicatorPrefab = annotationIndicatorPrefab;
            m_Pool = new ObjectPool<AnnotationIndicatorController>(CreateIndicator, ResetIndicator, ReleaseIndicator, DestroyIndicator);
        }

        public void SetIndicators(IEnumerable<ITopic> topics)
        {
            ClearIndicators();
            foreach (var topic in topics)
            {
                var indicator = m_Pool.Get();
                indicator.Initialize(topic);
                m_ActiveIndicators.Add(topic.Id, indicator);
            }
        }

        public void ClearIndicators()
        {
            foreach (var indicator in m_ActiveIndicators)
            {
                m_Pool.Release(indicator.Value);
            }
            m_ActiveIndicators.Clear();
        }

        public void SetIndicatorsVisible(bool visible=true)
        {
            m_IsShowing = visible;
            foreach (var indicator in m_ActiveIndicators)
            {
                indicator.Value.gameObject.SetActive(visible);
            }
        }

        public void AddIndicator(ITopic topic)
        {
            var indicator = m_Pool.Get();
            indicator.Initialize(topic);
            m_ActiveIndicators.Add(topic.Id, indicator);
        }

        public void RemoveIndicator(Guid topicId)
        {
            if (m_ActiveIndicators.TryGetValue(topicId, out var indicator))
            {
                m_Pool.Release(indicator);
                m_ActiveIndicators.Remove(topicId);
            }
        }

        public AnnotationIndicatorController GetIndicator(Guid topicId)
        {
            if (m_ActiveIndicators.TryGetValue(topicId, out var indicator))
            {
                return indicator;
            }
            return null;
        }

        AnnotationIndicatorController CreateIndicator()
        {
            return m_DiContainer.InstantiatePrefabForComponent<AnnotationIndicatorController>(m_AnnotationIndicatorPrefab);
        }

        void ResetIndicator(AnnotationIndicatorController indicator)
        {
            indicator.Reset();
            indicator.gameObject.SetActive(m_IsShowing);
        }

        void DestroyIndicator(AnnotationIndicatorController indicator)
        {
            Object.Destroy(indicator.gameObject);
        }

        void ReleaseIndicator(AnnotationIndicatorController indicator)
        {
            indicator.Reset();
            indicator.gameObject.SetActive(false);
        }
    }
}

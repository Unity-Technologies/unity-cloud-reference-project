using System;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Stats
{
    public abstract class BaseStat : MonoBehaviour, IStat
    {
        bool m_IsDisplayed;

        public abstract VisualElement CreateVisualTree();

        protected abstract void OnStatUpdate();

        IGlobalStats m_GlobalStats;

        [Inject]
        void Setup(IGlobalStats stats)
        {
            m_GlobalStats = stats;
        }
        
        protected virtual void OnEnable()
        {
            m_GlobalStats.AddStat(this);
        }

        protected virtual void OnDisable()
        {
            m_GlobalStats.RemoveStat(this);
        }
        
        public virtual void OnStatDisplayed()
        {
            m_IsDisplayed = true;
        }

        public virtual void OnStatHidden()
        {
            m_IsDisplayed = false;
        }
        
        protected virtual void Update()
        {
            if (m_IsDisplayed)
            {
                OnStatUpdate();
            }
        }
    }
}
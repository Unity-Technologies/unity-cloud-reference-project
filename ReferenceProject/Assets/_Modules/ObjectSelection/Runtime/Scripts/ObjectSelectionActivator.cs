using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.ObjectSelection
{
    public class ObjectSelectionActivator
    {
        readonly HashSet<MonoBehaviour> m_Listeners = new();

        public bool IsActive
        {
            get
            {
                RemoveMissingReferences();
                if (m_Listeners.Count == 0)
                    DispatchEvent();
                return m_Listeners.Count > 0;
            }
        }

        public event Action<bool> OnActivate;

        public void Subscribe(MonoBehaviour listener)
        {
            RemoveMissingReferences();
            m_Listeners.Add(listener);

            DispatchEvent();
        }

        public void Unsubscribe(MonoBehaviour listener)
        {
            RemoveMissingReferences();
            m_Listeners.Remove(listener);

            DispatchEvent();
        }

        void RemoveMissingReferences()
        {
            m_Listeners.RemoveWhere(x => x == null);
        }

        void DispatchEvent()
        {
            OnActivate?.Invoke(m_Listeners.Count > 0);
        }
    }
}

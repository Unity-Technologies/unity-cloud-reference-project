using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.ReferenceProject.InputDisabling
{
    public interface IInputDisablingManager
    {
        void AddSubscriber(IInputDisablingSubscriber subscriber);

        void RemoveSubscriber(IInputDisablingSubscriber subscriber);

        void AddOverride(IInputDisablingOverride disablingOverride);

        void RemoveOverride(IInputDisablingOverride disablingOverride);
    }

    public interface IInputDisablingBaseObject
    {
        /// <summary>
        ///     The host GameObject.
        /// </summary>
        public GameObject GameObject { get; }
    }

    public class InputDisablingManager : MonoBehaviour, IInputDisablingManager
    {
        /// <summary>
        ///     List of items that are actively disabling input.
        /// </summary>
        readonly List<IInputDisablingOverride> m_ActiveOverrides = new();
        /// <summary>
        ///     List of items that are affected by the input disabling state.
        /// </summary>
        readonly List<IInputDisablingSubscriber> m_InputDisablingSubscribers = new();

        public List<IInputDisablingSubscriber> InputDisablingSubscribers => m_InputDisablingSubscribers;

        public List<IInputDisablingOverride> ActiveOverrides => m_ActiveOverrides;

        public void AddSubscriber(IInputDisablingSubscriber subscriber)
        {
            m_InputDisablingSubscribers.Add(subscriber);
        }

        public void RemoveSubscriber(IInputDisablingSubscriber subscriber)
        {
            m_InputDisablingSubscribers.Remove(subscriber);
        }

        public void AddOverride(IInputDisablingOverride disablingOverride)
        {
            m_ActiveOverrides.Add(disablingOverride);
            DisableAll();
        }

        public void RemoveOverride(IInputDisablingOverride disablingOverride)
        {
            m_ActiveOverrides.Remove(disablingOverride);
            if (m_ActiveOverrides.Count == 0)
            {
                RestoreAll();
            }
        }

        /// <summary>
        ///     Disable all subscribers.
        /// </summary>
        void DisableAll()
        {
            var subscribers = m_InputDisablingSubscribers.ToArray();
            foreach (var subscriber in subscribers)
            {
                if (subscriber == null)
                    continue;

                subscriber.Disable();
            }
        }

        /// <summary>
        ///     Restore all subscribers.
        /// </summary>
        void RestoreAll()
        {
            var subscribers = m_InputDisablingSubscribers.ToArray();
            foreach (var subscriber in subscribers)
            {
                if (subscriber == null)
                    continue;

                subscriber.Enable();
            }
        }
    }
}

using System;
using UnityEngine;

namespace Unity.ReferenceProject.UIInputBlocker
{
    public interface IUIInputBlockerEventsDispatcher
    {
        public bool IsListenersAvailable { get; }
        public void DispatchRay(Ray ray);
    }

    public interface IUIInputBlockerEvents
    {
        public event Action<Ray> OnDispatchRay;
    }

    public class UIInputBlockerEventsDispatcher : MonoBehaviour, IUIInputBlockerEvents, IUIInputBlockerEventsDispatcher
    {
        public event Action<Ray> OnDispatchRay;

        public bool IsListenersAvailable => OnDispatchRay?.GetInvocationList().Length > 0;

        public void DispatchRay(Ray ray) => OnDispatchRay?.Invoke(ray);
    }
}

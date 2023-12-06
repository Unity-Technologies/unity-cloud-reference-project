using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.Common
{
    public class ServicesInitializer : MonoBehaviour
    {
        public event Action UnityServicesInitialized;
        public bool Initialized => UnityServices.State == ServicesInitializationState.Initialized;
        
        InitializationOptions m_InitializationOptions;

        [Inject]
        void Setup(InitializationOptions initializationOptions)
        {
            m_InitializationOptions = initializationOptions;
        }

        async Task Awake()
        {
            if (UnityServices.State != ServicesInitializationState.Uninitialized)
                return;

            await UnityServices.InitializeAsync(m_InitializationOptions);
            UnityServicesInitialized?.Invoke();
        }
    }
}

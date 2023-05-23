using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    public class UIInteractorRegister : MonoBehaviour
    {
        void OnEnable()
        {
            var module = EventSystem.current?.GetComponent<XRUIInputModuleUIToolkit>();

            if (module != null)
            {
                var interactors = GetComponents<IUIInteractor>();

                foreach (var interactor in interactors)
                {
                    module.RegisterInteractor(interactor);
                }
            }
        }

        void OnDisable()
        {
            var module = EventSystem.current?.GetComponent<XRUIInputModuleUIToolkit>();

            if (module != null)
            {
                var interactors = GetComponents<IUIInteractor>();

                foreach (var interactor in interactors)
                {
                    module.UnregisterInteractor(interactor);
                }
            }
        }
    }
}

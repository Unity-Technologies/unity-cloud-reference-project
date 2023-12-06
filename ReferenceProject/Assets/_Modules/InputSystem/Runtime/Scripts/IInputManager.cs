using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Unity.ReferenceProject.InputSystem
{
    /// <summary>
    /// InputManager is responsible to centralise all inputs binding for easy of access and unify single/double click/touch
    /// </summary>
    public interface IInputManager
    {
        /// <summary>
        /// Set an InputScheme to have priority on it's input triggering.
        /// If other InputScheme also have same input binding only the priority scheme will be triggered
        /// </summary>
        /// <param name="scheme"></param>
        void SetPriorityInputScheme(InputScheme scheme);

        /// <summary>
        /// Reset the priority InputScheme to null
        /// </summary>
        void UnsetPriorityInputScheme();

        /// <summary>
        /// Return the current priority scheme if one is set, otherwise null
        /// </summary>
        /// <returns></returns>
        InputScheme GetPriorityInputScheme();

        /// <summary>
        /// The UI should set this flag to true when and UI element is currently focused and world inputs should not be triggered.
        /// This flag is linked to IsUISelectionCheckEnabled in InputActionWrapper
        /// </summary>
        bool IsUIFocused { get; set; }

        /// <summary>
        /// This will enable/disable all InputScheme linked to the category provided.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="state"></param>
        void SetSchemeCategoryState(InputSchemeCategory category, bool state);

        /// <summary>
        /// Return all current InputScheme categories disabled
        /// </summary>
        /// <returns></returns>
        IReadOnlyCollection<InputSchemeCategory> GetDisableSchemeCategories();

        /// <summary>
        /// This will create a new InputScheme base of an InputActionAsset. All InputAction in the InputActionAsset will be wrapp into InputActionWrapper.
        /// If there is already an InputScheme of the type provided it will be returned and no new InputScheme will be created.
        /// In the case of the InputSchemeType.Other, an InputScheme will always be created.
        /// </summary>
        /// <param name="schemeType"></param>
        /// <param name="schemeCategory"></param>
        /// <param name="inputActionAsset"></param>
        /// <returns></returns>
        InputScheme GetOrCreateInputScheme(InputSchemeType schemeType, InputSchemeCategory schemeCategory, InputActionAsset inputActionAsset);

        /// <summary>
        /// This will create a new InputScheme base of an InputActionAsset. All InputAction in the actions array will be wrap into InputActionWrapper.
        /// If there is already an InputScheme of the type provided it will be returned and no new InputScheme will be created.
        /// In the case of the InputSchemeType.Other, an InputScheme will always be created.
        /// </summary>
        /// <param name="schemeType"></param>
        /// <param name="schemeCategory"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        InputScheme GetOrCreateInputScheme(InputSchemeType schemeType, InputSchemeCategory schemeCategory, InputAction[] actions);
    }
}
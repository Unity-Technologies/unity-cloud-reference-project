using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

// TODO: UI Blocking?
// TODO: "Blink" while moving.

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    ///     Allows the user to aim at a spot on the ground and teleport themself to that position. They can also adjust the
    ///     direction they will be facing.
    ///     If the Teleport action has a negative value then the user will be moved backwards (relative to the camera) a fixed
    ///     distance.
    /// </summary>
    public class Teleport : BaseLocomotionProvider
    {
        [SerializeField]
        InputActionReference m_TeleportDirectionInput;

        [SerializeField, Tooltip("A prefab that will be shown when aiming that determines the target position and rotation.")]
        TeleportVisuals m_TeleportVisualsPrefab;

        [SerializeField, Tooltip("The delay after releasing the teleport button to start the teleportation.")]
        float m_TeleportMoveDelay = 0.375f;

        [SerializeField, Tooltip("The distance that the rig will be moved when the user does a step back action (Teleport action with negative value).")]
        float m_StepBackDistance = 0.25f;
        bool m_Aiming;

        bool m_Moving;
        bool m_SteppedBack;
        InputAction m_TeleportAction;
        InputAction m_TeleportDirectionAction;
        TeleportVisuals m_TeleportVisuals;

        /// <summary>
        ///     A prefab that will be shown when aiming that determines the target position and rotation.
        /// </summary>
        public TeleportVisuals TeleportVisualsPrefab
        {
            get => m_TeleportVisualsPrefab;
            set => m_TeleportVisualsPrefab = value;
        }

        /// <summary>
        ///     The delay after releasing the teleport button to start the teleportation.
        /// </summary>
        public float TeleportMoveDelay
        {
            get => m_TeleportMoveDelay;
            set => m_TeleportMoveDelay = value;
        }

        /// <summary>
        ///     The distance that the rig will be moved when the user does a step back action (Teleport action with negative
        ///     value).
        /// </summary>
        public float StepBackDistance
        {
            get => m_StepBackDistance;
            set => m_StepBackDistance = value;
        }

        public bool IsTeleporting => m_Aiming || m_Moving;

        protected override void Awake()
        {
            m_TeleportDirectionAction = m_TeleportDirectionInput.action;

            base.Awake();

            m_TeleportAction = m_InputAction;
        }

        void Start()
        {
            var teleportGameObject = Instantiate(m_TeleportVisualsPrefab.gameObject, system.xrOrigin.transform);
            teleportGameObject.SetActive(false);
            m_TeleportVisuals = teleportGameObject.GetComponentInChildren<TeleportVisuals>();

            var ignoreColliders = new List<Collider>();
            system.xrOrigin.gameObject.GetComponentsInChildren(true, ignoreColliders);

            m_TeleportVisuals.ignoredGameObjects = new HashSet<GameObject>(ignoreColliders.ConvertAll(rigCollider => rigCollider.gameObject));
            teleportGameObject.transform.localPosition = Vector3.zero;
            teleportGameObject.transform.localRotation = Quaternion.identity;

            var rayInteractor = transform.parent.GetComponent<XRRayInteractor>();
            m_TeleportVisuals.xrRayInteractor = rayInteractor;
        }

        void Update()
        {
            if (m_Aiming)
            {
                m_TeleportVisuals.SetVisible(true);

                var mainCamera = system.xrOrigin.Camera;
                if (m_TeleportDirectionAction.IsPressed() && mainCamera != null)
                {
                    var stickInput = m_TeleportDirectionAction.ReadValue<Vector2>();
                    var angle = Mathf.Atan2(stickInput.x, stickInput.y) * Mathf.Rad2Deg; // Atan of x/y to get angle relative to up
                    var cameraTransform = mainCamera.transform;
                    var camForward = cameraTransform.forward;
                    camForward.y = 0;
                    var lookRotation = Quaternion.LookRotation(camForward, Vector3.up);

                    m_TeleportVisuals.Rotate(angle, cameraTransform.parent.rotation, lookRotation);
                }
                else
                {
                    m_TeleportVisuals.SetVisible(false);
                    if (CanBeginLocomotion() && !m_Moving && m_TeleportVisuals.gameObject.activeSelf)
                    {
                        if (m_TeleportVisuals.targetPosition.HasValue && m_TeleportVisuals.targetRotation.HasValue)
                        {
                            StartCoroutine(MoveTowardTarget(m_TeleportVisuals.targetPosition.Value, m_TeleportVisuals.targetRotation.Value));
                        }

                        m_TeleportVisuals.PlayRayAnim(m_TeleportMoveDelay);
                    }

                    m_Aiming = false;
                    SetSiblingLocomotionProviders(true);
                }
            }
        }

        protected override void InitializeInputs()
        {
            base.InitializeInputs();
            m_TeleportDirectionAction.Enable();
        }

        protected override void ResetInputs()
        {
            base.ResetInputs();
            m_TeleportDirectionAction.Disable();
        }

        void SetSiblingLocomotionProviders(bool active)
        {
            var results = transform.parent.GetComponentsInChildren<LocomotionProvider>(true);

            foreach (var locomotionProvider in results)
            {
                if (locomotionProvider != this)
                    locomotionProvider.gameObject.SetActive(active);
            }
        }

        protected override void OnPerformed(InputAction.CallbackContext callbackContext)
        {
            if (!isActiveAndEnabled)
                return;

            var TeleportValue = m_TeleportAction.ReadValue<float>();
            if (TeleportValue <= 0.0f)
            {
                if (!m_Aiming && !m_SteppedBack)
                {
                    TakeAStepBack();
                    m_SteppedBack = true;
                }
            }
            else
            {
                SetSiblingLocomotionProviders(false);
                m_Aiming = true;
            }
        }

        protected override void OnCanceled(InputAction.CallbackContext callbackContext)
        {
            m_SteppedBack = false;
        }

        void TakeAStepBack()
        {
            var mainCamera = system.xrOrigin.Camera;
            if (mainCamera != null && BeginLocomotion())
            {
                var xrRigTransform = system.xrOrigin.transform;

                var stepBackDirection = -mainCamera.transform.forward;
                stepBackDirection.y = 0f;
                xrRigTransform.position = xrRigTransform.position + stepBackDirection * (m_StepBackDistance /* * this.GetViewerScale() */);

                EndLocomotion();
            }
        }

        IEnumerator MoveTowardTarget(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (!BeginLocomotion())
            {
                m_TeleportVisuals.SetVisible(false);
                yield break;
            }

            m_Moving = true;
            yield return new WaitForSeconds(m_TeleportMoveDelay);

            var xrRigTransform = system.xrOrigin.transform;
            var currentPosition = xrRigTransform.position;

            var mainCamera = system.xrOrigin.Camera;
            if (mainCamera != null)
            {
                var cameraToRigOffset = currentPosition - mainCamera.transform.position;
                cameraToRigOffset = targetRotation * Quaternion.Inverse(xrRigTransform.rotation) * cameraToRigOffset;
                cameraToRigOffset.y = 0;
                targetPosition += cameraToRigOffset;
            }

            const float kTargetDuration = 0.05f;
            var currentDuration = 0f;
            while (currentDuration < kTargetDuration)
            {
                currentDuration += Time.unscaledDeltaTime;
                currentPosition = Vector3.Lerp(currentPosition, targetPosition, currentDuration / kTargetDuration);
                xrRigTransform.position = currentPosition;
                yield return null;
            }

            xrRigTransform.position = targetPosition;
            xrRigTransform.rotation = targetRotation;

            m_TeleportVisuals.SetVisible(false);
            m_Moving = false;

            EndLocomotion();
        }
    }
}

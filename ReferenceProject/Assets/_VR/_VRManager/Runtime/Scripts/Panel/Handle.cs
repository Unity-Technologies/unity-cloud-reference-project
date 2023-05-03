using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.ReferenceProject.VRManager
{
    public class Handle : XRBaseInteractable
    {
        const int k_DefaultCapacity = 2; // i.e. 2 controllers
        const float k_MaxPushDistanceMultiplier = 5f;
        const float k_MaxPullDistanceMultiplier = 0.1f;
        const float k_PushPullThreshold = 0.05f;
        const float k_MaxPushPullDistance = 0.25f;
        const float k_PushPullFactor = 0.25f;
        static readonly List<IXRInteractable> k_Targets = new List<IXRInteractable>();
        [SerializeField]
        InputActionAsset m_InputActionAsset;

        [SerializeField]
        float m_MaxPushDistance = 100f;

        [SerializeField]
        float m_PushPullAcceleration;
        readonly List<IXRInteractor> m_HoveringInteractors = new List<IXRInteractor>(k_DefaultCapacity);

        readonly List<IXRInteractor> m_SelectingInteractors = new List<IXRInteractor>(k_DefaultCapacity);

        float m_DefaultHoldingDistance;
        Vector3 m_GrabOffset;
        float m_InitialPushPullAmount;
        InputActionMap m_LocomotionInputActionMap;

        InputAction m_PushPullInputAction;
        float m_PushPullVelocity;
        Vector3 m_RelativeStartOrigin;

        protected override void Awake()
        {
            base.Awake();

            m_PushPullInputAction = m_InputActionAsset["PushPull"];
            m_LocomotionInputActionMap = m_InputActionAsset.FindActionMap("Locomotion");
        }

        protected override void OnDisable()
        {
            m_HoveringInteractors.Clear();
            m_SelectingInteractors.Clear();

            base.OnDisable();
        }

        public event Action DragStarted;
        public event Action<Vector3> Dragging;
        public event Action DragEnded;

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            if (m_SelectingInteractors.Any())
            {
                OnHandleSelecting(m_SelectingInteractors[0]);
            }

            foreach (var interactor in m_SelectingInteractors)
            {
                OnHandleDragging(interactor);
            }
        }

        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            if (!enabled || !base.IsHoverableBy(interactor))
                return false;

            // Target list is cleared by GetValidTargets
            interactor.GetValidTargets(k_Targets);

            // Only hover if the handle is the first (nearest) target
            var nearest = k_Targets.IndexOf(this) == 0;

            // Only hover if the interactor select is not active yet, or it's already hovering or selecting this
            var isSelectActive = (interactor as IXRSelectInteractor)?.isSelectActive ?? false;
            var canHover = !isSelectActive || m_HoveringInteractors.Contains(interactor) || m_SelectingInteractors.Contains(interactor);
            return nearest && canHover;
        }

        public override bool IsSelectableBy(IXRSelectInteractor interactor)
        {
            if (!base.IsSelectableBy(interactor))
                return false;

            // Only select if already hovering or selecting
            return m_HoveringInteractors.Contains(interactor) || m_SelectingInteractors.Contains(interactor);
        }

        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            m_HoveringInteractors.Add(args.interactorObject);
        }

        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            m_HoveringInteractors.Remove(args.interactorObject);
        }

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            m_SelectingInteractors.Add(args.interactorObject);
            if (m_SelectingInteractors.Count == 1)
            {
                m_PushPullInputAction.Enable();

                // TODO: Should be replace by a more generic call to a manager
                m_LocomotionInputActionMap.Disable();
            }

            OnHandleDragStarted(args.interactorObject);
        }

        void OnHandleDragStarted(IXRSelectInteractor interactor)
        {
            var rayOrigin = interactor.GetAttachTransform(this);
            var rayOriginPosition = rayOrigin.position;
            var rigidBodyPosition = transform.position;
            var worldPosition = rayOriginPosition;

            if (interactor is ILineRenderable lineRenderable && lineRenderable.TryGetHitInfo(out var position, out _, out _, out _))
            {
                worldPosition = position;
            }

            m_DefaultHoldingDistance = Vector3.Distance(rayOriginPosition, worldPosition);
            m_RelativeStartOrigin = rayOriginPosition;
            m_InitialPushPullAmount = CalculatePushPullAmount(rayOrigin);
            m_GrabOffset = worldPosition - rigidBodyPosition;
            DragStarted?.Invoke();
        }

        void OnHandleSelecting(IXRInteractor interactor)
        {
            var inputAxis = m_PushPullInputAction.ReadValue<float>();
            if (m_PushPullInputAction.inProgress)
            {
                var timeDelta = Time.unscaledDeltaTime;

                // If input is stopped, or is in the opposite direction, then reset speed to 0
                if (inputAxis * inputAxis < 0.001f || Mathf.Sign(inputAxis * m_PushPullVelocity) < 0f)
                {
                    m_PushPullVelocity = 0f;
                }
                else
                {
                    m_PushPullVelocity += inputAxis * m_PushPullAcceleration * timeDelta;
                }

                m_DefaultHoldingDistance += m_PushPullVelocity * timeDelta;
                m_DefaultHoldingDistance = Mathf.Clamp(m_DefaultHoldingDistance, 0f, m_MaxPushDistance);
            }
        }

        void OnHandleDragging(IXRInteractor interactor)
        {
            var rayOrigin = interactor.GetAttachTransform(this);

            var pushPullAmount = CalculatePushPullAmount(rayOrigin);
            var pushPullDiff = pushPullAmount - m_InitialPushPullAmount;
            var targetHoldDistance = m_DefaultHoldingDistance;

            if (pushPullDiff > k_PushPullThreshold)
            {
                var pushDistance = pushPullDiff - k_PushPullThreshold;
                var pushPercent = Mathf.Clamp01(pushDistance / k_MaxPushPullDistance);
                var pushFactor = pushPercent * pushPercent * k_PushPullFactor;
                targetHoldDistance = Mathf.Lerp(m_DefaultHoldingDistance, k_MaxPushDistanceMultiplier * m_DefaultHoldingDistance, pushFactor);
            }
            else if (pushPullDiff < -k_PushPullThreshold)
            {
                var pullDistance = -pushPullDiff - k_PushPullThreshold;
                var pullPercent = Mathf.Clamp01(pullDistance / k_MaxPushPullDistance);
                var pullFactor = pullPercent * pullPercent * k_PushPullFactor;
                targetHoldDistance = Mathf.Lerp(m_DefaultHoldingDistance, k_MaxPullDistanceMultiplier * m_DefaultHoldingDistance, pullFactor);
            }

            var targetPosition = new Ray(rayOrigin.position, rayOrigin.forward).GetPoint(targetHoldDistance);
            var grabPosition = (transform.position + m_GrabOffset);
            var delta = targetPosition - grabPosition;

            Dragging?.Invoke(delta);
        }

        float CalculatePushPullAmount(Transform rayOrigin)
        {
            // Determines how much the ray origin transform has been pushed or pulled in the direction it is facing from its initial position.
            // The amount factors in the scale of the main camera and compares the ray origin local position to its initial position
            var handPosition = rayOrigin.position;
            var handForward = rayOrigin.forward;
            var referencePosition = m_RelativeStartOrigin;
            var headToHand = (handPosition - referencePosition);

            return Vector3.Dot(headToHand, handForward);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            m_SelectingInteractors.Remove(args.interactorObject);
            if (m_SelectingInteractors.Count == 0)
            {
                m_PushPullInputAction.Disable();
                m_LocomotionInputActionMap.Enable();
            }

            DragEnded?.Invoke();
        }
    }
}

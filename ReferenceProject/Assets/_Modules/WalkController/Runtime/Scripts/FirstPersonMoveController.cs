using System;
using UnityEngine;

namespace Unity.ReferenceProject.WalkController
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonMoveController : MonoBehaviour, IFirstPersonMoveController
    {
        [SerializeField]
        float m_MoveSpeed = 1;

        [SerializeField]
        float m_SprintMoveSpeed = 3;

        [SerializeField]
        float m_JumpForce = 1;

        [SerializeField]
        float m_MaxFallSpeed = 40;

        [SerializeField]
        float m_MaxSlopeAngle = 50;

        CharacterController m_Controller;
        bool m_IsJumpingInput;
        Vector3 m_MoveDirectionInput;
        Vector3 m_PlayerVelocity;

        void Start()
        {
            m_Controller = GetComponent<CharacterController>();
            if (m_MaxFallSpeed > 0)
                m_MaxFallSpeed *= -1;
        }

        void Update()
        {
            Move();
        }

        public event Action onJump;

        public bool isGrounded { get; private set; }

        public bool isRunning { get; private set; }

        public bool useGravity { get; set; }

        void Move()
        {
            var groundHitInfo = GroundCheck();
            isGrounded = m_Controller.isGrounded || groundHitInfo.transform;
            var isUseGravity = GravityCheck();

            var slopeAngle = Vector3.Angle(Vector3.up, groundHitInfo.normal);

            // Moving
            var currentTransform = transform;
            var dir = m_MoveDirectionInput.z * currentTransform.forward + m_MoveDirectionInput.x * currentTransform.right;
            dir = Vector3.ClampMagnitude(dir, 1); // making the same movement speed in all directions
            dir = Vector3.ProjectOnPlane(dir,
                isUseGravity && slopeAngle < m_MaxSlopeAngle ? groundHitInfo.normal : Vector3.up).normalized;

            var moveSpeed = isRunning ? m_SprintMoveSpeed : m_MoveSpeed;

            var desiredMove = Vector3.zero;
            desiredMove.x = dir.x * moveSpeed;
            desiredMove.z = dir.z * moveSpeed;

            m_MoveDirectionInput = Vector3.zero; // zeroing input to prevent infinity movement

            // Jumping
            // m_PlayerVelocity.y comparing to preventing double jump, when during first one player catch ground
            if (m_IsJumpingInput && isGrounded && m_PlayerVelocity.y <= 0.2f)
            {
                m_PlayerVelocity.y += Mathf.Sqrt(m_JumpForce * -3.0f * Physics.gravity.y);
                isGrounded = false;
                onJump?.Invoke();
            }

            m_IsJumpingInput = false; // zeroing input to prevent infinity jumping

            if (isGrounded)
            {
                if (m_PlayerVelocity.y < 0)
                    m_PlayerVelocity.y = 0;
            }
            else
            {
                m_PlayerVelocity.y += Physics.gravity.y * Time.deltaTime;
            }

            // clamping vertical m_PlayerVelocity.y to m_MaxFallSpeed
            if (m_PlayerVelocity.y < m_MaxFallSpeed)
                m_PlayerVelocity.y = m_MaxFallSpeed;

            if (!isUseGravity)
                m_PlayerVelocity.y = 0;

            m_Controller.Move((desiredMove + m_PlayerVelocity) * Time.deltaTime);
        }

        RaycastHit GroundCheck()
        {
            Physics.SphereCast(transform.position, m_Controller.radius, Vector3.down, out var groundHitInfo,
                m_Controller.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            return groundHitInfo;
        }

        bool GravityCheck() => useGravity || Physics.Raycast(transform.position, Vector3.down);

        public void OnMoveInput(Vector2 direction)
        {
            m_MoveDirectionInput.x = direction.x;
            m_MoveDirectionInput.z = direction.y;
        }

        public void OnJumpInput()
        {
            m_IsJumpingInput = true;
        }

        public void OnSprintInput(bool isSprinting)
        {
            isRunning = isSprinting;
        }
    }
}

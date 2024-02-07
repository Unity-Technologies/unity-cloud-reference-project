using System;
using System.Threading.Tasks;
using Unity.Cloud.DataStreaming.Runtime;
using Unity.ReferenceProject.Messaging;
using Unity.ReferenceProject.ObjectSelection;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.WalkController
{
    [RequireComponent(typeof(CharacterController))]
    public class WalkModeMoveController : MonoBehaviour
    {
        [SerializeField]
        float m_WalkSpeed = 1;
        [SerializeField]
        float m_SprintSpeed = 3;
        [SerializeField]
        float m_JumpForce = 0.7f;
        [SerializeField]
        float m_StepUpSpeed = 0.8f; // the lower the value, the "smoother" the feel
        [SerializeField]
        float m_MaxFallSpeed = -30;
        [SerializeField]
        float m_GroundedRayLength = 1.5f; // will fire downcast 1.5m from character's feet, regardless of character height
        
        const string k_WalkSpeed = "WalkSpeed";
        const string k_SprintSpeed = "SprintSpeed";
        const string k_JumpForce = "JumpForce";
        const string k_CharacterHeight = "CharacterHeight";
        bool m_SettingValuesInitialized;
        
        public float WalkSpeed
        {
            get
            {
                if (m_SettingValuesInitialized)
                {
                    return m_WalkSpeed;
                }
                return PlayerPrefs.GetFloat(k_WalkSpeed, m_WalkSpeed);
            }
            set
            {
                m_WalkSpeed = value;
                PlayerPrefs.SetFloat(k_WalkSpeed, value);
            }
        }
        
        public float SprintSpeed
        {
            get
            {
                if (m_SettingValuesInitialized)
                {
                    return m_SprintSpeed;
                }
                return PlayerPrefs.GetFloat(k_SprintSpeed, m_SprintSpeed);            
            }
            set
            {
                m_SprintSpeed = value;
                PlayerPrefs.SetFloat(k_SprintSpeed, value);
            }
        }
        
        public float JumpForce
        {
            get
            {
                if (m_SettingValuesInitialized)
                {
                    return m_JumpForce;
                }
                return PlayerPrefs.GetFloat(k_JumpForce, m_JumpForce);
            }
            set
            {
                m_JumpForce = value;
                PlayerPrefs.SetFloat(k_JumpForce, value);
            }
        }

        public float CharacterHeight
        {
            get => PlayerPrefs.GetFloat(k_CharacterHeight, 1.5f);
            set
            {
                if (m_Controller != null)
                {
                    m_Controller.height = value;
                    PlayerPrefs.SetFloat(k_CharacterHeight, value);
                }
            }
        }
        
        CharacterController m_Controller;
        IObjectPicker m_Picker;
        Task m_GetGroundTask;
        PickerResult m_DownCastResult;
        Vector3 m_MoveDirectionInput;
        Vector3 m_PlayerVelocity;
        IAppMessaging m_AppMessaging;
        
        bool m_IsGrounded;
        bool m_IsSprinting;
        bool m_IsJumpingInput;
        bool m_IsTeleporting;
        bool m_IsHoverTeleporting; // If Teleport is above ground, hover until movement input (e.g. for Annotations GoTo)
        bool m_NoGroundWarningShown;
        bool m_GoUpStep;
        float m_StepUpTime;

        
        [Inject]
        void Setup(IObjectPicker objectPicker, IAppMessaging appMessaging)
        {
            m_Picker = objectPicker;
            m_AppMessaging = appMessaging;
        }
        
        void Awake()
        {
            m_Controller = GetComponent<CharacterController>();
            m_Controller.height = CharacterHeight;
        }

        void Start()
        {
            m_WalkSpeed = PlayerPrefs.GetFloat(k_WalkSpeed, m_WalkSpeed);
            m_SprintSpeed = PlayerPrefs.GetFloat(k_SprintSpeed, m_SprintSpeed);
            m_JumpForce = PlayerPrefs.GetFloat(k_JumpForce, m_JumpForce);
            m_SettingValuesInitialized = true;
        }
        
        void FixedUpdate()
        {
            if (m_Picker == null)
            {
                return;
            }
            Move();
        }

        /// <summary>
        ///     The main function to control movement
        /// </summary>
        void Move()
        {
            if (m_IsTeleporting)
                return;
            
            var currentTransform = transform;
            
            if (m_GetGroundTask?.IsCompleted ?? true)
            {
                _ = GetGroundAsync(currentTransform);
            }
            
            // y movement
            HandleJump();
            HandleGround();
            HandleStepUp();
            
            // x and z movement
            var desiredMove = XZMovement(currentTransform);
            
            transform.position += (desiredMove + m_PlayerVelocity) * Time.deltaTime;
        }

        void HandleJump()
        {
            // allows jumping mid-air or on no ground detected
            if (m_IsJumpingInput && m_PlayerVelocity.y <= 0.2f)
            {
                m_PlayerVelocity.y += Mathf.Sqrt(m_JumpForce * -3.0f * Physics.gravity.y);
                m_IsGrounded = false;
            }
            m_IsJumpingInput = false; // zeroing input to prevent infinity jumping
        }
        
        void HandleGround()
        {
            if (m_DownCastResult.HasIntersected && m_IsGrounded && m_PlayerVelocity.y <= 0)
            {
                m_PlayerVelocity.y = 0;
            }
            else if (!m_DownCastResult.HasIntersected && m_PlayerVelocity.y <= 0)
            {
                m_PlayerVelocity.y = 0;
            }
            else if (m_IsHoverTeleporting)
            {
                m_PlayerVelocity.y = 0;
            }
            else
            {
                m_PlayerVelocity.y += Physics.gravity.y * Time.deltaTime;
            }

            Mathf.Clamp(m_PlayerVelocity.y, m_MaxFallSpeed, m_MaxFallSpeed * -1f);
        }

        void HandleStepUp()
        {
            m_StepUpTime += Time.deltaTime * m_StepUpSpeed;
            if (m_GoUpStep)
            {
                var stepUpDistance = m_GroundedRayLength - m_DownCastResult.Distance;
                transform.position += new Vector3(0f, Mathf.Lerp(0f, stepUpDistance, m_StepUpTime), 0f);
            }
            m_GoUpStep = false;
        }

        Vector3 XZMovement(Transform currentTransform)
        {
            var dir = m_MoveDirectionInput.z * currentTransform.forward + m_MoveDirectionInput.x * currentTransform.right;
            dir = Vector3.ClampMagnitude(dir, 1); // making the same movement speed in all directions
            var normal = m_DownCastResult.HasIntersected ? m_DownCastResult.Normal : Vector3.up;
            dir = Vector3.ProjectOnPlane(dir, normal).normalized;

            var moveSpeed = m_IsSprinting ? m_SprintSpeed : m_WalkSpeed;
            var desiredMove = Vector3.zero;
            desiredMove.x = dir.x * moveSpeed;
            desiredMove.z = dir.z * moveSpeed;

            m_MoveDirectionInput = Vector3.zero; // zeroing input to prevent infinity movement
            return desiredMove;
        }

        async Task GetGroundAsync(Transform currentTransform)
        {
            m_GetGroundTask = DownCast();
            await m_GetGroundTask;
            if (m_DownCastResult.HasIntersected)
            {
                var feetPosition = currentTransform.position.y - m_Controller.height/2f;
                m_IsGrounded = m_Controller.isGrounded || feetPosition <= m_DownCastResult.Point.y;
                m_GoUpStep = m_DownCastResult.Distance < m_GroundedRayLength - 0.05f; // add epsilon to ignore miniscule steps
                if (!m_GoUpStep)
                {
                    m_StepUpTime = 0f; // reset
                }

                m_NoGroundWarningShown = false;
            }
            else
            {
                HandleNoGroundWarning();
            }
        }
        
        void HandleNoGroundWarning()
        {
            if (!m_NoGroundWarningShown)
            {
                m_AppMessaging.ShowWarning("@WalkMode:NoGroundDetected");
                m_NoGroundWarningShown = true;
            }
        }

        Vector3 GetDownCastFirePosition()
        {
            var currPos = transform.position;
            var feetPos = currPos.y - m_Controller.height / 2f;
            currPos.y = feetPos + m_GroundedRayLength;
            return currPos;
        }
        
        async Task DownCast()
        {
            try
            {
                m_DownCastResult = await m_Picker.PickAsync(new Ray(GetDownCastFirePosition(), Vector3.down)); 
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void OnMoveInput(Vector2 direction)
        {
            m_MoveDirectionInput.x = direction.x;
            m_MoveDirectionInput.z = direction.y;
        }

        public void OnMoveInputPressed()
        {
            m_IsHoverTeleporting = false;
        }

        public void OnJumpInput()
        {
            m_IsJumpingInput = true;
            m_IsHoverTeleporting = false;
        }

        public void OnSprintInput(bool isSprinting)
        {
            m_IsSprinting = isSprinting;
            m_IsHoverTeleporting = false;
        }

        public void OnTeleportInput(bool isTeleporting)
        {
            m_IsTeleporting = isTeleporting;
            m_IsHoverTeleporting = false;
        }
        
        public void OnHoverTeleportInput()
        {
            m_IsHoverTeleporting = true;
        }
    }
}

using System;
using System.Threading.Tasks;
using Unity.ReferenceProject.Navigation;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject.AppCamera
{
    public enum LookAtConstraint
    {
        /// <summary>
        ///     The lookAt point does not move with the camera, resulting
        ///     in the camera to continue to look at the same point.
        /// </summary>
        StandBy,

        /// <summary>
        ///     The lookAt point moves with the position, resulting in
        ///     the camera to keep its current rotation while moving.
        /// </summary>
        Follow
    }

    public class CameraProxy : MonoBehaviour
    {
        [SerializeField]
        CameraProxySettings m_Settings;

        [SerializeField, Tooltip("Once set, the camera will follow this transform")]
        Camera m_Camera;
        
        readonly float m_Acceleration = 10.0f;
        readonly float m_MaxSpeed = 1000.0f;

        // Default values to hide missing call to SetupCameraSpeed (hotfix)
        readonly float m_MinSpeed = 1.0f;
        readonly float m_WaitingDeceleration = 40.0f;

        Vector2 m_AngleOffset;

        Vector3 m_DesiredLookAt;
        Vector3 m_DesiredPosition;
        Quaternion m_DesiredRotation;
        Vector2 m_DesiredRotationEuler;
        Vector3 m_DesiredScale = Vector3.one;

        bool m_IsSphericalMovement;
        LookAtConstraint m_LookAtConstraint;
        Vector3 m_MovingDirection;
        float m_MovingSpeed;

        float m_PanningScale = 1.0f;
        Task m_Task;

        [Inject]
        public void Setup(Camera targetCamera, INavigationManager navigationManager)
        {
            m_Camera = targetCamera;
            navigationManager.NavigationTeleported += OnTeleport;
        }

        public CameraProxySettings settings
        {
            get => m_Settings;
            set => m_Settings = value;
        }

        void Start()
        {
            m_DesiredLookAt = m_Camera.transform.position + m_Camera.transform.forward * 10;
            m_DesiredPosition = m_Camera.transform.position;
            m_DesiredRotation = m_Camera.transform.rotation;
            m_DesiredRotationEuler = m_DesiredRotation.eulerAngles;

            m_IsSphericalMovement = false;
        }

        void Update()
        {
            var delta = Time.unscaledDeltaTime;

            if (m_MovingDirection != Vector3.zero)
            {
                var offset = m_DesiredRotation * m_MovingDirection * m_MovingSpeed * delta;

                m_DesiredPosition += offset;

                if (m_LookAtConstraint == LookAtConstraint.Follow)
                {
                    m_DesiredLookAt += offset;
                }

                m_MovingSpeed = Mathf.Clamp(m_MovingSpeed + m_Acceleration * delta, m_MinSpeed, m_MaxSpeed);
            }
            else
            {
                if (delta < 0.1f) // Should be based on UINavigationControllerSettings' inputLagSkipThreshold, but it's not in the scope.  Using default value for now.
                {
                    m_MovingSpeed = Mathf.Clamp(m_MovingSpeed - m_WaitingDeceleration * delta, m_MinSpeed, m_MaxSpeed);
                }
                else
                {
                    m_MovingSpeed = 0;
                }
            }

            var rotation = Quaternion.Lerp(m_Camera.transform.rotation, m_DesiredRotation, Mathf.Clamp(delta / m_Settings.RotationElasticity, 0.0f, 1.0f));
            m_Camera.transform.rotation = rotation;

            Vector3 position;
            if (m_IsSphericalMovement)
            {
                position = m_DesiredLookAt + rotation * Vector3.back * GetDistanceFromLookAt();
            }
            else
            {
                position = Vector3.Lerp(m_Camera.transform.position, m_DesiredPosition, Mathf.Clamp(delta / m_Settings.PositionElasticity, 0.0f, 1.0f));
            }

            m_Camera.transform.position = position;

            m_Camera.transform.localScale = Vector3.Lerp(m_Camera.transform.localScale, m_DesiredScale, Mathf.Clamp(delta / m_Settings.ScalingElasticity, 0.0f, 1.0f));
        }

        void UpdateSphericalMovement(bool isSphericalMovement)
        {
            m_IsSphericalMovement = isSphericalMovement;
        }

        /// <summary>
        ///     Move the camera in the specified local direction.
        /// </summary>
        /// <remarks>
        ///     After being called once, this method will continue to move the camera in
        ///     the specified direction. You need to call it with a zero vector to stop the camera from moving.
        /// </remarks>
        /// <param name="unitDir">A unit vector indicating the local direction in which the camera should move.</param>
        /// <param name="constraint">Specifies if the lookAt point is affected or not by this movement.</param>
        public void MoveInLocalDirection(Vector3 unitDir, LookAtConstraint constraint)
        {
            m_MovingDirection = unitDir;
            m_LookAtConstraint = constraint;

            UpdateSphericalMovement(false);
        }

        /// <summary>
        ///     Move the current position of the camera by an offset.
        /// </summary>
        /// <param name="offset">The offset by which the camera should be moved.</param>
        /// <param name="constraint">The constraint on lookAt point.</param>
        public void MovePosition(Vector3 offset, LookAtConstraint constraint)
        {
            m_DesiredPosition += offset;

            if (constraint == LookAtConstraint.Follow)
            {
                m_DesiredLookAt += offset;
            }

            UpdateSphericalMovement(false);
        }

        public void SetMovePosition(Vector3 pos, Quaternion rot)
        {
            m_DesiredPosition = pos;
            m_DesiredRotation = rot;
            m_DesiredRotationEuler = rot.eulerAngles;
            m_DesiredLookAt = m_DesiredRotation * new Vector3(0.0f, 0.0f, (m_DesiredLookAt - m_DesiredPosition).magnitude) + m_DesiredPosition;
            UpdateSphericalMovement(true);
            ForceStop();
        }

        /// <summary>
        ///     Drag the camera on the current frustum plane. If the
        ///     camera is looking forward, <see cref="Pan" /> will drag
        ///     the camera on its local up and right vectors.
        /// </summary>
        /// <param name="offset"></param>
        public void Pan(Vector3 offset)
        {
            offset = m_DesiredRotation * (offset * m_PanningScale);
            MovePosition(offset, LookAtConstraint.Follow);
        }

        public void PanStart(Vector2 pos)
        {
            var frustumCorners = new Vector3[4];
            var depth = -m_DesiredPosition.magnitude;
            m_Camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), depth, Camera.MonoOrStereoscopicEye.Mono, frustumCorners);
            m_PanningScale = Mathf.Abs((frustumCorners[2].x - frustumCorners[1].x) / Screen.width);
        }

        public float GetDistanceFromLookAt()
        {
            return (m_DesiredLookAt - m_DesiredPosition).magnitude;
        }

        /// <summary>
        ///     Move on the forward axis of the camera. The operation is similar
        ///     to a zoom without changing FOV.
        /// </summary>
        /// <remarks>
        ///     This function does nothing if the new distance from lookAt is greater than
        ///     the maximum camera distance.
        /// </remarks>
        /// <param name="nbUnits">
        ///     The number of units to move forward. A negative value
        ///     will move the camera away from the look at point.
        /// </param>
        public void MoveOnLookAtAxis(float nbUnits)
        {
            nbUnits *= GetDistanceFromLookAt() * m_Settings.MoveOnAxisScaling;
            var originalDistanceFromLookAt = GetDistanceFromLookAt();

            var forward = m_DesiredRotation * Vector3.forward;

            var pos = m_DesiredPosition + forward * nbUnits;

            m_DesiredPosition = pos;

            if (originalDistanceFromLookAt - nbUnits < m_Settings.MinDistanceFromLookAt)
            {
                m_DesiredLookAt = m_DesiredPosition + forward * m_Settings.MinDistanceFromLookAt;
            }

            UpdateSphericalMovement(false);
        }

        public void FocusOnPoint(Vector3 value)
        {
            var cameraPlane = new Plane(transform.forward, transform.position);
            var targetCameraPos = cameraPlane.ClosestPointOnPlane(value);
            m_DesiredLookAt = value;
            m_DesiredPosition = targetCameraPos;

            UpdateSphericalMovement(true);
        }

        /// <summary>
        ///     Rotate the camera by adding an offset to the current rotation.
        /// </summary>
        /// <remarks>
        ///     The <see cref="angleOffset" /> is a rotation in azimuth coordinate where Y is up
        ///     axis (azimuth angle, clockwise) and X, right axis (altitude, clockwise)
        /// </remarks>
        /// <param name="angleOffset">A rotation around the y axis, then the x axis</param>
        public void Rotate(Vector2 angleOffset)
        {
            m_DesiredRotationEuler += angleOffset;
            if (m_DesiredRotationEuler.x > 180)
                m_DesiredRotationEuler.x -= 360;
            m_DesiredRotationEuler.x = Mathf.Clamp(m_DesiredRotationEuler.x, -m_Settings.MaxPitchAngle, m_Settings.MaxPitchAngle);

            m_DesiredRotation =
                Quaternion.AngleAxis(m_DesiredRotationEuler.y, Vector3.up) *
                Quaternion.AngleAxis(m_DesiredRotationEuler.x, Vector3.right);

            m_DesiredLookAt = m_DesiredRotation * new Vector3(0.0f, 0.0f, (m_DesiredLookAt - m_DesiredPosition).magnitude) + m_DesiredPosition;

            UpdateSphericalMovement(false);
        }

        /// <summary>
        ///     Orbit around the look at point with up vector fixed to Y.
        /// </summary>
        /// <remarks>
        ///     The <see cref="angleOffset" /> is the same value that is provided in <see cref="Rotate" />
        ///     because the orbit rotation and camera rotation matches when orbiting.
        /// </remarks>
        /// <param name="angleOffset"></param>
        public void OrbitAroundLookAt(Vector2 angleOffset)
        {
            m_DesiredRotationEuler += angleOffset;
            if (m_DesiredRotationEuler.x > 180)
                m_DesiredRotationEuler.x -= 360;
            m_DesiredRotationEuler.x = Mathf.Clamp(m_DesiredRotationEuler.x, -m_Settings.MaxPitchAngle, m_Settings.MaxPitchAngle);

            m_DesiredRotation =
                Quaternion.AngleAxis(m_DesiredRotationEuler.y, Vector3.up) *
                Quaternion.AngleAxis(m_DesiredRotationEuler.x, Vector3.right);

            var negDistance = new Vector3(0.0f, 0.0f, -GetDistanceFromLookAt());
            m_DesiredPosition = m_DesiredRotation * negDistance + m_DesiredLookAt;

            UpdateSphericalMovement(true);
        }

        public void SetDistanceFromLookAt(float distance)
        {
            if (distance < m_Settings.MinDistanceFromLookAt)
            {
                distance = m_Settings.MinDistanceFromLookAt;
            }

            m_DesiredPosition = m_DesiredLookAt + (m_DesiredPosition - m_DesiredLookAt).normalized * distance;
        }

        public void ForceStop()
        {
            m_MovingSpeed = 0;
            m_MovingDirection = Vector3.zero;
        }

        public void SetRotation(Quaternion quaternion)
        {
            m_Camera.transform.rotation = quaternion;
            m_DesiredRotation = quaternion;
            m_DesiredRotationEuler = quaternion.eulerAngles;
            m_DesiredLookAt = m_DesiredRotation * new Vector3(0.0f, 0.0f, (m_DesiredLookAt - m_DesiredPosition).magnitude) + m_DesiredPosition;
        }

        public void TransformTo(Transform newTransform)
        {
            m_DesiredRotation = newTransform.rotation;
            m_DesiredRotationEuler = newTransform.rotation.eulerAngles;
            m_DesiredLookAt = newTransform.forward;
            m_DesiredPosition = newTransform.position;
            m_DesiredScale = newTransform.localScale;
            m_IsSphericalMovement = false;
        }

        internal void ApplyTransform(Vector3 newPosition, Vector3 newEulerAngle, Vector3 newForward)
        {
            var newRotation = Quaternion.Euler(newEulerAngle);

            m_Camera.transform.position = newPosition;
            m_Camera.transform.rotation = newRotation;
            m_DesiredRotation = newRotation;
            m_DesiredRotationEuler = newRotation.eulerAngles;
            m_DesiredLookAt = newForward;
            m_DesiredPosition = newPosition;
            m_IsSphericalMovement = false;
        }
        
        void OnTeleport()
        {
            m_DesiredLookAt = m_DesiredRotation * new Vector3(0.0f, 0.0f, (m_DesiredLookAt - m_DesiredPosition).magnitude) + m_DesiredPosition;
        }
    }
}

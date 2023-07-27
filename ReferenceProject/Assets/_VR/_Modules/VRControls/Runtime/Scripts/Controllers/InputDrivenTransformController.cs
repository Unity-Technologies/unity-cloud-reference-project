using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    /// Specifies how input values should move or rotate the local X, Y, or Z position and rotation.
    /// Uses input action references and defined ranges to drive transforms.
    /// </summary>
    public class InputDrivenTransformController : MonoBehaviour
    {
        class TransformInfo
        {
            internal Vector3 InitialPosition;
            internal Vector3 InitialRotation;
            internal Vector3 PositionOffset;
            internal Vector3 RotationOffset;

            internal void Apply(Transform transform)
            {
                transform.localPosition = InitialPosition + PositionOffset;
                transform.localRotation = Quaternion.Euler(InitialRotation + RotationOffset);
            }
        }

        [Serializable]
        class InputDrivenAxis
        {
            [SerializeField, Tooltip("The input action whose value drives the axis.")]
            internal InputActionReference InputActionReference;

            [SerializeField, Tooltip("The specific transform axis and range of movement to drive.")]
            internal TransformAxisRange AxisRange;

            [SerializeField, Tooltip("The specific transform axis and range of movement to drive.")]
            internal Transform Transform;

            [SerializeField, Tooltip("If enabled, the range minimum and maximum will be multiplied by -1.")]
            internal bool MirrorRange;

            public bool IsValid
            {
                get
                {
                    return InputActionReference != null &&
                        AxisRange != null &&
                        Transform != null;
                }
            }
        }

        [SerializeField, Tooltip("The input actions, axis ranges, and transforms that to be controlled.")]
        List<InputDrivenAxis> m_InputDrivenPose = new();

        // Dictionary is keyed by transform so that multiple driven axes can be applied simultaneously
        readonly Dictionary<Transform, TransformInfo> m_TransformInfos = new();

        void OnEnable()
        {
            StoreInitialTransformValues();
            InputSystem.onAfterUpdate += ProcessAffordances;
        }

        void OnDisable()
        {
            InputSystem.onAfterUpdate -= ProcessAffordances;
            ResetTransformOffsets();
            ApplyTransformOffsets();
        }

        void StoreInitialTransformValues()
        {
            var count = m_InputDrivenPose.Count;
            for (var i = 0; i < count; i++)
            {
                var drivenTransform = m_InputDrivenPose[i];
                if (!drivenTransform.IsValid)
                    continue;

                var transformToAnimate = drivenTransform.Transform;
                drivenTransform.InputActionReference.action.Enable();

                if (!m_TransformInfos.TryGetValue(transformToAnimate, out var info))
                {
                    info = new TransformInfo();
                    m_TransformInfos[transformToAnimate] = info;
                }

                info.InitialPosition = transformToAnimate.localPosition;
                info.InitialRotation = transformToAnimate.localRotation.eulerAngles;
            }
        }

        void ProcessAffordances()
        {
            ResetTransformOffsets();
            UpdateOffsetsForCurrentInput();
            ApplyTransformOffsets();
        }

        void UpdateOffsetsForCurrentInput()
        {
            foreach (var drivenTransform in m_InputDrivenPose)
            {
                var actionValue = drivenTransform.InputActionReference.action.ReadValue<float>();
                var transformToAnimate = drivenTransform.Transform;
                var definition = drivenTransform.AxisRange;
                var info = m_TransformInfos[transformToAnimate];
                var mirror = drivenTransform.MirrorRange ? -1 : 1;
                var min = definition.MinimumOutputValue * mirror;
                var max = definition.MaximumOutputValue * mirror;
                var offset = min + actionValue * (max - min);
                info.PositionOffset += definition.TranslateAxes.GetAxis() * offset;
                info.RotationOffset += definition.RotateAxes.GetAxis() * offset;
            }
        }

        void ApplyTransformOffsets()
        {
            foreach (var keyValuePair in m_TransformInfos)
            {
                keyValuePair.Value.Apply(keyValuePair.Key);
            }
        }

        void ResetTransformOffsets()
        {
            foreach (var value in m_TransformInfos.Values)
            {
                var transformInfo = value;
                transformInfo.PositionOffset = Vector3.zero;
                transformInfo.RotationOffset = Vector3.zero;
            }
        }
    }
}

using System;
using Unity.Cloud.Annotation;
using Unity.Cloud.Annotation.Runtime;
using Unity.ReferenceProject.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Unity.ReferenceProject.Annotation
{
    public class AnnotationIndicatorController : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField]
        Color m_SelectedBackgroundColor;
        [SerializeField]
        Color m_SelectedMiddleColor;

        [SerializeField]
        Color m_UnSelectedBackgroundColor;
        [SerializeField]
        Color m_UnSelectedMiddleColor;

        [SerializeField]
        ColorPalette m_ColorPalette;

        Material Material
        {
            get
            {
                if (m_Material == null)
                {
                    var renderer = GetComponentInChildren<Renderer>();
                    if (renderer == null)
                    {
                        Debug.Log($"Cannot find Renderer component on {GetType().Name}");
                        return null;
                    }

                    m_Material = renderer.material;
                }

                return m_Material;
            }
        }
        Material m_Material;

        // Indicator positionning
        Vector3 m_WorldPosition;
        Vector3? m_RelativePosition;

        // Camera positionning
        Vector3 m_CameraWorldPosition;
        Quaternion m_CameraWorldRotation;
        Vector3? m_CameraRelativePosition;
        Quaternion? m_CameraRelativeRotation;

        bool m_IsSelected;
        ITopic m_Topic;

        static readonly int k_IdBackgroundColor = Shader.PropertyToID("_BackgroundColor");
        static readonly int k_IdMiddleColor = Shader.PropertyToID("_MiddleColor");
        static readonly int k_IdDepthAvailable = Shader.PropertyToID("_DepthAvailable");

        public ITopic Topic
        {
            get
            {
                return m_Topic;
            }
            set
            {
                OnTopicChanged(value);
            }
        }

        public bool Initialized => m_Topic != null;

        void Awake()
        {
            OnSelectedChanged();

            bool isDepthTextureAvailable = false;

            var renderPipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            isDepthTextureAvailable = renderPipeline.supportsCameraDepthTexture;

            Material.SetFloat(k_IdDepthAvailable, isDepthTextureAvailable ? 1 : 0);
        }

        void LateUpdate()
        {
            if (!Initialized) return;

            SetPosition();
        }

        public void Initialize(ITopic topic)
        {
            if (Initialized)
            {
                Debug.LogWarning($"Double initialization in {GetType().Name}");
            }

            m_Topic = topic;

            OnTopicChanged(m_Topic);
        }

        public void Reset()
        {
            m_Topic = null;
            m_IsSelected = false;
            m_CameraRelativePosition = null;
            m_CameraRelativeRotation = null;
            m_CameraWorldPosition = Vector3.zero;
            m_CameraWorldRotation = Quaternion.identity;
            m_WorldPosition = Vector3.zero;
            m_RelativePosition = null;
        }

        public void SetSelected(bool selected)
        {
            if (m_IsSelected != selected)
            {
                m_IsSelected = selected;
                OnSelectedChanged();
            }
        }

        public (Vector3, Quaternion) ComputeCameraPositionAndRotation()
        {
            if (m_CameraRelativePosition.HasValue && m_CameraRelativeRotation.HasValue)
            {
                return (m_CameraRelativePosition.Value, m_CameraRelativeRotation.Value);
            }
            else
            {
                return (m_CameraWorldPosition, m_CameraWorldRotation);
            }
        }

        void OnTopicChanged(ITopic newTopic)
        {
            m_WorldPosition = newTopic.WorldTransform.Position.ToVector3();
            var worldRotation = Topic.WorldTransform.Rotation.ToQuaternion();

            m_Topic = newTopic;

            var worldIndicatorGo = new GameObject("Indicator World (read)");
            worldIndicatorGo.transform.parent = gameObject.transform.parent;
            worldIndicatorGo.transform.position = m_WorldPosition;
            worldIndicatorGo.transform.rotation = worldRotation;
            worldIndicatorGo.transform.localScale = Vector3.one;

            var worldLocalCameraGo = new GameObject("Camera (read)");
            worldLocalCameraGo.transform.parent = worldIndicatorGo.transform;
            worldLocalCameraGo.transform.position = Topic.WorldCameraTransform.Position.ToVector3();
            worldLocalCameraGo.transform.rotation = Topic.WorldCameraTransform.Rotation.ToQuaternion();
            worldLocalCameraGo.transform.localScale = Vector3.one;

            m_CameraWorldPosition = worldLocalCameraGo.transform.position;
            m_CameraWorldRotation = worldLocalCameraGo.transform.rotation;

            SetPosition();
            SetUnselectedColor();
            UpdateColor(m_UnSelectedBackgroundColor, m_UnSelectedMiddleColor);

            Destroy(worldLocalCameraGo);
            Destroy(worldIndicatorGo);
        }

        void SetUnselectedColor()
        {
            if (m_Topic != null && m_Topic.CreationAuthor != null)
            {
                m_UnSelectedBackgroundColor = m_ColorPalette.GetColor(m_Topic.CreationAuthor.ColorIndex);
            }
        }

        void OnSelectedChanged()
        {
            var backColor = m_IsSelected ? m_SelectedBackgroundColor : m_UnSelectedBackgroundColor;
            var midColor = m_IsSelected ? m_SelectedMiddleColor : m_UnSelectedMiddleColor;

            UpdateColor(backColor, midColor);
        }

        void UpdateColor(Color backgroundColor, Color middleColor)
        {
            Material.SetColor(k_IdBackgroundColor, backgroundColor);
            Material.SetColor(k_IdMiddleColor, middleColor);
        }

        void SetPosition()
        {
            if (m_RelativePosition.HasValue)
            {
                // If we are relative to an object, we put this position to world space and we use that
                // world space. That will ensure we follow the given object.
                transform.position = m_RelativePosition.Value;
            }
            else
            {
                // If we are not in relative position, we take the world space but treat it as local so that
                // if our world was transformed, we follow this transform
                transform.localPosition = m_WorldPosition;
            }
        }
    }
}

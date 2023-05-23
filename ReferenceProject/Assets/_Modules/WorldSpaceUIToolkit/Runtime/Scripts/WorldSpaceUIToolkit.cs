using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(TrackedDevicePhysicsRaycaster))]
    public class WorldSpaceUIToolkit : MonoBehaviour, IPointerMoveHandler, IPointerUpHandler, IPointerDownHandler,
        ISubmitHandler, ICancelHandler, IMoveHandler, IScrollHandler, ISelectHandler, IDeselectHandler, IDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        public delegate void InitializeDocument(UIDocument document);

        [Header("World Space Size Values")]
        [Tooltip("Width of the panel in pixels. The RenderTexture used to render the panel will have this width.")]
        [SerializeField]
        protected int m_PanelWidth = 1280;

        [Tooltip("Height of the panel in pixels. The RenderTexture used to render the panel will have this height.")]
        [SerializeField]
        protected int m_PanelHeight = 720;

        [Tooltip("Scale of the panel. It is like the zoom in a browser.")]
        [SerializeField]
        protected float m_PanelScale = 1.0f;

        [Tooltip("Pixels per world units, it will the determine the real panel size in the world based on panel pixel width and height.")]
        [SerializeField]
        protected float m_PixelsPerUnit = 1000.0f;

        [Space]
        [Header("UI Toolkit Document Values")]
        [Tooltip("Visual tree element object of this panel.")]
        [SerializeField]
        VisualTreeAsset m_VisualTreeAsset;

        [Tooltip("PanelSettings that will be used to create a new instance for this panel.")]
        [SerializeField]
        PanelSettings m_PanelSettingsPrefab;

        [Tooltip("RenderTexture that will be used to create a new instance for this panel.")]
        [SerializeField]
        protected RenderTexture m_RenderTexturePrefab;

        [Tooltip("Some input modules (like the XRUIInputModule from the XR Interaction toolkit package) doesn't send PointerMove events. If you are using such an input module, just set this to true so at least you can properly drag things around.")]
        [SerializeField]
        protected bool m_UseDragEventFix;
        protected readonly HashSet<(BaseEventData, int)> m_EventsProcessedInThisFrame = new HashSet<(BaseEventData, int)>();
        protected Material m_ActiveMaterial;
        protected BoxCollider m_BoxCollider;
        protected MeshFilter m_MeshFilter;

        protected MeshRenderer m_MeshRenderer;
        protected RenderTexture m_OutputTexture;
        protected PanelEventHandler m_PanelEventHandler;
        protected PanelSettings m_PanelSettings;

        PhysicsRaycaster m_Raycaster;
        protected UIDocument m_UIDocument;

        public bool UseDragEventFix => m_UseDragEventFix;

        public Vector2 PanelSize
        {
            get => new Vector2(m_PanelWidth, m_PanelHeight);
            set
            {
                m_PanelWidth = Mathf.RoundToInt(value.x);
                m_PanelHeight = Mathf.RoundToInt(value.y);
                RefreshPanelSize();
            }
        }

        public float PanelScale
        {
            get => m_PanelScale;
            set
            {
                m_PanelScale = value;

                if (m_PanelSettings != null)
                    m_PanelSettings.scale = value;
            }
        }

        public VisualTreeAsset VisualTreeAsset
        {
            get => m_VisualTreeAsset;
            set
            {
                m_VisualTreeAsset = value;

                if (m_UIDocument != null)
                    m_UIDocument.visualTreeAsset = value;
            }
        }

        public VisualElement RootVisualElement
        {
            get
            {
                if (m_UIDocument != null)
                {
                    return m_UIDocument.rootVisualElement;
                }

                return null;
            }
        }

        public int PanelWidth
        {
            get => m_PanelWidth;
            set
            {
                m_PanelWidth = value;
                RefreshPanelSize();
            }
        }

        public int PanelHeight
        {
            get => m_PanelHeight;
            set
            {
                m_PanelHeight = value;
                RefreshPanelSize();
            }
        }

        public float PixelsPerUnit
        {
            get => m_PixelsPerUnit;
            set
            {
                m_PixelsPerUnit = value;
                RefreshPanelSize();
            }
        }

        public PanelSettings PanelSettingsPrefab
        {
            get => m_PanelSettingsPrefab;
            set
            {
                m_PanelSettingsPrefab = value;
                RebuildPanel();
            }
        }

        public RenderTexture RenderTexturePrefab
        {
            get => m_RenderTexturePrefab;
            set
            {
                m_RenderTexturePrefab = value;
                RebuildPanel();
            }
        }

        public Material OpaqueMaterial { get; set; }

        public Material TransparentMaterial { get; set; }

        void Awake()
        {
            PixelsPerUnit = m_PixelsPerUnit;
            SetReferences();
        }

        void Reset()
        {
            SetReferences();

            m_MeshRenderer.sharedMaterial = null;
            m_MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            m_MeshRenderer.receiveShadows = false;
            m_MeshRenderer.allowOcclusionWhenDynamic = false;
            m_MeshRenderer.lightProbeUsage = LightProbeUsage.Off;
            m_MeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            m_MeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

            var size = m_BoxCollider.size;
            size.z = 0;
            m_BoxCollider.size = size;

            var quadGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_MeshFilter.sharedMesh = quadGo.GetComponent<MeshFilter>().sharedMesh;
#if UNITY_EDITOR
            DestroyImmediate(quadGo);
#else
            Destroy(quadGo);
#endif
        }

        void Start()
        {
            RebuildPanel();
        }

        void LateUpdate()
        {
            m_EventsProcessedInThisFrame.Clear();
        }

        void OnEnable()
        {
            m_PanelEventHandler = FoundPanelEventHandler(m_UIDocument);
        }

        void OnDestroy()
        {
            DestroyGeneratedAssets();
        }

        /// <summary>
        ///     Provides a Visual of the panel that will be instanced once the application enters runtime. The Cyan frame marks the
        ///     forward
        ///     It accurately reflects the UI size, since LocalScale is not used to calculate it
        /// </summary>
        void OnDrawGizmos()
        {
            var rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_PanelWidth / PixelsPerUnit, m_PanelHeight / PixelsPerUnit, 0.1f));
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(-Vector3.forward * 0.05f, new Vector3(m_PanelWidth / PixelsPerUnit, m_PanelHeight / PixelsPerUnit, 0.01f));
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying && m_ActiveMaterial != null && m_UIDocument != null)
            {
                if (m_UIDocument.visualTreeAsset != m_VisualTreeAsset)
                    VisualTreeAsset = m_VisualTreeAsset;
                if (m_PanelScale != m_PanelSettings.scale)
                    m_PanelSettings.scale = m_PanelScale;

                RefreshPanelSize();
            }
        }
#endif

        public void OnCancel(BaseEventData eventData)
        {
            m_PanelEventHandler?.OnCancel(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            m_PanelEventHandler?.OnDeselect(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (UseDragEventFix)
                OnPointerMove(eventData);
        }

        public void OnMove(AxisEventData eventData)
        {
            m_PanelEventHandler?.OnMove(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            TransformPointerEventForUIToolkit(eventData);
            m_PanelEventHandler?.OnPointerDown(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TransformPointerEventForUIToolkit(eventData);
            m_PanelEventHandler?.OnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TransformPointerEventForUIToolkit(eventData);
            m_PanelEventHandler?.OnPointerExit(eventData);
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            TransformPointerEventForUIToolkit(eventData);
            m_PanelEventHandler?.OnPointerMove(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            TransformPointerEventForUIToolkit(eventData);
            m_PanelEventHandler?.OnPointerUp(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            TransformPointerEventForUIToolkit(eventData);
            m_PanelEventHandler?.OnScroll(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            m_PanelEventHandler?.OnSelect(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            m_PanelEventHandler?.OnSubmit(eventData);
        }

        public event InitializeDocument OnPanelBuilt;

        static PanelEventHandler FoundPanelEventHandler(UIDocument uiDocument)
        {
            if (uiDocument == null)
                return null;

            // find the automatically generated PanelEventHandler and PanelRaycaster for this panel and disable the raycaster
            var handlers = FindObjectsOfType<PanelEventHandler>();

            foreach (var handler in handlers)
            {
                if (handler.panel == uiDocument.rootVisualElement.panel)
                {
                    var panelRaycaster = handler.GetComponent<PanelRaycaster>();
                    if (panelRaycaster != null)
                        panelRaycaster.enabled = false;

                    return handler;
                }
            }

            return null;
        }

        /// <summary>
        ///     Use this method to initialise the panel without triggering a rebuild (i.e.: when instantiating it from scripts).
        ///     Start method
        ///     will always trigger RebuildPanel(), but if you are calling this after the GameObject started you must call
        ///     RebuildPanel() so the
        ///     changes take effect.
        /// </summary>
        public void InitPanel(int panelWidth,
            int panelHeight,
            float panelScale,
            float pixelsPerUnit,
            VisualTreeAsset visualTreeAsset,
            PanelSettings panelSettingsPrefab,
            RenderTexture renderTexturePrefab)
        {
            m_PanelWidth = panelWidth;
            m_PanelHeight = panelHeight;
            m_PanelScale = panelScale;
            m_PixelsPerUnit = pixelsPerUnit;
            m_VisualTreeAsset = visualTreeAsset;
            m_PanelSettingsPrefab = panelSettingsPrefab;
            m_OutputTexture = renderTexturePrefab;
        }

        /// <summary>
        ///     Rebuilds the panel by destroy current assets and generating new ones based on the configuration.
        /// </summary>
        public void RebuildPanel()
        {
            DestroyGeneratedAssets();

            // generate render texture
            var textureDescriptor = m_RenderTexturePrefab.descriptor;
            textureDescriptor.width = m_PanelWidth;
            textureDescriptor.height = m_PanelHeight;
            m_OutputTexture = new RenderTexture(textureDescriptor);

            // generate panel settings
            m_PanelSettings = Instantiate(m_PanelSettingsPrefab);
            m_PanelSettings.targetTexture = m_OutputTexture;
            m_PanelSettings.clearColor = true; // ConstantPixelSize and clearColor are mandatory configs
            m_PanelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
            m_PanelSettings.scale = m_PanelScale;
            m_OutputTexture.name = $"{name} - RenderTexture";
            m_PanelSettings.name = $"{name} - PanelSettings";

            // generate UIDocument
            m_UIDocument = gameObject.AddComponent<UIDocument>();
            m_UIDocument.panelSettings = m_PanelSettings;
            m_UIDocument.visualTreeAsset = m_VisualTreeAsset;

            SetActiveMaterial();

            RefreshPanelSize();

            m_PanelEventHandler = FoundPanelEventHandler(m_UIDocument);

            OnPanelBuilt?.Invoke(m_UIDocument);
        }

        protected void RefreshPanelSize()
        {
            if (m_OutputTexture != null && (m_OutputTexture.width != m_PanelWidth || m_OutputTexture.height != m_PanelHeight))
            {
                m_OutputTexture.Release();
                m_OutputTexture.width = m_PanelWidth;
                m_OutputTexture.height = m_PanelHeight;
                m_OutputTexture.Create();

                if (m_UIDocument != null)
                    m_UIDocument.rootVisualElement?.MarkDirtyRepaint();
            }

            transform.localScale = new Vector3(m_PanelWidth / m_PixelsPerUnit, m_PanelHeight / m_PixelsPerUnit, 1.0f);
        }

        protected void DestroyGeneratedAssets()
        {
            if (m_UIDocument) Destroy(m_UIDocument);
            if (m_OutputTexture) Destroy(m_OutputTexture);
            if (m_PanelSettings) Destroy(m_PanelSettings);
            if (m_ActiveMaterial) Destroy(m_ActiveMaterial);
        }

        void SetReferences()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_MeshFilter = GetComponent<MeshFilter>();
            m_BoxCollider = GetComponent<BoxCollider>();
        }

        void SetActiveMaterial()
        {
            // decide on transparent or opaque material, and check if a preset is given, otherwise generate a material
            if (m_PanelSettings.colorClearValue.a < 1.0f)
            {
                m_ActiveMaterial = TransparentMaterial != null ? new Material(TransparentMaterial) : GenerateMaterial(true);
            }
            else
            {
                m_ActiveMaterial = OpaqueMaterial != null ? new Material(OpaqueMaterial) : GenerateMaterial(false);
            }

            m_ActiveMaterial.SetTexture("_BaseMap", m_OutputTexture);
            m_MeshRenderer.sharedMaterial = m_ActiveMaterial;
        }

        Material GenerateMaterial(bool hasAlpha)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));

            if (hasAlpha)
            {
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_SrcBlend", 5f);
                mat.SetFloat("_DstBlend", 10f);
                mat.SetFloat("_ZWrite", 0f);

                mat.renderQueue = (int)RenderQueue.Transparent;
            }
            else
            {
                mat.SetFloat("_Surface", 0f);
                mat.SetFloat("_SrcBlend", 1f);
                mat.SetFloat("_DstBlend", 0f);

                mat.renderQueue = (int)RenderQueue.Geometry;
            }

            return mat;
        }

        protected void TransformPointerEventForUIToolkit(PointerEventData eventData)
        {
            var eventKey = (eventData, eventData.pointerId);

            if (!m_EventsProcessedInThisFrame.Contains(eventKey))
            {
                m_EventsProcessedInThisFrame.Add(eventKey);
                var eventCamera = eventData.enterEventCamera ?? eventData.pressEventCamera;

                if (eventCamera != null)
                {
                    // get current event position and create the ray from the event camera
                    Vector3 position = eventData.position;
                    position.z = 1.0f;
                    position = eventCamera.ScreenToWorldPoint(position);
                    var panelPlane = new Plane(transform.forward, transform.position);
                    var ray = new Ray(eventCamera.transform.position, position - eventCamera.transform.position);

                    if (panelPlane.Raycast(ray, out var distance))
                    {
                        // get local pointer position within the panel
                        position = ray.origin + distance * ray.direction.normalized;
                        position = transform.InverseTransformPoint(position);

                        // compute a fake pointer screen position so it results in the proper panel position when projected from the camera by the PanelEventHandler
                        position.x += 0.5f;
                        position.y -= 0.5f;
                        position = Vector3.Scale(position, new Vector3(m_PanelWidth, m_PanelHeight, 1.0f));
                        position.y += Screen.height;

                        // update the event data with the new calculated position
                        eventData.position = position;
                        var raycastResult = eventData.pointerCurrentRaycast;
                        raycastResult.screenPosition = position;
                        eventData.pointerCurrentRaycast = raycastResult;
                        raycastResult = eventData.pointerPressRaycast;
                        raycastResult.screenPosition = position;
                        eventData.pointerPressRaycast = raycastResult;
                    }
                }
            }
        }
    }
}

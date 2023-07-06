using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.WorldSpaceUIDocumentExtensions
{
    [RequireComponent(typeof(UIDocument))]
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(WorldSpaceUIDocument))]
    public class WorldSpaceUIDocumentSize : MonoBehaviour
    {
        [SerializeField]
        Vector2 m_Size = new Vector2(1, 1);

        [Tooltip("Pixels per world units, it will determine the real panel size in the world based on panel pixel width and height.")]
        [SerializeField]
        float m_PixelsPerUnit = 1000.0f;

        public Vector2 Size
        {
            get => m_Size;
            set
            {
                m_Size = value;
                UpdateSize();
            }
        }

        public float PixelsPerUnit => m_PixelsPerUnit;

        RenderTexture m_RenderTexture;
        PanelSettings m_PanelSettings;
        WorldSpaceUIDocument m_WorldSpaceUIDocument;
        Renderer m_Renderer;
        RenderTextureDescriptor m_TextureDescriptor;

        void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();
            m_Renderer = GetComponent<Renderer>();
            var worldSpaceUIDocument = GetComponent<WorldSpaceUIDocument>();
            m_TextureDescriptor = uiDocument.panelSettings.targetTexture.descriptor;

            // Make a copy of the settings.
            m_PanelSettings = Instantiate(uiDocument.panelSettings);
            m_PanelSettings.clearColor = true; // clearColor are mandatory configs
            uiDocument.panelSettings = m_PanelSettings;

            // WorldSpaceUIDocument need to be re-enabled to make sure the custom function is applied on the right PanelSettings.
            worldSpaceUIDocument.enabled = false;
            worldSpaceUIDocument.enabled = true;
        }

        void OnEnable()
        {
            UpdateSize();
        }

        void OnDisable()
        {
            m_RenderTexture.Release();
        }

        void OnDestroy()
        {
            Destroy(m_RenderTexture);
            Destroy(m_PanelSettings);
        }

        void UpdateSize()
        {
            if (m_RenderTexture != null)
            {
                Destroy(m_RenderTexture);
            }

            // We need a instance of the RenderTexture for each WorldSpaceUIDocument.
            m_TextureDescriptor.width = Mathf.FloorToInt(m_Size.x);
            m_TextureDescriptor.height = Mathf.FloorToInt(m_Size.y);
            m_RenderTexture = new RenderTexture(m_TextureDescriptor);

            m_PanelSettings.targetTexture = m_RenderTexture;

            // Make sure to assign the texture to all entity that requires it.
            foreach (var material in m_Renderer.materials)
            {
                material.mainTexture = m_RenderTexture;
            }

            transform.localScale = new Vector3(m_Size.x / m_PixelsPerUnit, m_Size.y / m_PixelsPerUnit, 1f);
        }
    }
}

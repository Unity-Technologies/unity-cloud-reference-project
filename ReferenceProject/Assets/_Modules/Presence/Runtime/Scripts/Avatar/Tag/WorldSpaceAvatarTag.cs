using System;
using UnityEngine;
using UnityEngine.Dt.App.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public class WorldSpaceAvatarTag : AvatarTag
    {
        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        Renderer m_Renderer;

        [SerializeField]
        float m_ShowNameDistance = 20.0f;

        [Header("UXML")]
        [SerializeField]
        string m_NameContainerElement = "avatar-tag-name-container";
        
        [SerializeField]
        string m_HeaderElement = "avatar-tag-name";
        
        [SerializeField]
        string m_AvatarBadgeElement = "avatar-tag-badge";

        VisualElement m_NameContainer;
        Header m_Header;
        UnityEngine.Dt.App.UI.Avatar m_AvatarBadge;

        Camera m_Camera;
        RenderTexture m_RenderTexture;

        [Inject]
        void Setup(Camera streamingCamera)
        {
            m_Camera = streamingCamera;
        }

        void Awake()
        {
            // We need a instance of the RenderTexture for each Tag.
            // Make sure to assign the texture to all entity that requires it.
            SetupRenderTexture();

            var root = m_UIDocument.rootVisualElement;
            m_NameContainer = root.Q<VisualElement>(m_NameContainerElement);
            m_Header = root.Q<Header>(m_HeaderElement);
            m_AvatarBadge = root.Q<UnityEngine.Dt.App.UI.Avatar>(m_AvatarBadgeElement);
        }

        void SetupRenderTexture()
        {
            // We need a instance of the RenderTexture for each Tag.
            // Make sure to assign the texture to all entity that requires it.
            m_UIDocument.panelSettings = Instantiate(m_UIDocument.panelSettings); // Make a copy of the settings.

            m_RenderTexture = Instantiate(m_UIDocument.panelSettings.targetTexture); // Make a copy of the renderTexture.
            m_UIDocument.panelSettings.targetTexture = m_RenderTexture;

            foreach (var material in m_Renderer.materials)
            {
                material.mainTexture = m_RenderTexture;
            }
        }

        void Update()
        {
            if ((transform.position - m_Camera.transform.position).sqrMagnitude <= m_ShowNameDistance * m_ShowNameDistance)
            {
                m_NameContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                m_NameContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        }

        void OnDestroy()
        {
            Destroy(m_RenderTexture);
        }

        public override void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        public override void SetName(string tagName)
        {
            m_Header.text = tagName;
            m_AvatarBadge.text = AvatarUtils.GetInitials(tagName);
        }

        public override void SetColor(Color color)
        {
            m_AvatarBadge.backgroundColor = color;
        }
    }
}

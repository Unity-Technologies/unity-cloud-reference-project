using System;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.ReferenceProject.Common;
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

        [SerializeField]
        string m_AvatarBadgeTextElement = "avatar-tag-badge-text";

        VisualElement m_NameContainer;
        Heading m_Header;
        Unity.AppUI.UI.Avatar m_AvatarBadge;
        Unity.AppUI.UI.Text m_AvatarInitials;

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
            m_Header = root.Q<Heading>(m_HeaderElement);
            m_AvatarBadge = root.Q<Unity.AppUI.UI.Avatar>(m_AvatarBadgeElement);
            m_AvatarInitials = root.Q<Unity.AppUI.UI.Text>(m_AvatarBadgeTextElement);
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
            m_AvatarInitials.text = Utils.GetInitials(tagName);
        }

        public override void SetColor(Color color)
        {
            m_AvatarBadge.backgroundColor = color;
        }
    }
}

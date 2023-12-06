using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.Tools;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Presence;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.Identity
{
    public class ProfileToolUIController : ToolUIController
    {
        [SerializeField]
        AppState m_LogoutState;

        [Header("UXML")]
        [SerializeField]
        string m_UserNameHeaderElement = "profile-tool-username";

        [SerializeField]
        string m_LogoutButtonElement = "profile-tool-button";

        [SerializeField]
        ColorPalette m_AvatarColorPalette;

        IAppStateController m_AppStateController;
        ICloudSession m_Session;

        Heading m_UserName;
        Button m_Button;
        AvatarBadge m_Badge;

        public AppState LogoutState
        {
            get => m_LogoutState;
            set => m_LogoutState = value;
        }

        [Inject]
        void Setup(ICloudSession session, IAppStateController appStateController)
        {
            m_Session = session;
            m_AppStateController = appStateController;
        }

        void OnEnable()
        {
            m_Session.SessionStateChanged += OnSessionStateChanged;
            OnSessionStateChanged(m_Session.State);
        }

        void OnDisable()
        {
            m_Session.SessionStateChanged -= OnSessionStateChanged;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);

            m_UserName = root.Q<Heading>(m_UserNameHeaderElement);
            m_Button = root.Q<Button>(m_LogoutButtonElement);

            m_Button.clickable.clicked += OnLogoutClicked;

            OnSessionStateChanged(m_Session.State);

            return root;
        }

        void OnLogoutClicked()
        {
            Logout();
        }

        void Logout()
        {
            CloseSelf();
            m_AppStateController.PrepareTransition(m_LogoutState).Apply();
        }

        void OnSessionStateChanged(SessionState state)
        {
            if (state == SessionState.LoggedIn)
            {
                if (m_UserName != null)
                {
                    m_UserName.text = m_Session.UserData.Name;
                }

                UpdateButtonContent(true);

                m_Button?.SetEnabled(true);
                m_Session.UserData.ColorChanged -= OnBadgeColorChanged;
                m_Session.UserData.ColorChanged += OnBadgeColorChanged;
            }
            else
            {
                if (m_UserName != null)
                    m_UserName.text = "-";

                UpdateButtonContent(false);

                m_Button?.SetEnabled(false);
            }
        }

        void OnBadgeColorChanged(Color newColor)
        {
            UpdateBadgeColor(newColor);
        }

        public override VisualElement GetButtonContent()
        {
            m_Badge = new AvatarBadge();

            if (m_UserName != null && !string.IsNullOrEmpty(m_UserName.text))
            {
                m_Badge.Initials.text = Utils.GetInitials(m_UserName.text);
            }
            else
            {
                m_Badge.Initials.text = "-";
            }

            m_Badge.backgroundColor = Color.gray;
            m_Badge.size = Size.M;
            m_Badge.outlineColor = Color.clear;
            m_Badge.AddToClassList(m_LogoutButtonElement);

            return m_Badge;
        }

        void UpdateButtonContent(bool connected)
        {
            if (m_Badge == null)
                return;

            if (connected)
            {
                var initials = m_Badge.Q<Text>();
                initials.text = Utils.GetInitials(m_UserName.text);
            }
            else
            {
                var initials = m_Badge.Q<Text>();
                initials.text = "-";
            }
        }

        public void ResetBadgeColor()
        {
            UpdateBadgeColor(Color.grey);
        }

        public void UpdateBadgeColorIndex(int colorIndex)
        {
            if (m_Badge == null)
                return;
            
            Color color = m_AvatarColorPalette.GetColor(colorIndex);
            m_Badge.backgroundColor = color;
        }

        public void UpdateBadgeColor(Color color)
        {
            if (m_Badge == null)
                return;
            
            m_Badge.backgroundColor = color;
        }
    }
}

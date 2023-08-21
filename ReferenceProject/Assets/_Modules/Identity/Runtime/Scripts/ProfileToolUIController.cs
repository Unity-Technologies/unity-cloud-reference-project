using System;
using System.Diagnostics;
using System.Linq;
using Unity.Cloud.Identity;
using Unity.ReferenceProject.StateMachine;
using Unity.ReferenceProject.Tools;
using UnityEngine;
using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Presence;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;
using Debug = UnityEngine.Debug;

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
        IUrlRedirectionAuthenticator m_Authenticator;
        IUserInfoProvider m_UserInfoProvider;
        PresenceStreamingRoom m_PresenceStreamingRoom;

        Heading m_UserName;
        Button m_Button;
        AvatarBadge m_Badge;

        [Inject]
        void Setup(IUrlRedirectionAuthenticator authenticator, IUserInfoProvider userInfoProvider, IAppStateController appStateController, PresenceStreamingRoom streamingRoom)
        {
            m_Authenticator = authenticator;
            m_UserInfoProvider = userInfoProvider;
            m_AppStateController = appStateController;
            m_PresenceStreamingRoom = streamingRoom;
        }

        void OnEnable()
        {
            m_Authenticator.AuthenticationStateChanged += OnAuthenticationStateChanged;
            m_PresenceStreamingRoom.RoomJoined += OnRoomJoined;
            m_PresenceStreamingRoom.RoomLeft += OnRoomLeft;
            OnAuthenticationStateChanged(m_Authenticator.AuthenticationState);
        }

        void OnDisable()
        {
            m_Authenticator.AuthenticationStateChanged -= OnAuthenticationStateChanged;
        }

        protected override VisualElement CreateVisualTree(VisualTreeAsset template)
        {
            var root = base.CreateVisualTree(template);

            m_UserName = root.Q<Heading>(m_UserNameHeaderElement);
            m_Button = root.Q<Button>(m_LogoutButtonElement);

            m_Button.clickable.clicked += Logout;

            return root;
        }

        async void Logout()
        {
            await m_Authenticator.LogoutAsync();
            CloseSelf();

            m_AppStateController.PrepareTransition(m_LogoutState).Apply();
        }

        async void OnAuthenticationStateChanged(AuthenticationState obj)
        {
            if (obj == AuthenticationState.LoggedIn)
            {
                var userInfo = await m_UserInfoProvider.GetUserInfoAsync();

                if (m_UserName != null)
                    m_UserName.text = userInfo.Name;

                UpdateButtonContent(true);

                m_Button?.SetEnabled(true);
            }
            else
            {
                if (m_UserName != null)
                    m_UserName.text = "-";

                UpdateButtonContent(false);

                m_Button?.SetEnabled(false);
            }
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

        void UpdateButtonColor(Color color)
        {
            if (m_Badge == null)
                return;
            
            m_Badge.backgroundColor = color;
        }

        void OnRoomJoined(Room room)
        {
            room.ParticipantAdded += OnParticipantAdded;
            
            var owner = room.ConnectedParticipants.FirstOrDefault(p => p.IsSelf);
            if (owner == null)
            {
                return;
            }

            UpdateButtonColor(m_AvatarColorPalette.GetColor(owner.ColorIndex));
        }

        void OnParticipantAdded(IParticipant participant)
        {
            if (participant.IsSelf)
            {
                UpdateButtonColor(m_AvatarColorPalette.GetColor(participant.ColorIndex));
            }
        }

        void OnRoomLeft()
        {
            UpdateButtonColor(Color.gray);
        }
    }
}

using Unity.AppUI.UI;
using Unity.Cloud.Presence;
using Unity.ReferenceProject.Common;
using Unity.ReferenceProject.Messaging;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;
using Button = Unity.AppUI.UI.Button;

namespace Unity.ReferenceProject.Presence
{
    public class FollowModeUI : MonoBehaviour
    {
        [SerializeField]
        UIDocument m_UIDocument;

        [SerializeField]
        ColorPalette m_AvatarColorPalette;
        VisualElement m_Border;
        VisualElement m_TextBox;
        VisualElement m_BlockingLayout;
        IconButton m_Button;
        Text m_FollowedNameText;
        Text m_FollowText;
        bool m_IsPresentation = false;
        IFollowManager m_FollowManager;
        IAppMessaging m_AppMessaging;
        PresentationManager m_PresentationManager;
        
        [Inject]
        public void Setup(IFollowManager followManager,IAppMessaging appMessaging,
            PresentationManager presentationManager)
        {
            m_FollowManager = followManager;
            m_AppMessaging = appMessaging;
            m_PresentationManager = presentationManager;
        }
        
        void Awake()
        {
            var uiDocument = GetComponent<UIDocument>();

            if (uiDocument != null)
            {
                InitUIToolkit(uiDocument.rootVisualElement);
            }

            m_FollowManager.EnterFollowMode += OnEnterFollowMode;
            m_FollowManager.ExitFollowMode += OnExitFollowMode;
            m_FollowManager.ChangeFollowTarget += OnChangeFollowTarget;
        }

        private void OnDestroy()
        {
            if(m_FollowManager != null)
            {
                m_FollowManager.EnterFollowMode -= OnEnterFollowMode;
                m_FollowManager.ExitFollowMode -= OnExitFollowMode;
                m_FollowManager.ChangeFollowTarget -= OnChangeFollowTarget;
            }
        }

        void OnChangeFollowTarget(IParticipant participant)
        {
            UpdateUI(participant);
        }

        void OnEnterFollowMode(IParticipant participant, bool isPresentation)
        {
            m_IsPresentation = isPresentation;
            UpdateUI(participant);
            m_UIDocument.rootVisualElement.BringToFront();
        }

        void OnExitFollowMode()
        {
            Utils.SetVisible(m_Border, false);
        }
        
        void InitUIToolkit(VisualElement rootVisualElement)
        {
            m_BlockingLayout = rootVisualElement.Q("BlockingLayout");
            m_Border = rootVisualElement.Q("Border");
            m_TextBox = rootVisualElement.Q("TextBox");
            m_FollowedNameText = m_TextBox.Q<Text>("FollowedName");
            m_FollowText = rootVisualElement.Q<Text>("FollowText");
            m_Button = rootVisualElement.Q<IconButton>("Button");
            m_Button.icon = "x";
            m_Button.size = Size.M;
            m_Button.quiet = true;
            m_Button.clickable.clicked += () => ExitFollowMode();
            Utils.SetVisible(m_Border, false);
            m_Border.RegisterCallback<MouseDownEvent>(evt => OnScreenClicked(evt));
            m_UIDocument.rootVisualElement.Add(m_Border);
        }
        
        void OnScreenClicked(MouseDownEvent evt )
        {
            if (!evt.target.Equals(m_Button)) 
            {
                if (m_IsPresentation)
                {
                    m_AppMessaging.ShowDialog("@Presence:WarningTitlePresentation",
                        "@Presence:WarningMessagePresentation",
                        "@Presence:Close",
                        null,
                        "@Presence:QuitPresentation",
                        ExitPresentation); 
                }
                else
                {
                    m_AppMessaging.ShowDialog("@Presence:WarningTitleFollowMode",
                        "@Presence:WarningMessageFollowMode",
                        "@Presence:Close",
                        null,
                        "@Presence:Unfollow",
                        ExitFollowMode); 
                }
            }
            evt.StopPropagation();
        }

        void ExitPresentation()
        {
            m_PresentationManager.ExitPresentation();
        }

        void ExitFollowMode()
        {
            m_FollowManager.StopFollowMode();
        }

        void UpdateUI(IParticipant participant)
        {
            Utils.SetVisible(m_Border, true);
            m_TextBox.style.backgroundColor = m_Border.style.borderBottomColor = m_Border.style.borderTopColor =
                m_Border.style.borderRightColor = m_Border.style.borderLeftColor =
                    m_AvatarColorPalette.GetColor(participant.ColorIndex);
            m_FollowText.text = "@Presence:Following";
            m_FollowedNameText.text = participant.Name;
            m_Button.tooltip = m_IsPresentation ? "@Presence:QuitPresentation" : "@Presence:Unfollow";
        }
    }
}

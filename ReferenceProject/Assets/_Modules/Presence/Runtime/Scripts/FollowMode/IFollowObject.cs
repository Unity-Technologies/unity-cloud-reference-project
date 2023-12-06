using System;
using System.Linq;
using Unity.Cloud.Presence;
using Unity.Cloud.Presence.Runtime;
using Unity.ReferenceProject.Presence;
using UnityEngine;
using Zenject;

namespace Unity.ReferenceProject
{
    public interface IFollowObject: IDisposable
    {
        Transform GetFollowingObjectTransform();
        bool IsFollowing();
        void OnEnterFollowMode(IParticipant participant, bool isPresentation);
        void OnExitFollowMode();
    }

    public sealed class FollowObject : IFollowObject
    {
        Transform m_FollowingObject;
        bool m_IsFollowing = false;
        NetcodeParticipantManager m_NetcodeParticipantManager;
        IFollowManager m_FollowManager;

        [Inject]
        public void Setup(IFollowManager followManager, NetcodeParticipantManager netcodeParticipantManager)
        {
            m_NetcodeParticipantManager = netcodeParticipantManager;
            m_FollowManager = followManager;
            m_FollowManager.EnterFollowMode += OnEnterFollowMode;
            m_FollowManager.ExitFollowMode += OnExitFollowMode;
            m_FollowManager.ChangeFollowTarget += OnChangeFollowTarget;
        }
      
        void OnChangeFollowTarget(IParticipant participant)
        {
            m_FollowingObject = GetNetcodeParticipant(participant)?.Transform;
        }

        public void OnEnterFollowMode(IParticipant participant, bool isPresentation)
        {
            m_IsFollowing = true;
            m_FollowingObject = GetNetcodeParticipant(participant)?.Transform;
        }

        public void OnExitFollowMode()
        {
            m_IsFollowing = false;
        } 
      
        public Transform GetFollowingObjectTransform()
        {
            return m_FollowingObject;
        }
      
        public bool IsFollowing()
        {
            return m_IsFollowing;
        }

        INetcodeParticipant GetNetcodeParticipant(IParticipant participant)
        {
            var netcodeParticipants = m_NetcodeParticipantManager.NetcodeParticipants();
            return netcodeParticipants.FirstOrDefault(netcodeParticipant => netcodeParticipant.ParticipantId == participant.Id);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool dispose)
        {
            if (!dispose)
                return;

            if(m_FollowManager != null)
            {
                m_FollowManager.EnterFollowMode -= OnEnterFollowMode;
                m_FollowManager.ExitFollowMode -= OnExitFollowMode;
                m_FollowManager.ChangeFollowTarget -= OnChangeFollowTarget;
            }
        }
    }
}

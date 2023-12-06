using System;
using UnityEngine;

namespace Unity.ReferenceProject.Presence
{
    public abstract class AvatarTag : MonoBehaviour
    {
        public abstract void SetVisible(bool visible);

        public abstract void SetName(string tagName);
        
        public abstract void SetInitials(string tagInitials);

        public abstract void SetColor(Color color);
        
        public abstract void SetVoiceStatus(VoiceStatus status);
    }
}

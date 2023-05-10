using System;
using UnityEngine;

namespace Unity.ReferenceProject.Presence
{
    public abstract class AvatarModel : MonoBehaviour
    {
        public abstract void SetVisible(bool visible);

        public abstract void SetColor(Color color);
    }
}

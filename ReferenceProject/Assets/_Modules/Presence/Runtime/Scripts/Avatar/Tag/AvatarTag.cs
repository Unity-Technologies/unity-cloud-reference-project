using System;
using UnityEngine;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Zenject;

namespace Unity.ReferenceProject.Presence
{
    public abstract class AvatarTag : MonoBehaviour
    {
        public abstract void SetVisible(bool visible);

        public abstract void SetName(string tagName);

        public abstract void SetColor(Color color);
    }
}

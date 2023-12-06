using System;
using UnityEngine;

namespace Unity.ReferenceProject.Identity
{
    public interface IUserData
    {
        string Id { get; }
        string Name { get; }
        Color BadgeColor { get; }

        event Action<string> IdChanged;
        event Action<string> NameChanged;
        event Action<Color> ColorChanged;

        void UpdateBadgeColor(Color color);
    }
}

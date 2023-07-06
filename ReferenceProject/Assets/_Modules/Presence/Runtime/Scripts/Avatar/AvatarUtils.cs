using System;
using System.Collections.Generic;
using System.Text;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.ReferenceProject.Presence
{
    public class AvatarBadge : AppUI.UI.Avatar
    {
        public Text Initials { get; }

        public AvatarBadge()
        {
            Initials = new Text();
            Initials.style.color = new StyleColor(Color.white);
            Add(Initials);
        }
    }
}

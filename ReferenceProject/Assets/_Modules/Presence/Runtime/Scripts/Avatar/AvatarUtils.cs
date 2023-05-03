using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Unity.ReferenceProject.Presence
{
    public static class AvatarUtils
    {
        static readonly List<Color> k_ColorPalette = new ()
        {
            new Color(0.1254902f, 0.5882353f, 0.9529412f),
            new Color(0.89411765f, 0.13333334f, 0.3372549f),
            new Color(1.0f, 0.5137255f, 0.4392157f),
            new Color(0.99607843f, 0.78431374f, 0.3019608f),
            new Color(0.0f, 0.69411767f, 0.6901961f),
            new Color(0.43137255f, 0.20784314f, 0.38431373f),
            new Color(0.20392157f, 0.34509805f, 0.43137255f),
            new Color(0.94509804f, 0.3529412f, 0.13333334f),
            new Color(0.58431375f, 0.0f, 0.34117648f),
            new Color(0.105882354f, 0.36862746f, 0.1254902f),
            new Color(0.30980393f, 0.7647059f, 0.96862745f),
            new Color(0.9019608f, 0.31764707f, 0.0f),
            new Color(0.34117648f, 0.69803923f, 0.80784315f),
            new Color(0.7254902f, 0.050980393f, 0.15686275f),
            new Color(1.0f, 0.8392157f, 0.0f),
            new Color(0.19215687f, 0.105882354f, 0.57254905f),
            new Color(0.36078432f, 0.41960785f, 0.7529412f),
            new Color(0.67058825f, 0.2784314f, 0.7372549f),
            new Color(0.11764706f, 0.53333336f, 0.8980392f),
            new Color(0.9607843f, 0.4862745f, 0.0f),
            new Color(0.24705882f, 0.31764707f, 0.70980394f),
            new Color(0.4745098f, 0.33333334f, 0.28235295f),
            new Color(0.0f, 0.5882353f, 0.53333336f),
            new Color(0.4862745f, 0.7019608f, 0.25882354f),
            new Color(0.9137255f, 0.11764706f, 0.3882353f),
            new Color(0.61960787f, 0.61960787f, 0.61960787f)
        };

        public static string GetInitials(string value, int maxLength = 2)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var names = value.Split(' ');
            var initials = new StringBuilder();

            foreach (var n in names)
            {
                if (string.IsNullOrWhiteSpace(n))
                    continue;
                
                initials.Append(n[0]);

                if (initials.Length >= maxLength)
                    break;
            }

            return initials.ToString().ToUpper();
        }

        public static Color GetColor(int colorIndex)
        {
            return k_ColorPalette[colorIndex % k_ColorPalette.Count];
        }
    }
}

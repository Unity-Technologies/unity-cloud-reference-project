using System;
using UnityEngine;

namespace Unity.ReferenceProject.VR.VRControls
{
    /// <summary>
    /// Used to specify a combination of 3D axes
    /// </summary>
    [Flags]
    public enum Axis
    {
        /// <summary>
        /// The X axis
        /// </summary>
        X = 1 << 0,

        /// <summary>
        /// The Y axis
        /// </summary>
        Y = 1 << 1,

        /// <summary>
        /// The Z axis
        /// </summary>
        Z = 1 << 2
    }

    /// <summary>
    /// Extension methods for Axis
    /// </summary>
    public static class AxisExtensions
    {
        /// <summary>
        /// Get a Vector3 corresponding to the axis described by this Axis
        /// </summary>
        /// <param name="this">The Axis</param>
        /// <returns>The axis</returns>
        public static Vector3 GetAxis(this Axis @this)
        {
            return new Vector3(
                (@this & Axis.X) != 0 ? 1 : 0,
                (@this & Axis.Y) != 0 ? 1 : 0,
                (@this & Axis.Z) != 0 ? 1 : 0
            );
        }
    }
}
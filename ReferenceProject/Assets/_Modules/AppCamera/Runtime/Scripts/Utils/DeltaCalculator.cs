using System;
using UnityEngine;

namespace Unity.ReferenceProject.AppCamera
{
    /// <summary>
    ///     Fix to InputSystem to track a delta without its accumulation during the frame.
    ///     Not generic because generics do not support T - T, and iOS prohibits the use of dynamic keyword.
    /// </summary>
    public struct DeltaCalculator
    {
        public Vector2 delta { get; private set; }
        public Vector2 frameDelta { get; private set; }

        public void SetNewFrameDelta(Vector2 newFrameDelta)
        {
            delta = newFrameDelta - frameDelta;
            frameDelta = newFrameDelta;
        }

        public void Reset()
        {
            delta = default;
            frameDelta = default;
        }
    }
}

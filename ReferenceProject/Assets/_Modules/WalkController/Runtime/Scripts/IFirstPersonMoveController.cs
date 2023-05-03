using System;

namespace Unity.ReferenceProject.WalkController
{
    public interface IFirstPersonMoveController
    {
        bool isGrounded { get; }
        bool isRunning { get; }
        bool useGravity { get; set; }
        event Action onJump;
    }
}

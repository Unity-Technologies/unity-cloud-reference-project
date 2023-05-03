using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.ReferenceProject.WorldSpaceUIToolkit
{
    /// <summary>
    ///     Represents the state of a joystick in the Unity UI system. Keeps track of various book-keeping regarding UI
    ///     selection, and move and button states.
    /// </summary>
    struct JoystickModel
    {
        public struct ImplementationData
        {
            /// <summary>
            ///     Bookkeeping values for Unity UI that tracks the number of sequential move commands in the same direction that have
            ///     been sent.  Used to handle proper repeat timing.
            /// </summary>
            public int ConsecutiveMoveCount { get; set; }

            /// <summary>
            ///     Bookkeeping values for Unity UI that tracks the direction of the last move command.  Used to handle proper repeat
            ///     timing.
            /// </summary>
            public MoveDirection LastMoveDirection { get; set; }

            /// <summary>
            ///     Bookkeeping values for Unity UI that tracks the last time a move command was sent.  Used to handle proper repeat
            ///     timing.
            /// </summary>
            public float LastMoveTime { get; set; }

            /// <summary>
            ///     Resets this object to its default, unused state.
            /// </summary>
            public void Reset()
            {
                ConsecutiveMoveCount = 0;
                LastMoveTime = 0.0f;
                LastMoveDirection = MoveDirection.None;
            }
        }

        /// <summary>
        ///     A 2D Vector that represents a UI Selection movement command.  Think moving up and down in options menus or
        ///     highlighting options.
        /// </summary>
        public Vector2 Move { get; set; }

        /// <summary>
        ///     Tracks the current state of the submit or 'move forward' button.  Setting this also updates the
        ///     <see cref="SubmitButtonDelta" /> to track if a press or release occurred in the frame.
        /// </summary>
        public bool SubmitButtonDown
        {
            get => m_SubmitButtonDown;
            set
            {
                if (m_SubmitButtonDown != value)
                {
                    SubmitButtonDelta = value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released;
                    m_SubmitButtonDown = value;
                }
            }
        }

        /// <summary>
        ///     Tracks the changes in <see cref="SubmitButtonDown" /> between calls to <see cref="OnFrameFinished" />.
        /// </summary>
        internal ButtonDeltaState SubmitButtonDelta { get; private set; }

        /// <summary>
        ///     Tracks the current state of the submit or 'move backward' button.  Setting this also updates the
        ///     <see cref="CancelButtonDelta" /> to track if a press or release occurred in the frame.
        /// </summary>
        public bool CancelButtonDown
        {
            get => m_CancelButtonDown;
            set
            {
                if (m_CancelButtonDown != value)
                {
                    CancelButtonDelta = value ? ButtonDeltaState.Pressed : ButtonDeltaState.Released;
                    m_CancelButtonDown = value;
                }
            }
        }

        /// <summary>
        ///     Tracks the changes in <see cref="CancelButtonDown" /> between calls to <see cref="OnFrameFinished" />.
        /// </summary>
        internal ButtonDeltaState CancelButtonDelta { get; private set; }

        /// <summary>
        ///     Internal bookkeeping data used by the Unity UI system.
        /// </summary>
        internal ImplementationData ImplementationDataValue { get; set; }

        /// <summary>
        ///     Resets this object to it's default, unused state.
        /// </summary>
        public void Reset()
        {
            Move = Vector2.zero;
            m_SubmitButtonDown = m_CancelButtonDown = false;
            SubmitButtonDelta = CancelButtonDelta = ButtonDeltaState.NoChange;

            ImplementationDataValue.Reset();
        }

        /// <summary>
        ///     Call this at the end of polling for per-frame changes. This resets delta values, such as
        ///     <see cref="SubmitButtonDelta" /> and <see cref="CancelButtonDelta" />.
        /// </summary>
        public void OnFrameFinished()
        {
            SubmitButtonDelta = ButtonDeltaState.NoChange;
            CancelButtonDelta = ButtonDeltaState.NoChange;
        }

        bool m_SubmitButtonDown;
        bool m_CancelButtonDown;
    }
}

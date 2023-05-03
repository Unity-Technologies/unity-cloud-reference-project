//-----------------------------------------------------------------------
// <copyright file="PinchGesture.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//
// Modified 2023 by Unity Technologies Inc.
//
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Unity.ReferenceProject.Gestures
{
    /// <summary>
    ///     Gesture for when the user performs a two-finger pinch motion on the touch screen.
    /// </summary>
    public class PinchGesture : Gesture<PinchGesture>
    {
        /// <summary>
        ///     Constructs a PinchGesture gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        public PinchGesture(PinchGestureRecognizer recognizer, TouchControl touch1, TouchControl touch2)
            :
            base(recognizer)
        {
            this.touch1 = touch1;
            this.touch2 = touch2;
            startPosition1 = touch1.position.ReadValue();
            startPosition2 = touch2.position.ReadValue();
        }

        /// <summary>
        ///     (Read Only) The control of the first finger used in this gesture.
        /// </summary>
        public TouchControl touch1 { get; }

        /// <summary>
        ///     (Read Only) The control of the second finger used in this gesture.
        /// </summary>
        public TouchControl touch2 { get; }

        /// <summary>
        ///     (Read Only) The screen position of the first finger where the gesture started.
        /// </summary>
        public Vector2 startPosition1 { get; }

        /// <summary>
        ///     (Read Only) The screen position of the second finger where the gesture started.
        /// </summary>
        public Vector2 startPosition2 { get; }

        /// <summary>
        ///     (Read Only) The gap between then position of the first and second fingers.
        /// </summary>
        public float gap { get; private set; }

        /// <summary>
        ///     (Read Only) The gap delta between then position of the first and second fingers.
        /// </summary>
        public float gapDelta { get; private set; }

        public float startGap { get; private set; }

        /// <summary>
        ///     Returns true if this gesture can start.
        /// </summary>
        /// <returns>Returns <see langword="true" /> if the gesture can start. Returns <see langword="false" /> otherwise.</returns>
        protected internal override bool CanStart()
        {
            if (!touch1.isInProgress || !touch2.isInProgress)
            {
                Cancel();
                return false;
            }

            // Check that at least one finger is moving.
            if (touch1.delta.ReadValue() == Vector2.zero && touch2.delta.ReadValue() == Vector2.zero)
            {
                return false;
            }

            var pinchRecognizer = m_Recognizer as PinchGestureRecognizer;

            Vector3 firstToSecondDirection = (startPosition1 - startPosition2).normalized;
            var dot1 = Vector3.Dot(touch1.delta.ReadValue().normalized, -firstToSecondDirection);
            var dot2 = Vector3.Dot(touch2.delta.ReadValue().normalized, firstToSecondDirection);
            var dotThreshold = Mathf.Cos(pinchRecognizer.m_SlopMotionDirectionDegrees * Mathf.Deg2Rad);

            // Check angle of motion for the first touch.
            if (touch1.delta.ReadValue() != Vector2.zero && Mathf.Abs(dot1) < dotThreshold)
            {
                return false;
            }

            // Check angle of motion for the second touch.
            if (touch2.delta.ReadValue() != Vector2.zero && Mathf.Abs(dot2) < dotThreshold)
            {
                return false;
            }

            var deltaPosition = (startPosition1 - startPosition2).magnitude;
            gap = (touch1.position.ReadValue() - touch2.position.ReadValue()).magnitude;
            startGap = gap;
            var separation = GestureTouchesUtility.PixelsToInches(Mathf.Abs(gap - deltaPosition));
            return !(separation < pinchRecognizer.m_SlopInches);
        }

        /// <summary>
        ///     Action to be performed when this gesture is started.
        /// </summary>
        protected internal override void OnStart() { }

        /// <summary>
        ///     Updates this gesture.
        /// </summary>
        /// <returns>True if the update was successful.</returns>
        protected internal override bool UpdateGesture()
        {
            if (!touch1.isInProgress || !touch2.isInProgress)
            {
                Cancel();
                return false;
            }

            if (touch1.phase.ReadValue() == TouchPhase.Canceled || touch2.phase.ReadValue() == TouchPhase.Canceled)
            {
                Cancel();
                return false;
            }

            if (touch1.phase.ReadValue() == TouchPhase.Ended || touch2.phase.ReadValue() == TouchPhase.Ended)
            {
                Complete();
                return false;
            }

            if (touch1.phase.ReadValue() == TouchPhase.Moved || touch2.phase.ReadValue() == TouchPhase.Moved)
            {
                var newgap = (touch1.position.ReadValue() - touch2.position.ReadValue()).magnitude;
                gapDelta = newgap - gap;
                gap = newgap;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Action to be performed when this gesture is cancelled.
        /// </summary>
        protected internal override void OnCancel()
        {
            Complete();
        }

        /// <summary>
        ///     Action to be performed when this gesture is finished.
        /// </summary>
        protected internal override void OnFinish() { }
    }
}

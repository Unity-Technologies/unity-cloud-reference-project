//-----------------------------------------------------------------------
// <copyright file="TwoFingerDragGesture.cs" company="Google">
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
    ///     Gesture for when the user performs a two finger vertical swipe motion on the touch screen.
    /// </summary>
    public class TwoFingerDragGesture : Gesture<TwoFingerDragGesture>
    {
        /// <summary>
        ///     Constructs a two finger drag gesture.
        /// </summary>
        /// <param name="recognizer">The gesture recognizer.</param>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        public TwoFingerDragGesture(
            TwoFingerDragGestureRecognizer recognizer, TouchControl touch1, TouchControl touch2)
            :
            base(recognizer)
        {
            this.touch1 = touch1;
            StartPosition1 = touch1.position.ReadValue();
            this.touch2 = touch2;
            StartPosition2 = touch2.position.ReadValue();
            Position = (StartPosition1 + StartPosition2) / 2;
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
        public Vector2 StartPosition1 { get; }

        /// <summary>
        ///     (Read Only) The screen position of the second finger where the gesture started.
        /// </summary>
        public Vector2 StartPosition2 { get; }

        /// <summary>
        ///     (Read Only) The current screen position of the gesture.
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        ///     (Read Only) The delta screen position of the gesture.
        /// </summary>
        public Vector2 Delta { get; private set; }

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

            var pos1 = touch1.position.ReadValue();
            var diff1 = (pos1 - StartPosition1).magnitude;
            var pos2 = touch2.position.ReadValue();
            var diff2 = (pos2 - StartPosition2).magnitude;
            var slopInches = (m_Recognizer as TwoFingerDragGestureRecognizer).m_SlopInches;
            if (GestureTouchesUtility.PixelsToInches(diff1) < slopInches ||
                GestureTouchesUtility.PixelsToInches(diff2) < slopInches)
            {
                return false;
            }

            var recognizer = m_Recognizer as TwoFingerDragGestureRecognizer;

            // Check both fingers move in the same direction.
            var dot = Vector3.Dot(touch1.delta.ReadValue().normalized, touch2.delta.ReadValue().normalized);
            return !(dot < Mathf.Cos(recognizer.m_AngleThresholdRadians));
        }

        /// <summary>
        ///     Action to be performed when this gesture is started.
        /// </summary>
        protected internal override void OnStart()
        {
            if (GestureTouchesUtility.RaycastFromCamera(StartPosition1, out var hit1))
            {
                TargetObject = hit1.transform.gameObject;
            }

            Position = (touch1.position.ReadValue() + touch2.position.ReadValue()) / 2;
        }

        /// <summary>
        ///     Updates this gesture.
        /// </summary>
        /// <returns>Returns <see langword="true" /> if the update was successful. Returns <see langword="false" /> otherwise.</returns>
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
                Delta = ((touch1.position.ReadValue() + touch2.position.ReadValue()) / 2) - Position;
                Position = (touch1.position.ReadValue() + touch2.position.ReadValue()) / 2;
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

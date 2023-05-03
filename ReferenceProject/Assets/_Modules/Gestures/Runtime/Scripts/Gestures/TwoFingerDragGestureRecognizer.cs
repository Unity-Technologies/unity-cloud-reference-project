//-----------------------------------------------------------------------
// <copyright file="TwoFingerDragGestureRecognizer.cs" company="Google">
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

namespace Unity.ReferenceProject.Gestures
{
    /// <summary>
    ///     Gesture Recognizer for when the user performs a two finger drag motion on the touch screen.
    /// </summary>
    public class TwoFingerDragGestureRecognizer : GestureRecognizer<TwoFingerDragGesture>
    {
        const float k_SlopInches = 0.1f;
        const float k_AngleThresholdRadians = Mathf.PI / 6;

        readonly TouchControl m_Touch1;
        readonly TouchControl m_Touch2;

        internal TwoFingerDragGestureRecognizer(TouchControl touch1, TouchControl touch2)
        {
            m_Touch1 = touch1;
            m_Touch2 = touch2;
        }

        internal float m_SlopInches => k_SlopInches;

        internal float m_AngleThresholdRadians => k_AngleThresholdRadians;

        /// <summary>
        ///     Creates a two finger drag gesture with the given touches.
        /// </summary>
        /// <param name="touch1">The first touch that started this gesture.</param>
        /// <param name="touch2">The second touch that started this gesture.</param>
        /// <returns>The created Swipe gesture.</returns>
        internal TwoFingerDragGesture CreateGesture(TouchControl touch1, TouchControl touch2)
        {
            return new TwoFingerDragGesture(this, touch1, touch2);
        }

        /// <summary>
        ///     Tries to create a two finger drag gesture.
        /// </summary>
        protected internal override void TryCreateGestures()
        {
            TryCreateGestureTwoFingerGestureOnTouchBeganForTouchControls(m_Touch1, m_Touch2, CreateGesture);
        }
    }
}

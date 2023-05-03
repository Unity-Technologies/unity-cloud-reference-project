//-----------------------------------------------------------------------
// <copyright file="GestureRecognizer.cs" company="Google">
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
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;

namespace Unity.ReferenceProject.Gestures
{
    /// <summary>
    ///     Base class for all Gesture Recognizers (i.e. TapGestureRecognizer).
    ///     A Gesture recognizer processes touch input to determine if a gesture should start.
    ///     and fires an event when the gesture is started.
    ///     To determine when an gesture is finished/updated, listen to the events on the
    ///     gesture object.
    /// </summary>
    /// <typeparam name="T">The actual gesture.</typeparam>
    public abstract class GestureRecognizer<T> where T : Gesture<T>
    {
        /// <summary>
        ///     List of current active gestures.
        /// </summary>
        protected List<T> m_Gestures = new List<T>(); // TODO Convert to property

        /// <summary>
        ///     Event fired when a gesture is started.
        ///     To receive an event when the gesture is finished/updated, listen to
        ///     events on the Gesture object.
        /// </summary>
        public event Action<T> onGestureStarted;

        /// <summary>
        ///     Updates this gesture recognizer.
        /// </summary>
        public void Update()
        {
            // Instantiate gestures based on touch input.
            // Just because a gesture was created, doesn't mean that it is started.
            // For example, a DragGesture is created when the user touch's down,
            // but doesn't actually start until the touch has moved beyond a threshold.
            TryCreateGestures();

            // Update gestures and determine if they should start.
            for (var i = 0; i < m_Gestures.Count; i++)
            {
                Gesture<T> gesture = m_Gestures[i];

                gesture.Update();
            }
        }

        /// <summary>
        ///     Try to recognize and create gestures.
        /// </summary>
        protected internal abstract void TryCreateGestures();

        protected internal void TryCreateGestureTwoFingerGestureOnTouchBeganForTouchControls(
            TouchControl touch1, TouchControl touch2,
            Func<TouchControl, TouchControl, T> createGestureFunction)
        {
            if (m_Gestures.Count != 0)
            {
                return;
            }

            if (!touch1.isInProgress
                || GestureTouchesUtility.IsTouchOffScreenEdge(touch1))
            {
                return;
            }

            if (!touch2.isInProgress
                || GestureTouchesUtility.IsTouchOffScreenEdge(touch2))
            {
                return;
            }

            var gesture = createGestureFunction(touch1, touch2);
            gesture.onStart += OnStart;
            gesture.onFinished += OnFinished;
            m_Gestures.Add(gesture);
        }

        void OnStart(T gesture)
        {
            onGestureStarted?.Invoke(gesture);
        }

        void OnFinished(T gesture)
        {
            m_Gestures.Remove(gesture);
        }

        public void Reset()
        {
            m_Gestures.Clear();
        }
    }
}

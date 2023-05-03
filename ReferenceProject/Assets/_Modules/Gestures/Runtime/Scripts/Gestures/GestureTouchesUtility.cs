//-----------------------------------------------------------------------
// <copyright file="GestureTouchesUtility.cs" company="Google">
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
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Unity.ReferenceProject.Gestures
{
    class MockTouch
    {
        static readonly Dictionary<string, FieldInfo> fields;
        readonly object m_Touch;

        static MockTouch()
        {
            fields = new Dictionary<string, FieldInfo>();
            foreach (var f in typeof(Touch).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                fields.Add(f.Name, f);
        }

        public MockTouch()
        {
            m_Touch = new Touch();
        }

        public float deltaTime
        {
            get { return ((Touch)m_Touch).deltaTime; }
            set { fields["m_TimeDelta"].SetValue(m_Touch, value); }
        }

        public int tapCount
        {
            get { return ((Touch)m_Touch).tapCount; }
            set { fields["m_TapCount"].SetValue(m_Touch, value); }
        }

        public TouchPhase phase
        {
            get { return (TouchPhase)((Touch)m_Touch).phase; }
            set { fields["m_Phase"].SetValue(m_Touch, value); }
        }

        public Vector2 deltaPosition
        {
            get { return ((Touch)m_Touch).deltaPosition; }
            set { fields["m_PositionDelta"].SetValue(m_Touch, value); }
        }

        public int fingerId
        {
            get { return ((Touch)m_Touch).fingerId; }
            set { fields["m_FingerId"].SetValue(m_Touch, value); }
        }

        public Vector2 position
        {
            get { return ((Touch)m_Touch).position; }
            set { fields["m_Position"].SetValue(m_Touch, value); }
        }

        public Vector2 rawPosition
        {
            get { return ((Touch)m_Touch).rawPosition; }
            set { fields["m_RawPosition"].SetValue(m_Touch, value); }
        }

        public Touch Touch
        {
            get { return (Touch)m_Touch; }
        }
    }

    /// <summary>
    ///     Singleton used by Gesture's and GestureRecognizer's to interact with touch input.
    ///     1. Makes it easy to find touches by fingerId.
    ///     2. Allows Gestures to Lock/Release fingerIds.
    ///     3. Wraps Input.Touches so that it works both in editor and on device.
    ///     4. Provides helper functions for converting touch coordinates
    ///     and performing raycasts based on touches.
    /// </summary>
    static class GestureTouchesUtility
    {
        const float k_EdgeThresholdInches = 0.1f;

        /// <summary>
        ///     Converts Pixels to Inches.
        /// </summary>
        /// <param name="pixels">The amount to convert in pixels.</param>
        /// <returns>The converted amount in inches.</returns>
        public static float PixelsToInches(float pixels) => pixels / Screen.dpi;

        /// <summary>
        ///     Converts Inches to Pixels.
        /// </summary>
        /// <param name="inches">The amount to convert in inches.</param>
        /// <returns>The converted amount in pixels.</returns>
        public static float InchesToPixels(float inches) => inches * Screen.dpi;

        /// <summary>
        ///     Used to determine if a touch is off the edge of the screen based on some slop.
        ///     Useful to prevent accidental touches from simply holding the device from causing
        ///     confusing behavior.
        /// </summary>
        /// <param name="touch">The touch to check.</param>
        /// <returns>True if the touch is off screen edge.</returns>
        public static bool IsTouchOffScreenEdge(TouchControl touch)
        {
            var slopPixels = InchesToPixels(k_EdgeThresholdInches);

            var result = touch.position.x.ReadValue() <= slopPixels;
            result |= touch.position.y.ReadValue() <= slopPixels;
            result |= touch.position.x.ReadValue() >= Screen.width - slopPixels;
            result |= touch.position.y.ReadValue() >= Screen.height - slopPixels;

            return result;
        }

        /// <summary>
        ///     Performs a Raycast from the camera.
        /// </summary>
        /// <param name="screenPos">The screen position to perform the raycast from.</param>
        /// <param name="result">The RaycastHit result.</param>
        /// <returns>True if an object was hit.</returns>
        public static bool RaycastFromCamera(Vector2 screenPos, out RaycastHit result)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                result = new RaycastHit();
                return false;
            }

            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit))
            {
                result = hit;
                return true;
            }

            result = hit;
            return false;
        }
    }
}

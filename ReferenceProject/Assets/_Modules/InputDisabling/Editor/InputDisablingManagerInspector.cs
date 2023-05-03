using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.ReferenceProject.InputDisabling.Editor
{
    /// <summary>
    ///     Custom inspector for the <see cref="InputDisablingManager" />.
    /// </summary>
    [CustomEditor(typeof(InputDisablingManager))]
    public class InputDisablingManagerInspector : UnityEditor.Editor
    {
        static readonly string k_InfoText = $"All {nameof(InputDisablingSubscriber)}s in the scene are disabled when any UI element is focused, or when a {nameof(InputDisablingOverride)} is active.";
        static readonly string k_EnterPlayModeText = "Enter play mode to resolve items.";
        static readonly GUIContent k_SubscriberListContent = new($"{nameof(InputDisablingSubscriber)}s", $"All {nameof(InputDisablingSubscriber)}s currently found in the scene.");
        static readonly GUIContent k_ActiveOverridesListContent = new("Active Overrides", $"{nameof(InputDisablingOverride)}s currently found in the scene that are actively disabling all {nameof(InputDisablingSubscriber)}s, regardless of the UI focus state.");
        static readonly string k_NoItemsFoundLabel = "None found";

        InputDisablingManager m_Target;

        void OnEnable()
        {
            m_Target = target as InputDisablingManager;
        }

        void OnDisable()
        {
            m_Target = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (serializedObject.targetObject is not InputDisablingManager)
                return;

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(k_InfoText, MessageType.Info);

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(k_EnterPlayModeText, MessageType.Warning);
                return;
            }

            DrawObjectFieldList(k_SubscriberListContent, m_Target.InputDisablingSubscribers);
            DrawObjectFieldList(k_ActiveOverridesListContent, m_Target.ActiveOverrides);

            EditorUtility.SetDirty(m_Target);
        }

        void DrawObjectFieldList<T>(GUIContent guiContent, List<T> list) where T : IInputDisablingBaseObject
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(guiContent, EditorStyles.boldLabel);
            GUI.enabled = false;

            if (list == null)
                return;

            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    EditorGUILayout.ObjectField(item.GameObject, typeof(GameObject), true);
                }
            }
            else
            {
                EditorGUILayout.LabelField(k_NoItemsFoundLabel, EditorStyles.boldLabel);
            }

            EditorGUILayout.EndVertical();
            GUI.enabled = true;
        }
    }
}

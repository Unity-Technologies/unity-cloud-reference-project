using System;
using UnityEditor;
using UnityEngine;

namespace Unity.ReferenceProject.Tools.Editor
{
    [CustomEditor(typeof(ToolUIController), true)]
    public class ToolUIControllerInspector : UnityEditor.Editor
    {
        static readonly GUIContent k_EventsFoldoutContent = new("Events");

        bool m_EventsFoldout;
        ToolUIController m_Target;

        void OnEnable()
        {
            m_Target = target as ToolUIController;
        }

        void OnDisable()
        {
            m_Target = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (serializedObject.targetObject is not ToolUIController)
                return;

            serializedObject.Update();

            DrawEvents();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawEvents()
        {
            m_EventsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(m_EventsFoldout, k_EventsFoldoutContent);
            if (m_EventsFoldout)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m_Target.ToolOpened)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m_Target.ToolClosed)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m_Target.ToolPointerEntered)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m_Target.ToolPointerExited)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m_Target.ToolFocusIn)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(m_Target.ToolFocusOut)));
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}

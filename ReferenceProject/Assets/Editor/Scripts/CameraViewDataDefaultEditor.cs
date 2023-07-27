using System.Collections;
using System.Collections.Generic;
using Unity.ReferenceProject.Navigation;
using UnityEngine;

namespace Unity.ReferenceProject.Editor
{
    using UnityEditor;
    using UnityEngine;
    
    [CustomEditor(typeof(CameraViewDataDefault))]
    public class CameraViewDataDefaultEditor : Editor
    {
        SerializedProperty m_Icon;
        SerializedProperty m_ViewName;
        SerializedProperty m_UseDefaultView;
        SerializedProperty m_AngleRotation;
        
        void OnEnable()
        {
            m_Icon = serializedObject.FindProperty("m_Icon");
            m_ViewName = serializedObject.FindProperty("m_ViewName");
            m_UseDefaultView = serializedObject.FindProperty("m_UseDefaultView");
            m_AngleRotation = serializedObject.FindProperty("m_AngleRotation");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Icon);
            EditorGUILayout.PropertyField(m_ViewName);
            EditorGUILayout.PropertyField(m_UseDefaultView);

            if (m_UseDefaultView.boolValue)
            {
                GUI.enabled = false;
            }

            EditorGUILayout.PropertyField(m_AngleRotation);
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
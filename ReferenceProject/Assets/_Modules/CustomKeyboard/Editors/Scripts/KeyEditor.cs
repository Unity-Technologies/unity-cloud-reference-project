using System;
using UnityEditor;
using UnityEngine;

namespace Unity.ReferenceProject.CustomKeyboard.Editor
{
    [CustomPropertyDrawer(typeof(KeyData))]
    public class KeyEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var typeProp = property.FindPropertyRelative("Type");
            var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(typeRect, typeProp);

            var useText = property.FindPropertyRelative("UseText").boolValue;

            Rect sizeRect = new Rect();
            Rect isIconRect;
            var iconTextRect = GetRect(position, 1);
            switch ((KeyType)typeProp.enumValueIndex)
            {
                case KeyType.Character:
                    var characterRect =  GetRect(position, 1);
                    sizeRect =  GetRect(position, 2);

                    EditorGUI.PropertyField(characterRect, property.FindPropertyRelative("Character"));
                    break;
                case KeyType.ExtendedCharacter:
                    var regularCharRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);
                    var extendedCharRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 2, position.width, EditorGUIUtility.singleLineHeight);
                    sizeRect = new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * 3, position.width, EditorGUIUtility.singleLineHeight);

                    EditorGUI.PropertyField(regularCharRect, property.FindPropertyRelative("Character"));
                    EditorGUI.PropertyField(extendedCharRect, property.FindPropertyRelative("ExtendedCharacters"));
                    break;
                case KeyType.SwitchLayout:
                    var layoutRect =  GetRect(position, 2);
                    var returnMainLayout =  GetRect(position, 3);
                    sizeRect =  GetRect(position, 4);
                    isIconRect =  GetRect(position, 5);

                    IconOrText(useText, property, iconTextRect);

                    EditorGUI.PropertyField(layoutRect, property.FindPropertyRelative("Layout"));
                    EditorGUI.PropertyField(returnMainLayout, property.FindPropertyRelative("ReturnMainLayout"));
                    EditorGUI.PropertyField(isIconRect, property.FindPropertyRelative("UseText"));
                    break;
                case KeyType.Ascii:
                    var keyCodeRect =  GetRect(position, 2);
                    sizeRect = GetRect(position, 3);
                    isIconRect =  GetRect(position, 4);

                    IconOrText(useText, property, iconTextRect);

                    EditorGUI.PropertyField(keyCodeRect, property.FindPropertyRelative("AsciiCode"));
                    EditorGUI.PropertyField(isIconRect, property.FindPropertyRelative("UseText"));
                    break;
                case KeyType.Functional:
                    var functionalRect =  GetRect(position, 2);
                    sizeRect =  GetRect(position, 3);
                    isIconRect =  GetRect(position, 4);

                    IconOrText(useText, property, iconTextRect);

                    EditorGUI.PropertyField(functionalRect, property.FindPropertyRelative("FunctionalKeyType"));
                    EditorGUI.PropertyField(isIconRect, property.FindPropertyRelative("UseText"));
                    break;
                case KeyType.EmptySpace:
                    sizeRect =  GetRect(position, 1);
                    break;
            }

            EditorGUI.PropertyField(sizeRect, property.FindPropertyRelative("Size"));
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProp = property.FindPropertyRelative("Type");
            switch ((KeyType)typeProp.enumValueIndex)
            {
                case KeyType.EmptySpace:
                    return (EditorGUIUtility.singleLineHeight + 2) * 2;
                case KeyType.Character:
                    return (EditorGUIUtility.singleLineHeight + 2) * 3;
                case KeyType.ExtendedCharacter:
                    return (EditorGUIUtility.singleLineHeight + 2) * 4;
                case KeyType.Ascii:
                case KeyType.Functional:
                    return (EditorGUIUtility.singleLineHeight + 2) * 5;
                case KeyType.SwitchLayout:
                    return (EditorGUIUtility.singleLineHeight + 2) * 6;
                default:
                    return EditorGUIUtility.singleLineHeight;
            }
        }

        static Rect GetRect(Rect position, int lineNumber)
        {
            return new Rect(position.x, position.y + (EditorGUIUtility.singleLineHeight + 2) * lineNumber, position.width, EditorGUIUtility.singleLineHeight);
        }

        static void IconOrText(bool useText, SerializedProperty property, Rect iconTextRect)
        {
            if (useText)
            {
                EditorGUI.PropertyField(iconTextRect, property.FindPropertyRelative("Text"));
            }
            else
            {
                EditorGUI.PropertyField(iconTextRect, property.FindPropertyRelative("Icon"));
            }
        }
    }
}

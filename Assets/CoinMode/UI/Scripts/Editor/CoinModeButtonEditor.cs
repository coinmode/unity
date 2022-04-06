#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(CoinModeButton), true)]
    [CanEditMultipleObjects]
    public class CoinModeButtonEditor : ButtonEditor
    {
        SerializedProperty displayModeProperty;
        SerializedProperty buttonState;
        SerializedProperty loadingSpinner;
        SerializedProperty mainButtonText;
        SerializedProperty ignoreMenuStyle;

        CoinModeButton button;

        protected override void OnEnable()
        {
            base.OnEnable();
            displayModeProperty = serializedObject.FindProperty("_displayTemplate");
            buttonState = serializedObject.FindProperty("_buttonState");
            loadingSpinner = serializedObject.FindProperty("loadingSpinner");
            mainButtonText = serializedObject.FindProperty("mainButtonText");
            button = serializedObject.targetObject as CoinModeButton;
            ignoreMenuStyle = serializedObject.FindProperty("ignoreMenuStyle");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("Refresh Template"))
            {
                button.SetButtonTemplate(button.displayTemplate);
            }

            if (displayModeProperty.intValue < -2 || displayModeProperty.intValue >= CoinModeMenuStyle.customButtonTemplateCount)
            {
                displayModeProperty.intValue = -1;
            }

            displayModeProperty.intValue = EditorGUILayout.Popup(displayModeProperty.intValue + 2, CoinModeMenuStyleEditorWindow.buttonTemplateNames) - 2;
            EditorGUILayout.PropertyField(buttonState);
            EditorGUILayout.PropertyField(loadingSpinner);
            EditorGUILayout.PropertyField(mainButtonText);
            EditorGUILayout.PropertyField(ignoreMenuStyle);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif
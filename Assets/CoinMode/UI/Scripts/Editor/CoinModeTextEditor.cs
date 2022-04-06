#if UNITY_EDITOR
using CoinMode.UI;
using UnityEngine;
using UnityEditor;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(CoinModeText), true)]
    [CanEditMultipleObjects]
    public class CoinModeTextEditor : UnityEditor.UI.TextEditor
    {
        SerializedProperty useAccentColorProperty;
        SerializedProperty fontFamilyStyleProperty;
        SerializedProperty ignoreMenuStyleProperty;

        CoinModeText text;

        protected override void OnEnable()
        {
            base.OnEnable();
            useAccentColorProperty = serializedObject.FindProperty("_useAccentColor");
            fontFamilyStyleProperty = serializedObject.FindProperty("_fontFamilyStyle");
            ignoreMenuStyleProperty = serializedObject.FindProperty("ignoreMenuStyle");
            text = serializedObject.targetObject as CoinModeText;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (GUILayout.Button("Refresh Template"))
            {
                text.SetFontFamilyStyle(text.fontFamilyStyle);
            }

            EditorGUILayout.PropertyField(useAccentColorProperty);
            EditorGUILayout.PropertyField(fontFamilyStyleProperty);
            EditorGUILayout.PropertyField(ignoreMenuStyleProperty);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif
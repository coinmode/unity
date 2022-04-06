#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(CoinModeInputField), true)]
    [CanEditMultipleObjects]
    public class CoinModeInputFieldEditor : InputFieldEditor
    {
        SerializedProperty frame;
        SerializedProperty background;

        CoinModeInputField inputField;

        protected override void OnEnable()
        {
            base.OnEnable();
            frame = serializedObject.FindProperty("frame");
            background = serializedObject.FindProperty("background");
            inputField = serializedObject.targetObject as CoinModeInputField;
        }

        public override void OnInspectorGUI()
        {        
            serializedObject.Update();
            if (GUILayout.Button("Refresh Template"))
            {
                inputField.InitFromStyle();
            }

            EditorGUILayout.PropertyField(frame);
            EditorGUILayout.PropertyField(background);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif
#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEngine;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(TitleLoginComponent), true)]
    [CanEditMultipleObjects]
    public class TitleLoginComponentEditor : UnityEditor.Editor
    {
        private SerializedProperty loginButtonProperty = null;
        private SerializedProperty loginTextProperty = null;
        private SerializedProperty poweredByContainerProperty = null;
        private SerializedProperty poweredByTextProperty = null;
        private SerializedProperty coinModeTextProperty = null;
        private SerializedProperty spinnerProperty = null;
        private SerializedProperty loginErrorTextProperty = null;
        private SerializedProperty loginEventProperty = null;
        private SerializedProperty logoutEventProperty = null;
        private SerializedProperty sizeProperty = null;

        private bool showComponents = false;

        private void OnEnable()
        {
            loginButtonProperty = serializedObject.FindProperty("loginButton");
            loginTextProperty = serializedObject.FindProperty("loginText");
            poweredByContainerProperty = serializedObject.FindProperty("poweredByContainer");
            poweredByTextProperty = serializedObject.FindProperty("poweredByText");
            coinModeTextProperty = serializedObject.FindProperty("coinModeText");
            spinnerProperty = serializedObject.FindProperty("loadingSpinner");
            loginErrorTextProperty = serializedObject.FindProperty("loginErrorText");
            loginEventProperty = serializedObject.FindProperty("loginEvent");
            logoutEventProperty = serializedObject.FindProperty("logoutEvent");
            sizeProperty = serializedObject.FindProperty("size");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(sizeProperty);

            GUILayout.Space(5);

            EditorGUILayout.LabelField("Login Response Event", EditorStyles.boldLabel);

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(loginEventProperty);

            GUILayout.Space(-10);

            EditorGUILayout.LabelField("Logout Response Event", EditorStyles.boldLabel);

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(logoutEventProperty);

            GUILayout.Space(-10);

#if UNITY_2019_1_OR_NEWER
            showComponents = EditorGUILayout.Foldout(showComponents, "UI Components", EditorStyles.foldoutHeader);
#else
            showComponents = EditorGUILayout.Foldout(showComponents, "UI Components", EditorStyles.foldout);
#endif
            if (showComponents)
            {
                EditorGUILayout.PropertyField(loginButtonProperty);                
                EditorGUILayout.PropertyField(loginTextProperty);
                EditorGUILayout.PropertyField(poweredByContainerProperty);
                EditorGUILayout.PropertyField(poweredByTextProperty);
                EditorGUILayout.PropertyField(coinModeTextProperty);
                EditorGUILayout.PropertyField(spinnerProperty);
                EditorGUILayout.PropertyField(loginErrorTextProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
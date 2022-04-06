#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(CoinModeCurrencyInputField), true)]
    [CanEditMultipleObjects]
    public class CoinModeCurrencyInputFieldEditor : CoinModeInputFieldEditor
    {
        SerializedProperty prefixTextProperty; 
        SerializedProperty suffixTextProperty;
        SerializedProperty suffixButtonProperty;

        SerializedProperty conversionContainerProperty;
        SerializedProperty conversionPrefixProperty;
        SerializedProperty conversionTextProperty;
        SerializedProperty conversionSuffixProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            prefixTextProperty = serializedObject.FindProperty("currencyPrefix");
            suffixTextProperty = serializedObject.FindProperty("currencySuffix");
            suffixButtonProperty = serializedObject.FindProperty("currencySuffixButton");
            conversionContainerProperty = serializedObject.FindProperty("conversionDisplay");
            conversionPrefixProperty = serializedObject.FindProperty("conversionPrefix");
            conversionTextProperty = serializedObject.FindProperty("conversionText");
            conversionSuffixProperty = serializedObject.FindProperty("conversionSuffix");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(prefixTextProperty);
            EditorGUILayout.PropertyField(suffixTextProperty);
            EditorGUILayout.PropertyField(suffixButtonProperty);
            EditorGUILayout.PropertyField(conversionContainerProperty);
            EditorGUILayout.PropertyField(conversionPrefixProperty);
            EditorGUILayout.PropertyField(conversionTextProperty);
            EditorGUILayout.PropertyField(conversionSuffixProperty);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
#endif
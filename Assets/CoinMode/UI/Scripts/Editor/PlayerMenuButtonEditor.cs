#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(PlayerMenuButton), true)]
    [CanEditMultipleObjects]
    public class PlayerMenuButtonEditor : ButtonEditor
    {
        SerializedProperty downloadableImageProperty;
        SerializedProperty defaultIconProperty;
        SerializedProperty userIconProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            downloadableImageProperty = serializedObject.FindProperty("avatarDownloadableImage");
            defaultIconProperty = serializedObject.FindProperty("defaultIcon");
            userIconProperty = serializedObject.FindProperty("userIcon");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(downloadableImageProperty);
            EditorGUILayout.PropertyField(defaultIconProperty);
            EditorGUILayout.PropertyField(userIconProperty);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();
        }
    }
}
#endif
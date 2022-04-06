#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(TitleLoginButton), true)]
    [CanEditMultipleObjects]
    public class TitleLoginButtonEditor : CoinModeButtonEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif
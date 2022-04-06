#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(VerificationInputField), true)]
    [CanEditMultipleObjects]
    public class LoginVerificationFieldEditor : CoinModeInputFieldEditor
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
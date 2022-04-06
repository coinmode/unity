#if UNITY_EDITOR
using CoinMode.UI;
using UnityEditor;
using UnityEditor.UI;

namespace CoinMode.Editor
{
    [CustomEditor(typeof(CoinModeToggle), true)]
    [CanEditMultipleObjects]
    public class CoinModeToggleEditor : ToggleEditor
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
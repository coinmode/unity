
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using UnityEngine;

namespace CoinMode.UI
{
    public class CoinModeMenuStylePreset : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem("CoinMode/Create Menu Style Preset")]
        public static void CreateAsset()
        {
            CoinModeMenuStylePreset stylePreset = CreateInstance<CoinModeMenuStylePreset>();
            stylePreset.preset.LoadDefaultAssets();
            if (!Directory.Exists(Application.dataPath + "/CoinMode/Settings/Presets"))
            {
                Directory.CreateDirectory(Application.dataPath + "/CoinMode/Settings/Presets");
            }
            string fileName = AssetDatabase.GenerateUniqueAssetPath("Assets/CoinMode/Settings/Presets/StylePreset.asset");            
            AssetDatabase.CreateAsset(stylePreset, fileName);
#if UNITY_2020_3_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(stylePreset);
#else
            EditorUtility.SetDirty(stylePreset);
            AssetDatabase.SaveAssets();
#endif
            EditorUtility.FocusProjectWindow();
        }
#endif

        public CoinModeMenuStyle.StylePreset preset = new CoinModeMenuStyle.StylePreset();
    }
}

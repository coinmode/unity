#if UNITY_EDITOR
using CoinMode.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.Editor
{
    public class CoinModeMenuStyleEditorWindow : CoinModeEditorWindow
    {
        internal static string[] buttonTemplateNames { get; private set; } = new string[0];

        private bool titleLoginButtonTemplateFoldout = false;
        private bool defaultButtonTemplateFoldout = false;
        private List<bool> customButtonTemplateFoldouts = new List<bool>();
        private string[] customButtonTemplateNames
        {
            get { return _customButtonTemplateNames; }
            set
            {
                _customButtonTemplateNames = value;
                buttonTemplateNames = new string[_customButtonTemplateNames.Length + 2];
                buttonTemplateNames[0] = "Title Login Template";
                buttonTemplateNames[1] = "Default Template";
            }
        }
        private string[] _customButtonTemplateNames = new string[0];        

        private Vector2 scrollPos;

        private Color tempColor;
        private ColorBlock tempColorBlock;
        private CoinModeMenuStyle.TextColorBlock tempTextColorBlock;
        private ColorMode tempColorMode;
        private FontStyle tempFontStyle;

        private CoinModeMenuStylePreset stylePreset;

        [MenuItem("CoinMode/Menu Style")]
        static void Open()
        {
            CoinModeMenuStyleEditorWindow window = (CoinModeMenuStyleEditorWindow)EditorWindow.GetWindow(typeof(CoinModeMenuStyleEditorWindow));
            window.Show();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));           

            // Menu Styling
            GUILayout.Space(5.0F);
            GUILayout.Label("Menu Styling", headingLabel);
            GUILayout.Space(5.0F);

            // Background
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            GUILayout.Label("Background");
            GUILayout.EndHorizontal();

            BeginChangeCheck();
            tempSprite = DrawSprite("Sprite", 30.0F, CoinModeMenuStyle.backgroundSprite);
            if (EndChangeCheck("Changed Sprite"))
            {
                CoinModeMenuStyle.instance.EditorSetBackgroundSprite(tempSprite);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.backgroundColor;
            DrawColor("Color", 30.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetBackgroundColor(tempColor);
            }

            BeginChangeCheck();
            tempSprite = DrawSprite("Pattern Sprite", 30.0F, CoinModeMenuStyle.backgroundPatternSprite);
            if (EndChangeCheck("Changed Sprite"))
            {
                CoinModeMenuStyle.instance.EditorSetBackgroundPatternSprite(tempSprite);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.accentColor;
            DrawColor("Accent Color", 15.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetAccentColor(tempColor);
            }

            // Messages
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            GUILayout.Label("Messages");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(30.0F);
            GUILayout.Label("Background");
            GUILayout.EndHorizontal();

            BeginChangeCheck();
            tempSprite = DrawSprite("Sprite", 45.0F, CoinModeMenuStyle.messageBackgroundSprite);
            if (EndChangeCheck("Changed Sprite"))
            {
                CoinModeMenuStyle.instance.EditorSetMessageBackgroundSprite(tempSprite);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.messageBackgroundColor;
            DrawColor("Color", 45.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetMessageBackgroundColor(tempColor);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.messageSuccessColor;
            DrawColor("Success Color", 45.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetMessageSuccessColor(tempColor);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.messageFailureColor;
            DrawColor("Failure Color", 45.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetMessageFailureColor(tempColor);
            }

            // Text Color
            GUILayout.Space(5.0F);
            GUILayout.Label("Text Color", headingLabel);
            GUILayout.Space(5.0F);

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.textColor;
            DrawColor("Main", 15.0F, ref tempColor);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetTextColor(tempColor);
            }

            // Font Family
            GUILayout.Space(5.0F);
            GUILayout.Label("Fonts", headingLabel);
            GUILayout.Space(5.0F);

            BeginChangeCheck();
            tempFont = DrawFont("Thin", 15.0F, CoinModeMenuStyle.thinFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetThinFont(tempFont);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Light", 15.0F, CoinModeMenuStyle.lightFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetLightFont(tempFont);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Regular", 15.0F, CoinModeMenuStyle.regularFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetRegularFont(tempFont);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Medium", 15.0F, CoinModeMenuStyle.mediumFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetMediumFont(tempFont);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Bold", 15.0F, CoinModeMenuStyle.boldFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetBoldFont(tempFont);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Black", 15.0F, CoinModeMenuStyle.blackFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetBlackFont(tempFont);
            }

            // Button Styling
            GUILayout.Space(5.0F);
            GUILayout.Label("Button Styling", headingLabel);
            GUILayout.Space(5.0F);

            // Draw Title Login Button Template
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            titleLoginButtonTemplateFoldout = EditorGUILayout.Foldout(titleLoginButtonTemplateFoldout, "Title Login Template");
            GUILayout.EndHorizontal();

            if (titleLoginButtonTemplateFoldout)
            {
                DrawButtonTemplate(30.0F, 45.0F, -2);
            }

            // Draw Default Button Template
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            defaultButtonTemplateFoldout = EditorGUILayout.Foldout(defaultButtonTemplateFoldout, "Default Template");
            GUILayout.EndHorizontal();

            if (defaultButtonTemplateFoldout)
            {
                DrawButtonTemplate(30.0F, 45.0F);
            }

            // Custom Button Templates
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            GUILayout.Label("Custom Templates");
            GUILayout.EndHorizontal();

            // Add / Remove Custom Button Templates
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Add Custom Template"))
            {
                RegisterChange("Add Custom Template");
                CoinModeMenuStyle.instance.EditorAddCustomButtonTemplate();
                customButtonTemplateFoldouts.Add(true);
                customButtonTemplateNames = new string[CoinModeMenuStyle.customButtonTemplateCount];
                for (int i = 0; i < CoinModeMenuStyle.customButtonTemplateCount; i++)
                {
                    customButtonTemplateNames[i] = GetCustomTemplateLabel(i);
                }
                Array.Copy(_customButtonTemplateNames, 0, buttonTemplateNames, 2, _customButtonTemplateNames.Length);
            }
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Remove Custom Template"))
            {
                if (CoinModeMenuStyle.customButtonTemplateCount > 0)
                {
                    RegisterChange("Remove Custom Template");
                    CoinModeMenuStyle.instance.EditorRemoveCustomButtonTemplate();
                    customButtonTemplateFoldouts.RemoveAt(customButtonTemplateFoldouts.Count - 1);

                    customButtonTemplateNames = new string[CoinModeMenuStyle.customButtonTemplateCount];
                    for (int i = 0; i < CoinModeMenuStyle.customButtonTemplateCount; i++)
                    {
                        customButtonTemplateNames[i] = GetCustomTemplateLabel(i);
                    }
                    Array.Copy(_customButtonTemplateNames, 0, buttonTemplateNames, 2, _customButtonTemplateNames.Length);
                }                
            }
            GUILayout.EndHorizontal();

            // Draw Custom Button Templates
            for(int i = 0; i < CoinModeMenuStyle.customButtonTemplateCount; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(15.0F);
                customButtonTemplateFoldouts[i] = EditorGUILayout.Foldout(customButtonTemplateFoldouts[i], customButtonTemplateNames[i]);
                GUILayout.EndHorizontal();

                if (customButtonTemplateFoldouts[i])
                {
                    DrawButtonTemplate(30.0F, 45.0F, i);
                }
            }

            // Input Field Styling
            GUILayout.Space(5.0F);
            GUILayout.Label("Input Field Styling", headingLabel);
            GUILayout.Space(5.0F);

            BeginChangeCheck();
            tempSprite = DrawSprite("Frame Sprite", 15.0F, CoinModeMenuStyle.inputFieldFrameSprite);
            if (EndChangeCheck("Changed Sprite"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldFrameSprite(tempSprite);
            }

            BeginChangeCheck();
            tempSprite = DrawSprite("Background Sprite", 15.0F, CoinModeMenuStyle.inputFieldBackgroundSprite);
            if (EndChangeCheck("Changed Sprite"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldBackgroundSprite(tempSprite);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.inputFieldBackgroundColor;
            DrawColor("Background Color", 15.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldBackgroundColor(tempColor);
            }

            BeginChangeCheck();
            tempColorBlock = CoinModeMenuStyle.inputFieldColors;
            DrawColorBlock("Button Colors", 15.0F, 30.0F, ref tempColorBlock);
            if (EndChangeCheck("Changed Color Block"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldColors(tempColorBlock);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Font", 15.0F, CoinModeMenuStyle.inputFieldFont);
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldFont(tempFont);
            }

            BeginChangeCheck();
            tempFontStyle = CoinModeMenuStyle.inputFieldFontStyle;
            DrawFontStyle("Font Style", 15.0F, ref tempFontStyle);
            if (EndChangeCheck("Changed Font Style"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldFontStyle(tempFontStyle);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.inputFieldTextColor;
            DrawColor("Text Color", 15.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldTextColor(tempColor);
            }

            BeginChangeCheck();
            tempColor = CoinModeMenuStyle.inputFieldPlaceholderColor;
            DrawColor("Placeholder Color", 15.0F, ref tempColor);
            if (EndChangeCheck("Changed Color"))
            {
                CoinModeMenuStyle.instance.EditorSetInputFieldPlaceholderColor(tempColor);
            }

            // Other
            GUILayout.Space(5.0F);
            GUILayout.Label("Other", headingLabel);
            GUILayout.Space(5.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Reset To Default"))
            {
                RegisterChange("Reset to default");
                CoinModeMenuStyle.instance.EditorResetToDefault();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(15.0F);

            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            stylePreset = EditorGUILayout.ObjectField("Preset", stylePreset, typeof(CoinModeMenuStylePreset), false, GUILayout.Height(EditorGUIUtility.singleLineHeight)) as CoinModeMenuStylePreset;
            GUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(stylePreset == null);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Load From Preset"))
            {
                RegisterChange("Loaded from preset");
                CoinModeMenuStyle.instance.EditorLoadFromPreset(stylePreset);
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(stylePreset == null);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15.0F);
            if (GUILayout.Button("Save To Preset"))
            {
                RegisterChange("Saved to preset");
                stylePreset.preset = CoinModeMenuStyle.instance.EditorGetStyle();
#if UNITY_2020_3_OR_NEWER
                EditorUtility.SetDirty(stylePreset);
                AssetDatabase.SaveAssetIfDirty(stylePreset);
#else
                EditorUtility.SetDirty(stylePreset);
                AssetDatabase.SaveAssets();
#endif
            }
            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            GUILayout.Space(15.0F);

            base.OnGUI();

            EditorGUILayout.EndScrollView();
        }

        private void DrawButtonTemplate(float offset, float innerOffset, int templateIndex = -1)
        {
            BeginChangeCheck();
            tempSprite = DrawSprite("Sprite", offset, CoinModeMenuStyle.GetButtonSprite(templateIndex));
            if (EndChangeCheck("Changed Sprite"))
            {
                CoinModeMenuStyle.instance.EditorSetButtonSprite(tempSprite, templateIndex);
            }

            BeginChangeCheck();
            tempColorBlock = CoinModeMenuStyle.GetButtonColors(templateIndex);
            DrawColorBlock("Button Colors", offset, innerOffset, ref tempColorBlock);
            if (EndChangeCheck("Changed Color Block"))
            {
                CoinModeMenuStyle.instance.EditorSetButtonColors(tempColorBlock, templateIndex);
            }

            BeginChangeCheck();
            tempFont = DrawFont("Font", offset, CoinModeMenuStyle.GetButtonFont(templateIndex));
            if (EndChangeCheck("Changed Font"))
            {
                CoinModeMenuStyle.instance.EditorSetButtonFont(tempFont, templateIndex);
            }

            BeginChangeCheck();
            tempFontStyle = CoinModeMenuStyle.GetButtonFontStyle(templateIndex);
            DrawFontStyle("Font Style", offset, ref tempFontStyle);
            if (EndChangeCheck("Changed Font Style"))
            {
                CoinModeMenuStyle.instance.EditorSetButtonFontStyle(tempFontStyle, templateIndex);
            }

            BeginChangeCheck();
            tempTextColorBlock = CoinModeMenuStyle.GetButtonTextColors(templateIndex);
            DrawTextColorBlock("Text Colors", offset, innerOffset, ref tempTextColorBlock);
            if (EndChangeCheck("Changed Text Color Block"))
            {
                CoinModeMenuStyle.instance.EditorSetButtonTextColors(tempTextColorBlock, templateIndex);
            }

            BeginChangeCheck();
            tempColorMode = CoinModeMenuStyle.GetButtonLoadingColorMode(templateIndex);
            DrawColorMode("Loading Color Mode", offset, ref tempColorMode);
            if (EndChangeCheck("Changed Text Color Block"))
            {
                CoinModeMenuStyle.instance.EditorSetButtonLoadingColorMode(tempColorMode, templateIndex);
            }
        }

        protected override void Init()
        {
            base.Init();
            CoinModeMenuStyle.InitStyle();
            objectInContext = CoinModeMenuStyle.instance;

            titleContent.text = "CoinMode Menu Style";

            customButtonTemplateFoldouts.Clear();
            for (int i = 0; i < CoinModeMenuStyle.customButtonTemplateCount; i++)
            {
                customButtonTemplateFoldouts.Add(false);
            }

            customButtonTemplateNames = new string[CoinModeMenuStyle.customButtonTemplateCount];
            for (int i = 0; i < CoinModeMenuStyle.customButtonTemplateCount; i++)
            {
                customButtonTemplateNames[i] = GetCustomTemplateLabel(i);
            }
            Array.Copy(_customButtonTemplateNames, 0, buttonTemplateNames, 2, _customButtonTemplateNames.Length);
        }

        private string GetCustomTemplateLabel(int index)
        {
            return "Custom Template " + (index + 1).ToString("D2");
        }
    }
}
#endif
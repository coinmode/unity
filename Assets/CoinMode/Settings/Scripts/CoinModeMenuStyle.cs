using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public enum ColorMode
    {
        Light,
        Dark
    }

    public enum FontFamilyStyle
    {
        Thin,
        Light,
        Regular,
        Medium,
        Bold,
        Black
    }

    [DefaultExecutionOrder(-202)]
    public class CoinModeMenuStyle : ScriptableObject
    {
        public static Color defaultBackgroundColor = new Color32(26, 26, 26, 255);
        public static Color defaultAccentColor = new Color32(253, 183, 20, 255);
        public static ColorBlock defaultColorBlock
        {
            get
            {
                ColorBlock c = new ColorBlock();
                c.normalColor = new Color32(255, 255, 255, 255);
                c.highlightedColor = new Color32(245, 245, 245, 255);
                c.pressedColor = new Color32(200, 200, 200, 255);
#if UNITY_2019_1_OR_NEWER
                c.selectedColor = new Color32(245, 245, 245, 255);
#endif
                c.disabledColor = new Color32(200, 200, 200, 128);
                c.colorMultiplier = 1.0F;
                c.fadeDuration = 0.1F;
                return c;
            }
        }

        [System.Serializable]
        public struct TextColorBlock
        {
            public static TextColorBlock darkColors { get { return new TextColorBlock(new Color32(25, 25, 25, 255), new Color32(25, 25, 25, 128)); } }
            public static TextColorBlock lightColors { get { return new TextColorBlock(new Color32(240, 240, 240, 255), new Color32(240, 240, 240, 128)); } }

            public static TextColorBlock defaultCoinModeColors { get { return new TextColorBlock(new Color32(50, 50, 50, 255), new Color32(255, 255, 255, 128));} }
            public static TextColorBlock titleLoginColors { get { return new TextColorBlock(new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 128));} }

            public static TextColorBlock GetContrastingColor(in Color color)
            {
                return GetColorMode(in color) == ColorMode.Light ? darkColors : lightColors;
            }

            public Color normalColor;
            public Color disabledColor;

            TextColorBlock(Color normalColor, Color disabledColor)
            {
                this.normalColor = normalColor;
                this.disabledColor = disabledColor;
            }
        }

        [System.Serializable]
        public class ButtonTemplate
        {            
            private static ColorBlock defaultCoinModeColors
            {
                get
                {
                    ColorBlock c = new ColorBlock();
                    c.normalColor = new Color32(253, 183, 20, 255);
                    c.highlightedColor = new Color32(64, 163, 255, 255);
                    c.pressedColor = new Color32(67, 136, 175, 255);
#if UNITY_2019_1_OR_NEWER
                    c.selectedColor = new Color32(64, 163, 255, 255);
#endif
                    c.disabledColor = new Color32(53, 53, 53, 255);
                    c.colorMultiplier = 1.0F;
                    c.fadeDuration = 0.1F;
                    return c;
                }
            }

            internal static ColorBlock titleLoginColors
            {
                get
                {
                    ColorBlock c = new ColorBlock();
                    c.normalColor = new Color32(0, 0, 0, 255);
                    c.highlightedColor = new Color32(55, 55, 55, 255);
                    c.pressedColor = new Color32(66, 66, 66, 255);
#if UNITY_2019_1_OR_NEWER
                    c.selectedColor = new Color32(55, 55, 55, 255);
#endif
                    c.disabledColor = new Color32(200, 200, 200, 128);
                    c.colorMultiplier = 1.0F;
                    c.fadeDuration = 0.1F;
                    return c;
                }
            }

            internal static Sprite defaultSprite
            {
                get
                {
                    return Resources.Load<Sprite>("ButtonSprite");
                }
            }

            internal static Font defaultFont
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Regular");
                }
            }

            internal static Font defaultTitleLoginFont
            {
                get
                {
                    return Resources.Load<Font>("Fonts/SuezOne-Regular");
                }
            }

            public Sprite sprite = null;
            public ColorBlock colors = defaultCoinModeColors;
            public Font font = null;
            public FontStyle fontStyle = FontStyle.Normal;
            public TextColorBlock textColors = TextColorBlock.defaultCoinModeColors;
            public ColorMode loadingColorMode = ColorMode.Dark;
        }

        [System.Serializable]
        public class InputFieldTemplate
        {
            private static ColorBlock defaultCoinModeColors
            {
                get
                {
                    ColorBlock c = new ColorBlock();
                    c.normalColor = new Color32(255, 255, 255, 128);
                    c.highlightedColor = new Color32(255, 255, 255, 255);
                    c.pressedColor = new Color32(253, 183, 20, 255);
#if UNITY_2019_1_OR_NEWER
                    c.selectedColor = new Color32(253, 183, 20, 255);
#endif
                    c.disabledColor = new Color32(200, 200, 200, 128);
                    c.colorMultiplier = 1.0F;
                    c.fadeDuration = 0.1F;
                    return c;
                }
            }

            internal static Sprite defaultFrameSprite
            {
                get
                {
                    return Resources.Load<Sprite>("BackgroundFrameSprite");
                }
            }

            internal static Sprite defaultBackgroundSprite
            {
                get
                {
                    return Resources.Load<Sprite>("BackgroundFillSprite");
                }
            }

            internal static Font defaultFont
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Regular");
                }
            }

            public Sprite frameSprite = null;
            public Sprite backgroundSprite = null;
            public Color backgroundColor = new Color32(0, 0, 0, 50);
            public ColorBlock colors = defaultCoinModeColors;
            public Font font = null;
            public FontStyle fontStyle = FontStyle.Normal;
            public Color textColor = Color.white;
            public Color placeholderColor = new Color32(255, 255, 255, 128);
        }

        [System.Serializable]
        public class MessageTemplate
        {
            internal static Sprite defaultBackgroundSprite
            {
                get
                {
                    return Resources.Load<Sprite>("MessageBackgroundSprite");
                }
            }

            public Sprite backgroundSprite = null;
            public Color backgroundColor = new Color32(26, 26, 26, 255);
            public Color successColor = new Color(0.176F, 0.718F, 0.329F, 1.0F);
            public Color failureColor = new Color(0.718F, 0.176F, 0.176F, 1.0F);
        }

        [System.Serializable]
        public class FontFamily
        {
            internal static Font defaultThin
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Thin");
                }
            }

            internal static Font defaultLight
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Light");
                }
            }

            internal static Font defaultRegular
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Regular");
                }
            }

            internal static Font defaultMedium
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Medium");
                }
            }

            internal static Font defaultBold
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Bold");
                }
            }

            internal static Font defaultBlack
            {
                get
                {
                    return Resources.Load<Font>("Fonts/Roboto/Roboto-Black");
                }
            }

            public Font thin = null;
            public Font light = null;
            public Font regular = null;
            public Font medium = null;
            public Font bold = null;
            public Font black = null;
        }

        [System.Serializable]
        public class StylePreset
        {
            public Sprite backgroundSprite = null;
            public Color backgroundColor = defaultBackgroundColor;
            public Color accentColor = defaultAccentColor;
            public Sprite backgroundPatternSprite = null;
            public MessageTemplate messageTemplate = new MessageTemplate();
            public Color textColor = Color.white;
            public FontFamily fontFamily = new FontFamily();
            public ButtonTemplate titleLoginButton = new ButtonTemplate();
            public ButtonTemplate defaultButton = new ButtonTemplate();
            public List<ButtonTemplate> customButtons = new List<ButtonTemplate>();
            public InputFieldTemplate inputField = new InputFieldTemplate();

            public bool initialized { get; private set; } = false;

            public void ResetToDefault()
            {
                backgroundColor = defaultBackgroundColor;
                accentColor = defaultAccentColor;
                backgroundPatternSprite = null;
                messageTemplate = new MessageTemplate();
                textColor = Color.white;
                fontFamily = new FontFamily();
                titleLoginButton = new ButtonTemplate();
                defaultButton = new ButtonTemplate();
                customButtons = new List<ButtonTemplate>();
                inputField = new InputFieldTemplate();
                LoadDefaultAssets();
            }

            public void LoadDefaultAssets()
            {
                initialized = true;
                CheckResourceIsNull(backgroundSprite = defaultBackgroundSprite);

                CheckResourceIsNull(messageTemplate.backgroundSprite = MessageTemplate.defaultBackgroundSprite);

                CheckResourceIsNull(fontFamily.thin = FontFamily.defaultThin);
                CheckResourceIsNull(fontFamily.light = FontFamily.defaultLight);
                CheckResourceIsNull(fontFamily.regular = FontFamily.defaultRegular);
                CheckResourceIsNull(fontFamily.medium = FontFamily.defaultMedium);
                CheckResourceIsNull(fontFamily.bold = FontFamily.defaultBold);
                CheckResourceIsNull(fontFamily.black = FontFamily.defaultBlack);

                CheckResourceIsNull(defaultButton.sprite = ButtonTemplate.defaultSprite);
                CheckResourceIsNull(defaultButton.font = ButtonTemplate.defaultFont);

                CheckResourceIsNull(titleLoginButton.sprite = ButtonTemplate.defaultSprite);
                CheckResourceIsNull(titleLoginButton.font = ButtonTemplate.defaultTitleLoginFont);
                CheckResourceIsNull(titleLoginButton.colors = ButtonTemplate.titleLoginColors);
                CheckResourceIsNull(titleLoginButton.textColors = TextColorBlock.titleLoginColors);

                CheckResourceIsNull(inputField.backgroundSprite = InputFieldTemplate.defaultBackgroundSprite);
                CheckResourceIsNull(inputField.frameSprite = InputFieldTemplate.defaultFrameSprite);
                CheckResourceIsNull(inputField.font = InputFieldTemplate.defaultFont);
            }

            public StylePreset ShallowCopy()
            {
                return (StylePreset)MemberwiseClone();
            }

            private void CheckResourceIsNull(object resource)
            {
                if(resource == null)
                {
                    initialized = false;
                }
            }
        }

        // Helpers
        public static ColorBlock GetTintedDefaultColors(in Color tint)
        {
            ColorBlock c = defaultColorBlock;
            c.normalColor *= tint;
            c.highlightedColor *= tint;
            c.pressedColor *= tint;
#if UNITY_2019_1_OR_NEWER
            c.selectedColor *= tint;
#endif
            c.disabledColor *= tint;
            return c;
        }

        public static ColorMode GetColorMode(in Color color)
        {
            float luminance = color.r * 0.299F + color.g * 0.587F + color.b * 0.114F;
            return luminance > 0.729F ? ColorMode.Light : ColorMode.Dark;
        }

        // Returns black or white based on input color
        public static Color GetContastingColor(in Color color)
        {
            return GetColorMode(in color) == ColorMode.Light ? Color.black : Color.white;
        }        

        // Resources 
        private static Sprite defaultBackgroundSprite
        {
            get
            {
                return Resources.Load<Sprite>("MenuBackgroundSprite");
            }
        }

        // Advertisement Integration
        public static bool useAdvertiserColors { get; set; } = false;

        // Menu
        public static Sprite backgroundSprite { get { return instance._currentStyle.backgroundSprite; } }

        public static Color backgroundColor
        {
            get
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.primaryColorIsSet)
                    {
                        return ad.primaryColor;
                    }
                }
                return instance._currentStyle.backgroundColor;
            }
        }

        public static Sprite backgroundPatternSprite { get { return instance._currentStyle.backgroundPatternSprite; } }

        public static Color accentColor
        {
            get
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.secondaryColorIsSet)
                    {
                        return ad.secondaryColor;
                    }
                }
                return instance._currentStyle.accentColor;
            }
        }

        // Messages
        public static Sprite messageBackgroundSprite { get { return instance._currentStyle.messageTemplate.backgroundSprite; } }

        public static Color messageBackgroundColor { get { return instance._currentStyle.messageTemplate.backgroundColor; } }

        public static Color messageSuccessColor { get { return instance._currentStyle.messageTemplate.successColor; } }

        public static Color messageFailureColor { get { return instance._currentStyle.messageTemplate.failureColor; } }

        // Text Color 
        public static Color textColor { get { return instance._currentStyle.textColor; } }

        // Fonts
        public static Font thinFont { get { return instance._currentStyle.fontFamily.thin; } }

        public static Font lightFont { get { return instance._currentStyle.fontFamily.light; } }

        public static Font regularFont { get { return instance._currentStyle.fontFamily.regular; } }

        public static Font mediumFont { get { return instance._currentStyle.fontFamily.medium; } }

        public static Font boldFont { get { return instance._currentStyle.fontFamily.bold; } }

        public static Font blackFont { get { return instance._currentStyle.fontFamily.black; } }

        // Buttons
        public static Sprite defualtButtonSprite { get { return instance._currentStyle.defaultButton.sprite; } }

        public static ColorBlock defualtButtonColors 
        { 
            get 
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.secondaryColorIsSet)
                    {
                        return GetTintedDefaultColors(ad.secondaryColor);
                    }
                }
                return instance._currentStyle.defaultButton.colors; 
            } 
        }

        public static Font defualtButtonFont { get { return instance._currentStyle.defaultButton.font; } }

        public static FontStyle defualtButtonFontStyle { get { return instance._currentStyle.defaultButton.fontStyle; } }

        public static TextColorBlock defualtButtonTextColors 
        { 
            get 
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.secondaryColorIsSet)
                    {
                        return TextColorBlock.GetContrastingColor(ad.secondaryColor);
                    }
                }
                return instance._currentStyle.defaultButton.textColors; 
            } 
        }

        public static ColorMode defualtButtonLoadingColorMode { get { return instance._currentStyle.defaultButton.loadingColorMode; } }

        public static Sprite titleLoginButtonSprite { get { return instance._currentStyle.titleLoginButton.sprite; } }

        public static ColorBlock titleLoginButtonColors { get { return instance._currentStyle.titleLoginButton.colors; } }

        public static Font titleLoginButtonFont { get { return instance._currentStyle.titleLoginButton.font; } }

        public static FontStyle titleLoginButtonFontStyle { get { return instance._currentStyle.titleLoginButton.fontStyle; } }

        public static TextColorBlock titleLoginButtonTextColors { get { return instance._currentStyle.titleLoginButton.textColors; } }

        public static ColorMode titleLoginButtonLoadingColorMode { get { return instance._currentStyle.titleLoginButton.loadingColorMode; } }

        public static int customButtonTemplateCount { get { return instance._currentStyle.customButtons.Count; } }

        // Input Field
        public static Sprite inputFieldFrameSprite { get { return instance._currentStyle.inputField.frameSprite; } }

        public static Sprite inputFieldBackgroundSprite { get { return instance._currentStyle.inputField.backgroundSprite; } }

        public static Color inputFieldBackgroundColor { get { return instance._currentStyle.inputField.backgroundColor; } }

        public static ColorBlock inputFieldColors
        {
            get
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.secondaryColorIsSet)
                    {
                        return GetTintedDefaultColors(ad.secondaryColor);
                    }
                }
                return instance._currentStyle.inputField.colors;
            }
        }

        public static Font inputFieldFont { get { return instance._currentStyle.inputField.font; } }

        public static FontStyle inputFieldFontStyle { get { return instance._currentStyle.inputField.fontStyle; } }

        public static Color inputFieldTextColor
        {
            get
            {
                return instance._currentStyle.inputField.textColor;
            }
        }

        public static Color inputFieldPlaceholderColor
        {
            get
            {
                return instance._currentStyle.inputField.placeholderColor;
            }
        }

        internal static CoinModeMenuStyle instance
        {
            get
            {
                if (_instance == null)
                {
                    InitStyle();
                }
                else if(!_instance._currentStyle.initialized)
                {
                    _instance._currentStyle.LoadDefaultAssets();
                }
                return _instance;
            }
        }
        private static CoinModeMenuStyle _instance = null;

        public static bool ContainsButtonTemplate(int templateIndex)
        {
            return templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count;
        }

        public static Sprite GetButtonSprite(int templateIndex = -1)
        {
            if (templateIndex == -1)
            {
                return instance._currentStyle.defaultButton.sprite;
            }
            else if (templateIndex == -2)
            {
                return instance._currentStyle.titleLoginButton.sprite;
            }
            else if (templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count)
            {
                return instance._currentStyle.customButtons[templateIndex].sprite;
            }
            return null;
        }

        public static ColorBlock GetButtonColors(int templateIndex = -1)
        {
            if (templateIndex == -1)
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.secondaryColorIsSet)
                    {
                        return GetTintedDefaultColors(ad.secondaryColor);
                    }
                }
                return instance._currentStyle.defaultButton.colors;
            }
            else if (templateIndex == -2)
            {
                return instance._currentStyle.titleLoginButton.colors;
            }
            else if (templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count)
            {
                return instance._currentStyle.customButtons[templateIndex].colors;
            }
            return ColorBlock.defaultColorBlock;
        }

        public static Font GetButtonFont(int templateIndex = -1)
        {
            if (templateIndex == -1)
            {
                return instance._currentStyle.defaultButton.font;
            }
            else if (templateIndex == -2)
            {
                return instance._currentStyle.titleLoginButton.font;
            }
            else if (templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count)
            {
                return instance._currentStyle.customButtons[templateIndex].font;
            }
            return null;
        }

        public static FontStyle GetButtonFontStyle(int templateIndex = -1)
        {
            if (templateIndex == -1)
            {
                return instance._currentStyle.defaultButton.fontStyle;
            }
            else if (templateIndex == -2)
            {
                return instance._currentStyle.titleLoginButton.fontStyle;
            }
            else if (templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count)
            {
                return instance._currentStyle.customButtons[templateIndex].fontStyle;
            }
            return FontStyle.Normal;
        }

        public static TextColorBlock GetButtonTextColors(int templateIndex = -1)
        {
            if (templateIndex == -1)
            {
                if (Application.isPlaying && useAdvertiserColors && CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    if (ad.secondaryColorIsSet)
                    {
                        return TextColorBlock.GetContrastingColor(ad.secondaryColor);
                    }
                }
                return instance._currentStyle.defaultButton.textColors;
            }
            else if (templateIndex == -2)
            {
                return instance._currentStyle.titleLoginButton.textColors;
            }
            else if (templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count)
            {
                return instance._currentStyle.customButtons[templateIndex].textColors;
            }
            return TextColorBlock.defaultCoinModeColors;
        }

        public static ColorMode GetButtonLoadingColorMode(int templateIndex = -1)
        {
            if (templateIndex == -1)
            {
                return instance._currentStyle.defaultButton.loadingColorMode;
            }
            else if (templateIndex == -2)
            {
                return instance._currentStyle.titleLoginButton.loadingColorMode;
            }
            else if (templateIndex >= 0 && templateIndex < instance._currentStyle.customButtons.Count)
            {
                return instance._currentStyle.customButtons[templateIndex].loadingColorMode;
            }
            return ColorMode.Dark;
        }

        internal static void InitStyle()
        {
            CoinModeMenuStyle styleObject = Resources.Load("CoinModeMenuStyle", typeof(CoinModeMenuStyle)) as CoinModeMenuStyle;
            if (styleObject == null)
            {
                styleObject = CreateInstance<CoinModeMenuStyle>();
                styleObject._currentStyle.LoadDefaultAssets();

#if UNITY_EDITOR
                if (!Directory.Exists(Application.dataPath + "/CoinMode/Settings/Resources"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/CoinMode/Settings/Resources");
                }
                AssetDatabase.CreateAsset(styleObject, "Assets/CoinMode/Settings/Resources/CoinModeMenuStyle.asset");
#if UNITY_2020_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(styleObject);
#else
                EditorUtility.SetDirty(styleObject);
                AssetDatabase.SaveAssets();
#endif
                EditorUtility.FocusProjectWindow();
#endif
            }
            _instance = styleObject;
        }

        [SerializeField]
        [HideInInspector]
        private StylePreset _currentStyle = new StylePreset();

#if UNITY_EDITOR
        // Background
        internal void EditorSetBackgroundSprite(Sprite sprite)
        {
            _currentStyle.backgroundSprite = sprite;
        }

        internal void EditorSetBackgroundColor(Color color)
        {
            _currentStyle.backgroundColor = color;
        }

        internal void EditorSetBackgroundPatternSprite(Sprite sprite)
        {
            _currentStyle.backgroundPatternSprite = sprite;
        }

        internal void EditorSetAccentColor(Color color)
        {
            _currentStyle.accentColor = color;
        }

        // Messages
        internal void EditorSetMessageBackgroundSprite(Sprite sprite)
        {
            _currentStyle.messageTemplate.backgroundSprite = sprite;
        }

        internal void EditorSetMessageBackgroundColor(Color color)
        {
            _currentStyle.messageTemplate.backgroundColor = color;
        }

        internal void EditorSetMessageSuccessColor(Color color)
        {
            _currentStyle.messageTemplate.successColor = color;
        }

        internal void EditorSetMessageFailureColor(Color color)
        {
            _currentStyle.messageTemplate.failureColor = color;
        }

        internal void EditorSetTextColor(Color color)
        {
            _currentStyle.textColor = color;
        }

        // Font
        internal void EditorSetThinFont(Font font)
        {
            _currentStyle.fontFamily.thin = font;
        }

        internal void EditorSetLightFont(Font font)
        {
            _currentStyle.fontFamily.light = font;
        }

        internal void EditorSetRegularFont(Font font)
        {
            _currentStyle.fontFamily.regular = font;
        }

        internal void EditorSetMediumFont(Font font)
        {
            _currentStyle.fontFamily.medium = font;
        }

        internal void EditorSetBoldFont(Font font)
        {
            _currentStyle.fontFamily.bold = font;
        }

        internal void EditorSetBlackFont(Font font)
        {
            _currentStyle.fontFamily.black = font;
        }

        // Buttons
        internal void EditorAddCustomButtonTemplate()
        {
            ButtonTemplate newTemplate = new ButtonTemplate();
            newTemplate.sprite = ButtonTemplate.defaultSprite;
            newTemplate.font = ButtonTemplate.defaultFont;
            _currentStyle.customButtons.Add(newTemplate);
        }

        internal void EditorRemoveCustomButtonTemplate()
        {
            if (_currentStyle.customButtons.Count > 0)
            {
                _currentStyle.customButtons.RemoveAt(_currentStyle.customButtons.Count - 1);
            }
        }

        internal void EditorSetButtonSprite(Sprite sprite, int index = -1)
        {
            if (index == -1)
            {
                _currentStyle.defaultButton.sprite = sprite;
            }
            else if (index == -2)
            {
                _currentStyle.titleLoginButton.sprite = sprite;
            }
            else if (index >= 0 && index < _currentStyle.customButtons.Count)
            {
                _currentStyle.customButtons[index].sprite = sprite;
            }
        }

        internal void EditorSetButtonColors(ColorBlock colors, int index = -1)
        {
            if (index == -1)
            {
                _currentStyle.defaultButton.colors = colors;
            }
            else if (index == -2)
            {
                _currentStyle.titleLoginButton.colors = colors;
            }
            else if (index >= 0 && index < _currentStyle.customButtons.Count)
            {
                _currentStyle.customButtons[index].colors = colors;
            }
        }

        internal void EditorSetButtonFont(Font font, int index = -1)
        {
            if (index == -1)
            {
                _currentStyle.defaultButton.font = font;
            }
            else if (index == -2)
            {
                _currentStyle.titleLoginButton.font = font;
            }
            else if (index >= 0 && index < _currentStyle.customButtons.Count)
            {
                _currentStyle.customButtons[index].font = font;
            }
        }

        internal void EditorSetButtonFontStyle(FontStyle fontStyle, int index = -1)
        {
            if (index == -1)
            {
                _currentStyle.defaultButton.fontStyle = fontStyle;
            }
            else if (index == -2)
            {
                _currentStyle.titleLoginButton.fontStyle = fontStyle;
            }
            else if (index >= 0 && index < _currentStyle.customButtons.Count)
            {
                _currentStyle.customButtons[index].fontStyle = fontStyle;
            }
        }

        internal void EditorSetButtonTextColors(TextColorBlock colors, int index = -1)
        {
            if (index == -1)
            {
                _currentStyle.defaultButton.textColors = colors;
            }
            else if (index == -2)
            {
                _currentStyle.titleLoginButton.textColors = colors;
            }
            else if (index >= 0 && index < _currentStyle.customButtons.Count)
            {
                _currentStyle.customButtons[index].textColors = colors;
            }
        }

        internal void EditorSetButtonLoadingColorMode(ColorMode colorMode, int index = -1)
        {
            if (index == -1)
            {
                _currentStyle.defaultButton.loadingColorMode = colorMode;
            }
            else if (index == -2)
            {
                _currentStyle.titleLoginButton.loadingColorMode = colorMode;
            }
            else if (index >= 0 && index < _currentStyle.customButtons.Count)
            {
                _currentStyle.customButtons[index].loadingColorMode = colorMode;
            }
        }

        // Input Field
        internal void EditorSetInputFieldFrameSprite(Sprite sprite)
        {
            _currentStyle.inputField.frameSprite = sprite;
        }

        internal void EditorSetInputFieldBackgroundSprite(Sprite sprite)
        {
            _currentStyle.inputField.backgroundSprite = sprite;
        }

        internal void EditorSetInputFieldBackgroundColor(Color color)
        {
            _currentStyle.inputField.backgroundColor = color;
        }

        internal void EditorSetInputFieldColors(in ColorBlock colors)
        {
            _currentStyle.inputField.colors = colors;
        }

        internal void EditorSetInputFieldFont(Font font)
        {
            _currentStyle.inputField.font = font;
        }

        internal void EditorSetInputFieldFontStyle(FontStyle fontStyle)
        {
            _currentStyle.inputField.fontStyle = fontStyle;
        }

        internal void EditorSetInputFieldTextColor(Color color)
        {
            _currentStyle.inputField.textColor = color;
        }

        internal void EditorSetInputFieldPlaceholderColor(Color color)
        {
            _currentStyle.inputField.placeholderColor = color;
        }

        // Reset & Presets
        internal void EditorResetToDefault()
        {
            _currentStyle.ResetToDefault();
        }

        internal void EditorLoadFromPreset(CoinModeMenuStylePreset preset)
        {
            _currentStyle = preset.preset.ShallowCopy();
        }

        internal StylePreset EditorGetStyle()
        {
            return _currentStyle.ShallowCopy();
        }

        // Checkbox

        // Input box

        // Control Button (Number up / down)

        // Scroll Bar

        // Box background color (Scroll rects, containers etc)
#endif
    }
}

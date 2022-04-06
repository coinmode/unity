using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM Text")]
    public class CoinModeText : Text
    {
        public bool useAccentColor { get { return _useAccentColor; } }

        [SerializeField]
        private bool _useAccentColor = false;

        public FontFamilyStyle fontFamilyStyle { get { return _fontFamilyStyle; } }

        [SerializeField]
        private FontFamilyStyle _fontFamilyStyle = FontFamilyStyle.Regular;

        [SerializeField]
        private bool ignoreMenuStyle = false;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Initialise();
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            Initialise();
        }

        private void Initialise()
        {
            if(!ignoreMenuStyle)
            {
                SetFontFamilyStyle(_fontFamilyStyle);
                if (useAccentColor)
                {
                    SetColor(CoinModeMenuStyle.accentColor);
                }
                else
                {
                    SetColor(CoinModeMenuStyle.textColor);
                }
            }            
        }

        private void SetColor(Color color)
        {
            this.color = color;
        }

        public void SetFontFamilyStyle(FontFamilyStyle fontFamilyStyle)
        {
            _fontFamilyStyle = fontFamilyStyle;

            switch (fontFamilyStyle)
            {
                case FontFamilyStyle.Thin:
                    font = CoinModeMenuStyle.thinFont;
                    break;
                case FontFamilyStyle.Light:
                    font = CoinModeMenuStyle.lightFont;
                    break;
                case FontFamilyStyle.Regular:
                    font = CoinModeMenuStyle.regularFont;
                    break;
                case FontFamilyStyle.Medium:
                    font = CoinModeMenuStyle.mediumFont;
                    break;
                case FontFamilyStyle.Bold:
                    font = CoinModeMenuStyle.boldFont;
                    break;
                case FontFamilyStyle.Black:
                    font = CoinModeMenuStyle.blackFont;
                    break;
                default:
                    font = CoinModeMenuStyle.regularFont;
                    break;
            }
        }

        public void SetText(string text, Color? color = null)
        {
            if(this.text != null)
            {
                this.text = text;
                if (color != null)
                {
                    SetTextColor(color.Value);
                }
            }            
        }

        public void SetTextColor(Color color)
        {
            this.color = color;
        }
    }
}
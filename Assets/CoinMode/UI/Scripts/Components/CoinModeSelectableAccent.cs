using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM Selectable Accent")]
    public class CoinModeSelectableAccent : CoinModeAccentController
    {    
        [SerializeField]
        private ColorBlock baseColors = CoinModeMenuStyle.defaultColorBlock;

        [SerializeField]
        private Selectable targetSelectable = null;
        
        public override void SetAccent(Color accentColor)
        {
            if(targetSelectable != null)
            {
                ColorBlock colors = targetSelectable.colors;
                colors.normalColor = CoinModeMenuStyle.accentColor * baseColors.normalColor;
                colors.highlightedColor = CoinModeMenuStyle.accentColor * baseColors.highlightedColor;
                colors.pressedColor = CoinModeMenuStyle.accentColor * baseColors.pressedColor;
#if UNITY_2019_1_OR_NEWER
                colors.selectedColor = CoinModeMenuStyle.accentColor * baseColors.selectedColor;
#endif
                colors.disabledColor = CoinModeMenuStyle.accentColor * baseColors.disabledColor;
                targetSelectable.colors = colors;
            }
        }
    }
}

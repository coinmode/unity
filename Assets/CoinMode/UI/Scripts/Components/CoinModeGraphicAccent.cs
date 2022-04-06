using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM Graphic Accent")]
    public class CoinModeGraphicAccent : CoinModeAccentController
    {
        [SerializeField]
        private Graphic targetGraphic = null;
        
        public override void SetAccent(Color accentColor)
        {
            if(targetGraphic != null)
            {
                float alpha = targetGraphic.color.a;
                targetGraphic.color = new Color(accentColor.r, accentColor.g, accentColor.b, alpha);
            }
        }
    }
}

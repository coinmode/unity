using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM TitleLoginButton")]
    public class TitleLoginButton : CoinModeButton
    {
        [SerializeField]
        private GameObject additionalText = null;

        protected override void SetTextVisibility(bool visible)
        {
            base.SetTextVisibility(visible);
            if(additionalText != null)
            {
                additionalText.SetActive(visible);
            }
        }
    }
}

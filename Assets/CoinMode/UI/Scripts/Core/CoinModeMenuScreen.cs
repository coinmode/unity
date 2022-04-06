using System;
using System.Collections.Generic;

namespace CoinMode.UI
{    
    public abstract class CoinModeMenuScreen : CoinModeScreen
    {
        new public CoinModeMenu controller { get { return _controller; } }
        [NonSerialized]
        private CoinModeMenu _controller = null;

        private bool styledToAdvertiser = false;

        protected void StyleToMatchAdvertiser()
        {
            if (CoinModeManager.advertisementComponent.advertDataAvailable)
            {
                styledToAdvertiser = true;
                CoinModeMenuStyle.useAdvertiserColors = true;
                DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                if (ad.primaryColorIsSet)
                {
                    controller.SetMenuColor(ad.primaryColor);
                }

                if (ad.secondaryColorIsSet)
                {
                    List<CoinModeButton> buttons = new List<CoinModeButton>();
                    GetComponentsInChildren(buttons);
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        if (buttons[i].initialised)
                        {
                            buttons[i].RefreshStylye();
                        }
                    }

                    List<CoinModeAccentController> accents = new List<CoinModeAccentController>();
                    GetComponentsInChildren(accents);
                    for (int i = 0; i < accents.Count; i++)
                    {
                        if (accents[i].initialised)
                        {
                            accents[i].RefreshAccent();
                        }
                    }
                }           
            }
        }

        protected void RevertStyle()
        {
            if(styledToAdvertiser)
            {
                controller.ResetMenuColor();

                List<CoinModeButton> buttons = new List<CoinModeButton>();
                GetComponentsInChildren(buttons);
                for (int i = 0; i < buttons.Count; i++)
                {
                    if (buttons[i].initialised)
                    {
                        buttons[i].RefreshStylye();
                    }
                }

                List<CoinModeAccentController> accents = new List<CoinModeAccentController>();
                GetComponentsInChildren(accents);
                for (int i = 0; i < accents.Count; i++)
                {
                    if (accents[i].initialised)
                    {
                        accents[i].RefreshAccent();
                    }
                }
            }
        }

        internal override void InitScreen(CoinModeScreenController controller)
        {
            base.InitScreen(controller);
            _controller = base.controller as CoinModeMenu;
        }
    }
}
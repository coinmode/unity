using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM CurrencyDropDown")]
    public class CurrencyDropDown : CoinModeDropDown
    {
        public delegate void WalletEvent(Wallet currency);

        public WalletEvent onCurrencySelected;

        private WalletComponent walletComponent = null;
        private List<Wallet> currencies = new List<Wallet>(8);

        public bool Init(WalletComponent walletComponent)
        {
            if(walletComponent != null)
            {
                this.walletComponent = walletComponent;
                options.Clear();
                currencies.Clear();
                foreach(Wallet currency in walletComponent.wallets)
                {
                    if(currency != null)
                    {
                        OptionData data = new OptionData();
                        data.text = currency.fullName;
                        options.Add(data);
                        currencies.Add(currency);
                    }
                }
                onValueChanged.RemoveListener(OnCurrencyValueChanged);
                onValueChanged.AddListener(OnCurrencyValueChanged);
                return true;
            }
            return false;
        }

        public bool SetCurrency(string walletId)
        {
            for(int i = 0; i < currencies.Count; i++)
            {
                if(currencies[i].walletId == walletId)
                {
                    value = i;
                    return true;
                }
            }
            return false;
        }

        private void OnCurrencyValueChanged(int optionIndex)
        {
            onCurrencySelected?.Invoke(currencies[optionIndex]);
        }
    }
}

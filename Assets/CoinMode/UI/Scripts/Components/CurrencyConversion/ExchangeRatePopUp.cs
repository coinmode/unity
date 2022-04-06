using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM ExchangeRatePopUp")]
    public class ExchangeRatePopUp : CoinModeWindow
    {
        public struct WindowData
        {
            public Wallet wallet { get; private set; }
            public string currencyKey { get; private set; }
            public double source { get; private set; }

            public WindowData(Wallet wallet, string currencyKey, double source)
            {
                this.wallet = wallet;
                this.currencyKey = currencyKey;
                this.source = source;
            }
        }

        [SerializeField]
        CoinModeText baseCurrencyText = null;

        [SerializeField]
        CoinModeText exchangeRateText = null;

        [SerializeField]
        CoinModeText sourceAmountText = null;

        public override bool requiresData { get; } = true;
        private WindowData windowData = new WindowData();

        private CurrencyExchangeData defaultUnitExchange;
        private CurrencyExchangeData targetCurrencyExchange;

        protected override void OnOpen(object data)
        {
            windowData = ValidateObject<WindowData>(data);
            SetUp();
        }

        protected override bool OnUpdateData(object data)
        {
            windowData = ValidateObject<WindowData>(data);
            SetUp();
            return true; 
        }

        public override bool IsValidData(object data)
        {
            return IsValidObject<WindowData>(data);
        }

        private void SetUp()
        {
            if (windowData.wallet != null)
            {
                if (windowData.wallet.exhangeData.TryGetValue(windowData.wallet.currencyKey, out defaultUnitExchange) &&
                    windowData.wallet.exhangeData.TryGetValue(windowData.currencyKey, out targetCurrencyExchange))
                {
                    if (baseCurrencyText != null)
                    {
                        baseCurrencyText.SetText(string.Format(baseCurrencyText.text, windowData.wallet.fullName));
                    }

                    if (exchangeRateText != null)
                    {
                        string sourceUnitString = "1" + defaultUnitExchange.shortCode;

                        double oneDefaultUnit = 1.0D / defaultUnitExchange.conversion.Value;
                        string formattedValue = string.Format("{0:F" + targetCurrencyExchange.decimals.ToString() + "}", oneDefaultUnit * targetCurrencyExchange.conversion.Value);
                        string targetUnitString = targetCurrencyExchange.prefix + formattedValue + targetCurrencyExchange.suffix;

                        exchangeRateText.SetText(string.Format(exchangeRateText.text, sourceUnitString, targetUnitString));
                    }

                    if (sourceAmountText != null)
                    {
                        // Prime example of why we need info regarding what the currency unit is!
                        sourceAmountText.SetText(string.Format(sourceAmountText.text, windowData.source.ToString(), windowData.wallet.smallestUnitKey));
                    }
                }
            }
        }
    }
}

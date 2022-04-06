using CoinMode.NetApi;
using System;
using System.Collections.Generic;

namespace CoinMode
{
    public delegate void WalletEvent(WalletComponent walletComponent);
    public delegate void WalletFailureEvent(WalletComponent walletComponent, CoinModeError error);

    [System.Serializable]
    public class WalletComponent : CoinModeComponent
    {      
        public enum WalletState
        {
            Clean,
            RetrievingData,
            Ready,
        }

        public class BasicCurrencyInfo
        {
            public string currencyKey { get; private set; } = "";
            public string prefix { get; private set; } = "";
            public string suffix { get; private set; } = "";
            public string shortcode { get; private set; } = "";
            public int decimals { get; private set; } = 0;

            public BasicCurrencyInfo(string currencyKey, string prefix, string suffix, string shortcode, int decimals)
            {
                this.currencyKey = currencyKey;
                this.prefix = prefix;
                this.suffix = suffix;
                this.shortcode = shortcode;
                this.decimals = decimals;
            }

            public BasicCurrencyInfo() { }
        }

        public IEnumerable<Wallet> wallets
        {
            get 
            {
                foreach (KeyValuePair<string, Wallet> pair in _wallets.wallets)
                {
                    yield return pair.Value;
                }
            }
        }

        // Dictionary of crypto's and their exchange rate, the key is the wallet id
        private Wallets _wallets = null;

        // Information regarding currencies used for display and conversion
        private Dictionary<string, BasicCurrencyInfo> basicCurrencyInfo = new Dictionary<string, BasicCurrencyInfo>();

        public WalletState walletState { get; private set; } = WalletState.Clean;

        private WalletEvent getExchangeRateSuccess = null;
        private WalletFailureEvent getExchangeRateFailure = null;

        internal WalletComponent() { }

        internal bool GetWalletData(WalletEvent onSuccess, WalletFailureEvent onFailure)
        {
            if (walletState != WalletState.Clean)
            {
                if (walletState == WalletState.RetrievingData)
                {
                    getExchangeRateSuccess -= onSuccess;
                    getExchangeRateSuccess += onSuccess;

                    getExchangeRateFailure -= onFailure;
                    getExchangeRateFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("WalletComponent", "GetWalletData", "Cannot get wallet data while wallet component is {9}", walletState);
                    return false;
                }
            }

            walletState = WalletState.RetrievingData;

            getExchangeRateSuccess = onSuccess;
            getExchangeRateFailure = onFailure;
            CoinModeManager.SendRequest(Info.GetExchangeRates(OnGetExchangeRateSuccess, OnGetExchangeRateFailure));
            return true;
        }

        private void OnGetExchangeRateSuccess(Info.GetExchangeRatesResponse response)
        {
            walletState = WalletState.Ready;
            _wallets = response.properties;
            if (_wallets != null)
            {
                foreach (KeyValuePair<string, Wallet> wallet in _wallets.wallets)
                {
                    foreach (KeyValuePair<string, CurrencyExchangeData> exchange in wallet.Value.exhangeData)
                    {
                        if (!basicCurrencyInfo.ContainsKey(exchange.Key))
                        {
                            basicCurrencyInfo.Add(exchange.Key, new BasicCurrencyInfo(exchange.Key, exchange.Value.prefix, exchange.Value.suffix, exchange.Value.shortCode,
                                exchange.Value.decimals != null ? exchange.Value.decimals.Value : 0));
                        }
                    }
                }
            }
            getExchangeRateSuccess?.Invoke(this);
        }

        private void OnGetExchangeRateFailure(CoinModeErrorResponse errorResponse)
        {
            walletState = WalletState.Clean;
            getExchangeRateFailure?.Invoke(this, new CoinModeError(errorResponse));
        }

        public bool TryGetWallet(string walletId, out Wallet wallet)
        {
            return _wallets.wallets.TryGetValue(walletId, out wallet);
        }

        public bool ConvertBaseUnitToCurrency(string sourceWalletId, double value, string targetCurrencyKey, out CurrencyConversion conversion)
        {
            Wallet wallet = null;
            _wallets.wallets.TryGetValue(sourceWalletId, out wallet);
            return ConvertBaseUnitToCurrency(wallet, value, targetCurrencyKey, out conversion);
        }

        public bool ConvertBaseUnitToCurrency(Wallet sourceWallet, double value, string targetCurrencyKey, out CurrencyConversion conversion)
        {
            if (sourceWallet != null)
            {
                CurrencyExchangeData target = null;
                if (sourceWallet.exhangeData.TryGetValue(targetCurrencyKey, out target))
                {
                    conversion = new CurrencyConversion(value, target);
                    return true;
                }
            }
            conversion = new CurrencyConversion();
            return false;
        }

        public bool ConvertCurrencyToBaseUnit(string sourceCurrencyKey, double sourceValue, string targetWalletId, out double targetValue)
        {
            Wallet wallet = null;
            _wallets.wallets.TryGetValue(targetWalletId, out wallet);
            return ConvertCurrencyToBaseUnit(sourceCurrencyKey, sourceValue, wallet, out targetValue);
        }

        public bool ConvertCurrencyToBaseUnit(string sourceCurrencyKey, double sourceValue, Wallet targetWallet, out double targetValue)
        {
            if (targetWallet != null)
            {
                CurrencyExchangeData source = null;
                if (targetWallet.exhangeData.TryGetValue(sourceCurrencyKey, out source))
                {
                    targetValue = Math.Round(sourceValue / source.conversion.Value);
                    return true;
                }
            }
            targetValue = 0.0D;
            return false;
        }

        public bool ConvertBaseUnitToDefaultUnit(Wallet sourceWallet, double value, out CurrencyConversion conversion)
        {
            if (sourceWallet != null)
            {
                CurrencyExchangeData target = null;
                if (sourceWallet.exhangeData.TryGetValue(sourceWallet.currencyKey, out target))
                {
                    conversion = new CurrencyConversion(value, target);
                    return true;
                }
            }
            conversion = new CurrencyConversion();
            return false;
        }

        public double ConvertDefaultUnitToBaseUnit(Wallet sourceWallet, double sourceValue)
        {
            if (sourceWallet != null)
            {
                CurrencyExchangeData source = null;
                if (sourceWallet.exhangeData.TryGetValue(sourceWallet.currencyKey, out source))
                {
                    return Math.Round(sourceValue / source.conversion.Value);
                }
            }
            return 0.0D;
        }

        public double GetOneDefaultUnitInBaseUnit(Wallet sourceWallet)
        {
            if (sourceWallet != null)
            {
                CurrencyExchangeData defaultUnit = null;
                if (sourceWallet.exhangeData.TryGetValue(sourceWallet.currencyKey, out defaultUnit))
                {
                    return Math.Round(1.0D / defaultUnit.conversion.Value);
                }
            }
            return 0.0D;
        }

        public bool TryGetCurrencyInfo(string currencyKey, out BasicCurrencyInfo currencyInfo)
        {
            return basicCurrencyInfo.TryGetValue(currencyKey, out currencyInfo);
        }

        [System.Serializable]
        public struct CurrencyConversion
        {
            // Source currency data
            public double sourceValue
            {
                get { return _sourceValue; }
                set
                {
                    _sourceValue = value;
                    targetValue = CoinModeWallet.RoundTowardZero(_sourceValue * conversion, targetDecimals);
                }
            }
            double _sourceValue;

            // Target currency data
            public double targetValue { get; private set; }
            public string targetCurrencyKey { get; private set; }
            public string targetPrefix { get; private set; }
            public string targetSuffix { get; private set; }
            public string targetShortCode { get; private set; }
            public int targetDecimals { get; private set; }
            public bool targetApprox { get; private set; }
            public string targetCurrencyString
            {
                get
                {
                    string formattedValue = string.Format("{0:F" + targetDecimals.ToString() + "}", targetValue);
                    return (targetApprox ? "~" : "") + targetPrefix + formattedValue + targetSuffix;
                }
            }

            // Source to target conversion rate
            public double conversion { get; }

            internal CurrencyConversion(double value, CurrencyExchangeData targetCurrency)
            {
                targetCurrencyKey = targetCurrency.currencyKey != null ? targetCurrency.currencyKey : "";
                targetPrefix = targetCurrency.prefix != null ? targetCurrency.prefix : "";
                targetSuffix = targetCurrency.suffix != null ? targetCurrency.suffix : "";
                targetShortCode = targetCurrency.shortCode != null ? targetCurrency.shortCode : "";
                targetDecimals = targetCurrency.decimals != null ? targetCurrency.decimals.Value : 0;
                targetApprox = targetCurrency.approxConversion;

                conversion = targetCurrency.conversion.Value;

                _sourceValue = value;
                targetValue = CoinModeWallet.RoundTowardZero(value * conversion, targetDecimals);
            }
        }
    }    

    //[System.Serializable]
    //public struct CurrencyConversion
    //{
    //    // Source currency data
    //    public double sourceValue
    //    {
    //        get { return _sourceValue; }
    //        set
    //        {
    //            _sourceValue = value;
    //            targetValue = Math.Round(_sourceValue * conversion, targetDecimals);
    //        }
    //    }
    //    double _sourceValue;

    //    public string sourceCurrencyKey { get; private set; }
    //    public string sourcePrefix { get; private set; }
    //    public string sourceSuffix { get; private set; }
    //    public string sourceShortCode { get; private set; }
    //    public int sourceDecimals { get; private set; }
    //    public bool sourceApprox { get; private set; }
    //    public string sourceCurrencyString
    //    {
    //        get
    //        {
    //            string formattedValue = string.Format("{0:F" + sourceDecimals.ToString() + "}", _sourceValue);
    //            return (sourceApprox ? "~" : "") + sourcePrefix + formattedValue + sourceSuffix;
    //        }
    //    }

    //    // Target currency data
    //    public double targetValue { get; private set; }
    //    public string targetCurrencyKey { get; private set; }
    //    public string targetPrefix { get; private set; }
    //    public string targetSuffix { get; private set; }
    //    public string targetShortCode { get; private set; }
    //    public int targetDecimals { get; private set; }
    //    public bool targetApprox { get; private set; }
    //    public string targetCurrencyString
    //    {
    //        get
    //        {
    //            string formattedValue = string.Format("{0:F" + targetDecimals.ToString() + "}", targetValue);
    //            return (targetApprox ? "~" : "") + targetPrefix + formattedValue + targetSuffix;
    //        }
    //    }

    //    // Source to target conversion rate
    //    public double conversion { get; }

    //    internal CurrencyConversion(double value, double conversion, CurrencyExchangeData sourceCurrency, CurrencyExchangeData targetCurrency)
    //    {
    //        sourceCurrencyKey = sourceCurrency.currencyKey;
    //        sourcePrefix = sourceCurrency.prefix;
    //        sourceSuffix = sourceCurrency.suffix;
    //        sourceShortCode = sourceCurrency.shortCode;
    //        sourceDecimals = sourceCurrency.decimals.Value;
    //        sourceApprox = sourceCurrency.approxConversion;

    //        targetCurrencyKey = targetCurrency.currencyKey;
    //        targetPrefix = targetCurrency.prefix;
    //        targetSuffix = targetCurrency.suffix;
    //        targetShortCode = targetCurrency.shortCode;
    //        targetDecimals = targetCurrency.decimals.Value;
    //        targetApprox = targetCurrency.approxConversion;

    //        this.conversion = conversion;

    //        _sourceValue = value;
    //        targetValue = Math.Round(value * conversion, targetDecimals);
    //    }
    //}
}
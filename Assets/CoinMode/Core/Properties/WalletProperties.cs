using System.Collections.Generic;
using LightJson;

namespace CoinMode
{
    [System.Serializable]
    public class Wallets : CoinModeProperties
    {
        public Dictionary<string, Wallet> wallets { get; private set; } = null;

        internal Wallets() { }

        internal override void FromJson(JsonObject json)
        {
            this.wallets = new Dictionary<string, Wallet>();

            JsonObject wallets = json["wallets"];
            if(wallets != null)
            {
                foreach (KeyValuePair<string, JsonValue> pair in wallets)
                {
                    JsonObject walletObject = pair.Value;
                    if (walletObject != null)
                    {
                        Wallet walletProperties = new Wallet(pair.Key);
                        walletProperties.FromJson(walletObject);
                        this.wallets[pair.Key] = walletProperties;
                    }
                }
            }            
        }
    }

    [System.Serializable]
    public class Wallet : CoinModeProperties
    {
        public string walletId { get; private set; } = null;
        public string fullName { get; private set; } = null;
        public string shortCode { get; private set; } = null;
        public bool? isTest { get; private set; } = null;
        public string currencyKey { get; private set; } = null;
        public string smallestUnitKey { get; private set; } = null;
        public Dictionary<string, CurrencyExchangeData> exhangeData { get; private set; } = null;

        internal Wallet(string walletId)
        {
            this.walletId = walletId;
        }

        internal override void FromJson(JsonObject json)
        {
            fullName = json["full_name"];
            shortCode = json["shortcode"];
            isTest = json["is_test"];
            currencyKey = json["default_fx"];
            smallestUnitKey = json["smallest_units"];            

            JsonObject exchangeRatesObject = json["exchange"];
            if(exchangeRatesObject != null)
            {
                exhangeData = new Dictionary<string, CurrencyExchangeData>();
                foreach (KeyValuePair<string, JsonValue> pair in exchangeRatesObject)
                {
                    JsonObject exchangeRateObject = pair.Value;
                    if (exchangeRateObject != null)
                    {
                        bool approx = pair.Key != currencyKey && pair.Key != smallestUnitKey;
                        CurrencyExchangeData exchange = new CurrencyExchangeData(pair.Key, approx);
                        exchange.FromJson(exchangeRateObject);
                        exhangeData.Add(pair.Key, exchange);
                    }
                }
            }            
        }
    }

    [System.Serializable]
    public class CurrencyExchangeData : CoinModeProperties 
    {
        public string currencyKey { get; private set; } = null;
        public string prefix { get; private set; } = null;
        public bool? isTest { get; private set; } = null;
        public double? conversion { get; private set; } = null;
        public string suffix { get; private set; } = null;
        public string shortCode { get; private set; } = null;
        public int? decimals { get; private set; } = null;
        public bool approxConversion { get; private set; } = false;

        internal CurrencyExchangeData(string currencyKey, bool approximate)
        {
            this.currencyKey = currencyKey;
            this.approxConversion = approximate;
        }

        internal override void FromJson(JsonObject json)
        {
            prefix = json["prefix"];
            isTest = json["is_test"];
            conversion = json["conversion"];
            suffix = json["postfix"];
            shortCode = json["shortcode"];
            decimals = json["decimals"];
        }
    }    
}

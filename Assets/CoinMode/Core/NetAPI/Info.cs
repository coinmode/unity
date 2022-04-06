using LightJson;

namespace CoinMode.NetApi
{
    public static partial class Info
    {
        public delegate void GetDisplayCurrenciesSuccess(GetDisplayCurrenciesResponse response);

        public static GetDisplayCurrenciesRequest GetDisplayCurrencies(GetDisplayCurrenciesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetDisplayCurrenciesRequest(onSuccess, onFailure);
        }

        public class GetDisplayCurrenciesRequest : CoinModeRequest<GetDisplayCurrenciesResponse>
        {
            private GetDisplayCurrenciesSuccess onRequestSuccess;

            internal GetDisplayCurrenciesRequest(GetDisplayCurrenciesSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "info/get_display_currencies";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
            }

            protected override GetDisplayCurrenciesResponse ConstructSuccessResponse()
            {
                return new GetDisplayCurrenciesResponse();
            }

            protected override void RequestSuccess(GetDisplayCurrenciesResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetDisplayCurrenciesResponse : CoinModeResponse
        {            
            internal GetDisplayCurrenciesResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);                
            }
        }

        public delegate void GetExchangeRatesSuccess(GetExchangeRatesResponse response);

        public static GetExchangeRatesRequest GetExchangeRates(string walletId, GetExchangeRatesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetExchangeRatesRequest(walletId, onSuccess, onFailure);
        }

        public static GetExchangeRatesRequest GetExchangeRates(GetExchangeRatesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetExchangeRatesRequest(null, onSuccess, onFailure);
        }        

        public class GetExchangeRatesRequest : CoinModeRequest<GetExchangeRatesResponse>
        {
            private GetExchangeRatesSuccess onRequestSuccess;            

            internal GetExchangeRatesRequest(string walletId, GetExchangeRatesSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "info/get_exchange_rates";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("wallet", walletId);
            }

            protected override GetExchangeRatesResponse ConstructSuccessResponse()
            {
                return new GetExchangeRatesResponse();
            }

            protected override void RequestSuccess(GetExchangeRatesResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetExchangeRatesResponse : CoinModeResponse
        {
            public Wallets properties { get; } = new Wallets();

            internal GetExchangeRatesResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                properties.FromJson(json);
            }
        }
    }    
}
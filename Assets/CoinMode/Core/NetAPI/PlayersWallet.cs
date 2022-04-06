using LightJson;
using System.Collections.Generic;

namespace CoinMode.NetApi
{
    public static partial class PlayersWallet
    {
        public delegate void GetDepositAddressSuccess(GetDepositAddressResponse response);

        public static GetDepositAddressRequest GetLightningDepositAddress(string playToken, string walletId, bool? forceNewAddress, int? lightningDepositAmount,
            GetDepositAddressSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetDepositAddressRequest(playToken, walletId, forceNewAddress, true, lightningDepositAmount, onSuccess, onFailure);
        }

        public static GetDepositAddressRequest GetDepositAddress(string playToken, string walletId, bool? forceNewAddress, 
            GetDepositAddressSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetDepositAddressRequest(playToken, walletId, forceNewAddress, null, null, onSuccess, onFailure);
        }

        public class GetDepositAddressRequest : CoinModeRequest<GetDepositAddressResponse>
        {
            private GetDepositAddressSuccess onRequestSuccess;

            internal GetDepositAddressRequest(string playToken, string walletId, bool? forceNewAddress, bool? useLightning, int? lightningDepositAmount,
            GetDepositAddressSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/wallet/get_deposit_address";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("wallet", walletId);
                requestJson.AddIfNotNull("new_address", forceNewAddress);
                requestJson.AddIfNotNull("lightning", useLightning);
                requestJson.AddIfNotNull("requested_amount", lightningDepositAmount);
            }

            protected override GetDepositAddressResponse ConstructSuccessResponse()
            {
                return new GetDepositAddressResponse();
            }

            protected override void RequestSuccess(GetDepositAddressResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetDepositAddressResponse : CoinModeResponse
        {
            public string walletId { get; private set; } = null;
            public string address { get; private set; } = null;
            public bool? addressUsable { get; private set; } = null;

            internal GetDepositAddressResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                walletId = json["wallet"];
                address = json["address"];
                addressUsable = json["deposit_address_usable"];
            }
        }

        public delegate void GetBalancesSuccess(GetBalancesResponse response);

        public static GetBalancesRequest GetBalances(string playToken, string walletId, string otherPlayerId, string otherRoundId, GetBalancesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetBalancesRequest(playToken, walletId, otherPlayerId, otherRoundId, onSuccess, onFailure);
        }

        public static GetBalancesRequest GetBalances(string playToken, string walletId, GetBalancesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetBalancesRequest(playToken, walletId, null, null, onSuccess, onFailure);
        }

        public class GetBalancesRequest : CoinModeRequest<GetBalancesResponse>
        {
            private GetBalancesSuccess onRequestSuccess;            

            internal GetBalancesRequest(string playToken, string walletId, string otherPlayerId, string otherRoundId, GetBalancesSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/wallet/get_balances";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("wallet", walletId);
                requestJson.AddIfNotNull("other_player_id", otherPlayerId);
                requestJson.AddIfNotNull("other_round_id", otherRoundId);
            }

            protected override GetBalancesResponse ConstructSuccessResponse()
            {
                return new GetBalancesResponse();
            }

            protected override void RequestSuccess(GetBalancesResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetBalancesResponse : CoinModeResponse
        {
            public string walletOwnerId { get; private set; } = null;
            public Dictionary<string, WalletBalances> balances { get; private set; } = null;
            public string filter { get; private set; } = null;
            public bool? newUser { get; private set; } = null;
            public bool? balanceFound { get; private set; } = null;

            internal GetBalancesResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                walletOwnerId = json["wallet_id"];
                JsonObject balancesObject = json["balances"];
                if(balancesObject != null)
                {
                    balances = new Dictionary<string, WalletBalances>();
                    foreach(KeyValuePair<string, JsonValue> pairs in balancesObject)
                    {
                        JsonObject walletObject = pairs.Value;
                        if(walletObject != null)
                        {
                            WalletBalances newBalances = new WalletBalances();
                            newBalances.FromJson(walletObject);
                            balances.Add(pairs.Key, newBalances);
                        }
                    }
                }
                filter = json["filter"];
                newUser = json["newuser"];
                balanceFound = json["balance_found"];
            }
        }
    }    
}
using LightJson;
using System.Collections.Generic;

namespace CoinMode.NetApi
{
    public static partial class PlayersWalletTransferFunds
    {
        public delegate void VerifyTransferSuccess(VerifyTransferResponse response);

        public static VerifyTransferRequest Verify(string playToken, string pendingPaymentId, Dictionary<string, string> verificationKeys, 
            VerifyTransferSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new VerifyTransferRequest(playToken, pendingPaymentId, verificationKeys, onSuccess, onFailure);
        }

        public class VerifyTransferRequest : CoinModeRequest<VerifyTransferResponse>
        {
            private VerifyTransferSuccess onRequestSuccess;

            internal VerifyTransferRequest(string playToken, string pendingPaymentId,  Dictionary<string, string> verificationKeys, 
                VerifyTransferSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/wallet/transfer_funds/verify";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("pending_payment_id", pendingPaymentId);
                if (verificationKeys != null)
                {
                    foreach (KeyValuePair<string, string> pair in verificationKeys)
                    {
                        requestJson.AddIfNotNull(pair.Key, pair.Value);
                    }
                }
            }

            protected override VerifyTransferResponse ConstructSuccessResponse()
            {
                return new VerifyTransferResponse();
            }

            protected override void RequestSuccess(VerifyTransferResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class VerifyTransferResponse : CoinModeResponse
        {
            public string pendingPaymentId { get; private set; } = null;
            public string[] failedVerification { get; private set; } = null;
            public string paymentStatusText { get; private set; } = null;
            public int? paymentStatusId { get; private set; } = null;

            internal VerifyTransferResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                pendingPaymentId = json["pending_payment_id"];
                JsonArray fV = json["array_failed_tests"];
                if(fV != null)
                {
                    failedVerification = new string[fV.Count];
                    for (int i = 0; i < fV.Count; i++)
                    {
                        failedVerification[i] = fV[i];
                    }
                }
                paymentStatusText = json["status_text"];
                paymentStatusId = json["status_id"];
            }
        }

        public delegate void RequestTransferSuccess(RequestTransferResponse response);

        public static RequestTransferRequest Request(string playToken, string targetPlayerId, string messageForSender, string messageForReceiver,
                string messageForCoinMode, double? amount, string walletId, RequestTransferSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RequestTransferRequest(playToken, "player_id", targetPlayerId, messageForSender, messageForReceiver, messageForCoinMode, amount,
                walletId, onSuccess, onFailure);
        }

        public class RequestTransferRequest : CoinModeRequest<RequestTransferResponse>
        {
            private RequestTransferSuccess onRequestSuccess;

            internal RequestTransferRequest(string playToken, string targetType, string target, string messageForSender, string messageForReceiver,
                string messageForCoinMode, double? amount, string walletId, RequestTransferSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/wallet/transfer_funds/request";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("to_type", targetType);
                requestJson.AddIfNotNull("to", target);
                requestJson.AddIfNotNull("description_for_sender", messageForSender);
                requestJson.AddIfNotNull("description_for_receiver", messageForReceiver);
                requestJson.AddIfNotNull("description_for_coinmode", messageForCoinMode);
                requestJson.AddIfNotNull("amount", amount);
                requestJson.AddIfNotNull("wallet", walletId);
            }

            protected override RequestTransferResponse ConstructSuccessResponse()
            {
                return new RequestTransferResponse();
            }

            protected override void RequestSuccess(RequestTransferResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class RequestTransferResponse : CoinModeResponse
        {
            public string pendingPaymentId { get; private set; } = null;
            public double? fee { get; private set; } = null;
            public VerificationMethodProperties[] requiredVerification { get; private set; } = null;

            internal RequestTransferResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                pendingPaymentId = json["pending_payment_id"];
                fee = json["fee"];
                JsonArray verificationArray = json["array_required_verification"];
                if (verificationArray != null)
                {
                    requiredVerification = new VerificationMethodProperties[verificationArray.Count];
                    for (int i = 0; i < verificationArray.Count; i++)
                    {
                        JsonObject verificationObject = verificationArray[i];
                        if (verificationObject != null)
                        {
                            VerificationMethodProperties verification = new VerificationMethodProperties();
                            verification.FromJson(verificationArray[i]);
                            requiredVerification[i] = verification;
                        }
                    }
                }
            }
        }

        public delegate void GetTransferFeeSuccess(GetTransferFeesResponse response);

        public static GetTransferFeesRequest GetFeesToPlayerId(string playToken, double? amount, string walletId, GetTransferFeeSuccess onSuccess, 
            CoinModeRequestFailure onFailure)
        {
            return new GetTransferFeesRequest(playToken, "player_id", amount, walletId, onSuccess, onFailure);
        }

        public static GetTransferFeesRequest GetFees(string playToken, string targetType, double? amount, string walletId, GetTransferFeeSuccess onSuccess,
            CoinModeRequestFailure onFailure)
        {
            return new GetTransferFeesRequest(playToken, targetType, amount, walletId, onSuccess, onFailure);
        }

        public class GetTransferFeesRequest : CoinModeRequest<GetTransferFeesResponse>
        {
            private GetTransferFeeSuccess onRequestSuccess;

            internal GetTransferFeesRequest(string playToken, string targetType, double? amount, string walletId, 
                GetTransferFeeSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/wallet/transfer_funds/get_fees";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("to_type", targetType);
                requestJson.AddIfNotNull("amount", amount);
                requestJson.AddIfNotNull("wallet", walletId);
            }

            protected override GetTransferFeesResponse ConstructSuccessResponse()
            {
                return new GetTransferFeesResponse();
            }

            protected override void RequestSuccess(GetTransferFeesResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetTransferFeesResponse : CoinModeResponse
        {
            public double? fee { get; private set; } = null;

            internal GetTransferFeesResponse() { }            

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                fee = json["fee"];
            }
        }
    }    
}
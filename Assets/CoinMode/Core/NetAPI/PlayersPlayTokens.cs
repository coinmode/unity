using LightJson;
using System.Collections.Generic;

namespace CoinMode.NetApi
{
    public static partial class PlayersPlayTokens
    {
        public delegate void RequestPlayTokenSuccess(RequestPlayTokenResponse response);

        public static RequestPlayTokenRequest Request (string uuidOrEmail, string titleId, string gameId, RequestPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RequestPlayTokenRequest(PlayerAuthMode.UuidOrEmail, uuidOrEmail, titleId, gameId, null, onSuccess, onFailure);
        }

        public static RequestPlayTokenRequest RequestWithDiscord(string discordId, string discordSignInId, string titleId, string gameId, RequestPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RequestPlayTokenRequest(PlayerAuthMode.Discord, discordId, titleId, gameId, discordSignInId, onSuccess, onFailure);
        }

        public static RequestPlayTokenRequest RequestWithGoogle(string googleId, string googleSignInId, string titleId, string gameId, RequestPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RequestPlayTokenRequest(PlayerAuthMode.Google, googleId, titleId, gameId, googleSignInId, onSuccess, onFailure);
        }

        public static RequestPlayTokenRequest RequestWithApple(string appleId, string appleSignInId, string titleId, string gameId, RequestPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RequestPlayTokenRequest(PlayerAuthMode.Apple, appleId, titleId, gameId, appleSignInId, onSuccess, onFailure);
        }

        public class RequestPlayTokenRequest : CoinModeRequest<RequestPlayTokenResponse>
        {
            private RequestPlayTokenSuccess onRequestSuccess;

            internal RequestPlayTokenRequest(PlayerAuthMode authMode, string loginId, string titleId, string gameId, string externalSignInId, RequestPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/playtokens/request";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();               
                requestJson.AddIfNotNull("game_id", gameId);
                requestJson.AddIfNotNull("title_id", titleId);
                switch (authMode)
                {
                    case PlayerAuthMode.UuidOrEmail:
                        requestJson.AddIfNotNull("player_uuid_or_email", loginId);
                        break;
                    case PlayerAuthMode.Google:
                        requestJson.AddIfNotNull("google_id", loginId);
                        requestJson.AddIfNotNull("google_sign_in_id", externalSignInId);
                        break;
                    case PlayerAuthMode.Discord:
                        requestJson.AddIfNotNull("discord_id", loginId);
                        requestJson.AddIfNotNull("discord_sign_in_id", externalSignInId);
                        break;
                    case PlayerAuthMode.Apple:
                        requestJson.AddIfNotNull("apple_id", loginId);
                        requestJson.AddIfNotNull("apple_sign_in_id", externalSignInId);
                        break;
                }
            }

            protected override RequestPlayTokenResponse ConstructSuccessResponse()
            {
                return new RequestPlayTokenResponse();
            }

            protected override void RequestSuccess(RequestPlayTokenResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class RequestPlayTokenResponse : CoinModeResponse
        {
            public string playToken { get; private set; } = null;
            public bool? isActivated { get; private set; } = null;
            public string publicPlayerId { get; private set; } = null;
            public string titleName { get; private set; } = null;
            public string titleId { get; private set; } = null;
            public VerificationMethodProperties[] requiredVerification { get; private set; } = null;                        
            public int? maxSessions { get; private set; } = null;
            public TitlePermission[] permissions { get; private set; } = null;
            public bool? requireseLicenseSigning { get; private set; } = null;
            public Dictionary<string, bool> signedLicenses { get; private set; } = null;
            public Dictionary<string, LicenseProperties> requiredLicenses { get; private set; } = null;

            internal RequestPlayTokenResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);

                playToken = json["play_token"];
                isActivated = json["is_activated"];
                publicPlayerId = json["player_id"];
                titleName = json["title_name"];
                titleId = json["title_id"];

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
                
                maxSessions = json["max_sessions"];

                JsonArray titlePermissions = json["permissions"];
                if (titlePermissions != null)
                {
                    permissions = new TitlePermission[titlePermissions.Count];
                    for (int i = 0; i < titlePermissions.Count; i++)
                    {
                        JsonObject permissionObject = titlePermissions[i];
                        if (permissionObject != null)
                        {
                            TitlePermission newPermissions = new TitlePermission();
                            newPermissions.FromJson(permissionObject);
                            permissions[i] = newPermissions;
                        }
                    }
                }

                JsonObject licenses = json["licenses"];
                if(licenses != null)
                {
                    requireseLicenseSigning = licenses["needs_license_signing"];

                    JsonObject signed = licenses["signed"];
                    if(signed != null)
                    {
                        signedLicenses = new Dictionary<string, bool>();
                        foreach(KeyValuePair<string, JsonValue> pair in signed)
                        {
                            signedLicenses.Add(pair.Key, pair.Value);
                        }
                    }

                    JsonObject required = licenses["required"];
                    if(required != null)
                    {
                        requiredLicenses = new Dictionary<string, LicenseProperties>();
                        foreach (KeyValuePair<string, JsonValue> pair in required)
                        {
                            LicenseProperties properties = new LicenseProperties();
                            JsonObject license = pair.Value.AsJsonObject;
                            if(license != null)
                            {
                                properties.FromJson(license);
                                requiredLicenses.Add(pair.Key, properties);
                            }                            
                        }
                    }
                }
            }
        }

        public delegate void VerifyPlayTokenSuccess(VerifyPlayTokenResponse response);

        public static VerifyPlayTokenRequest Verify (string playToken, Dictionary<string, string> verificationKeys, List<string> licensesToSign, VerifyPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new VerifyPlayTokenRequest(playToken, verificationKeys, PlayerAuthMode.UuidOrEmail, null, licensesToSign, onSuccess, onFailure);
        }

        public static VerifyPlayTokenRequest VerifyWithDiscord(string playToken, string discordSignInId, List<string> licensesToSign, VerifyPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new VerifyPlayTokenRequest(playToken, null, PlayerAuthMode.Discord, discordSignInId, licensesToSign, onSuccess, onFailure);
        }

        public static VerifyPlayTokenRequest VerifyWithGoogle(string playToken, string googleSignInId, List<string> licensesToSign, VerifyPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new VerifyPlayTokenRequest(playToken, null, PlayerAuthMode.Google, googleSignInId, licensesToSign, onSuccess, onFailure);
        }

        public static VerifyPlayTokenRequest VerifyWithApple(string playToken, string appleSignInId, List<string> licensesToSign, VerifyPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new VerifyPlayTokenRequest(playToken, null, PlayerAuthMode.Apple, appleSignInId, licensesToSign, onSuccess, onFailure);
        }

        public class VerifyPlayTokenRequest : CoinModeRequest<VerifyPlayTokenResponse>
        {
            private VerifyPlayTokenSuccess onRequestSuccess;

            internal VerifyPlayTokenRequest(string playToken, Dictionary<string, string> verificationKeys, PlayerAuthMode authMode, string externalSignInId, List<string> licensesToSign, VerifyPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/playtokens/verify";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                if(verificationKeys != null)
                {
                    foreach (KeyValuePair<string, string> pair in verificationKeys)
                    {
                        requestJson.AddIfNotNull(pair.Key, pair.Value);
                    }
                }

                switch (authMode)
                {
                    case PlayerAuthMode.Discord:
                        requestJson.AddIfNotNull("discord_sign_in_id", externalSignInId);
                        break;
                    case PlayerAuthMode.Google:
                        requestJson.AddIfNotNull("google_sign_in_id", externalSignInId);
                        break;
                    case PlayerAuthMode.Apple:
                        requestJson.AddIfNotNull("apple_sign_in_id", externalSignInId);
                        break;
                }                                

                if(licensesToSign != null)
                {
                    JsonArray licensesArray = new JsonArray();
                    for (int i = 0; i < licensesToSign.Count; i++)
                    {
                        licensesArray.Add(licensesToSign[i]);
                    }
                    requestJson.Add("license_ids", licensesArray);
                }
            }

            protected override VerifyPlayTokenResponse ConstructSuccessResponse()
            {
                return new VerifyPlayTokenResponse();
            }

            protected override void RequestSuccess(VerifyPlayTokenResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class VerifyPlayTokenResponse : CoinModeResponse
        {
            public string playToken { get; private set; } = null;
            public bool? isActivated { get; private set; } = null;
            public bool? justActivated { get; private set; } = null;
            public Dictionary<string, string> errorMessages { get; private set; } = null;
            public Dictionary<string, string> verificationMethodStatus { get; private set; } = null;
            public Dictionary<string, string> infoSubmittedThisCall { get; private set; } = null;

            internal VerifyPlayTokenResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                playToken = json["playtoken"];
                isActivated = json["is_activated"];
                justActivated = json["just_activated"];

                JsonObject errorObject = json["error_messages"];
                if(errorObject != null)
                {
                    errorMessages = new Dictionary<string, string>();
                    foreach(KeyValuePair<string, JsonValue> pair in errorObject)
                    {
                        errorMessages.Add(pair.Key, pair.Value);
                    }
                }

                JsonObject verificationObject = json["playtoken_security_status"];
                if (verificationObject != null)
                {
                    verificationMethodStatus = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, JsonValue> pair in verificationObject)
                    {
                        verificationMethodStatus.Add(pair.Key, pair.Value);
                    }
                }

                JsonObject submittedInfoObject = json["submitted_info_on_this_call"];
                if (submittedInfoObject != null)
                {
                    infoSubmittedThisCall = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, JsonValue> pair in submittedInfoObject)
                    {
                        infoSubmittedThisCall.Add(pair.Key, pair.Value);
                    }
                }
            }
        }

        public delegate void CancelPlayTokenSuccess(CancelPlayTokenResponse response);

        public static CancelPlayTokenRequest Cancel(string playToken, CancelPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new CancelPlayTokenRequest(playToken, onSuccess, onFailure);
        }

        public class CancelPlayTokenRequest : CoinModeRequest<CancelPlayTokenResponse>
        {
            private CancelPlayTokenSuccess onRequestSuccess;

            internal CancelPlayTokenRequest(string playToken, CancelPlayTokenSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/playtokens/cancel";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
            }

            protected override CancelPlayTokenResponse ConstructSuccessResponse()
            {
                return new CancelPlayTokenResponse();
            }

            protected override void RequestSuccess(CancelPlayTokenResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class CancelPlayTokenResponse : CoinModeResponse
        {
            public string tokenStatus { get; private set; } = null;

            internal CancelPlayTokenResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                tokenStatus = json["token"];
            }
        }

        public delegate void GetInfoSuccess(GetInfoResponse response);

        public static GetInfoRequest GetInfo(string playToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetInfoRequest(playToken, onSuccess, onFailure);
        }

        public class GetInfoRequest : CoinModeRequest<GetInfoResponse>
        {
            private GetInfoSuccess onRequestSuccess;

            internal GetInfoRequest(string playToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "players/playtokens/get_token_info";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
            }

            protected override GetInfoResponse ConstructSuccessResponse()
            {
                return new GetInfoResponse();
            }

            protected override void RequestSuccess(GetInfoResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetInfoResponse : CoinModeResponse
        {
            public string playToken { get; private set; } = null;
            public bool? isActive { get; private set; } = null;
            public string publicPlayerId { get; private set; } = null;
            public string titleId { get; private set; } = null;
            public TitlePermission[] permissions { get; private set; } = null;
            public Dictionary<string, string> verificationMethodStatus { get; private set; } = null;

            public PlayTokenStatus activeStatus = null;

            public int? maxSessions { get; private set; } = null;

            internal GetInfoResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);

                playToken = json["play_token"];
                isActive = json["is_active"];
                publicPlayerId = json["player_id"];
                titleId = json["title_id"];

                JsonArray titlePermissions = json["permissions"];
                if (titlePermissions != null)
                {
                    permissions = new TitlePermission[titlePermissions.Count];
                    for (int i = 0; i < titlePermissions.Count; i++)
                    {
                        JsonObject permissionObject = titlePermissions[i];
                        if (permissionObject != null)
                        {
                            TitlePermission newPermissions = new TitlePermission();
                            newPermissions.FromJson(permissionObject);
                            permissions[i] = newPermissions;
                        }
                    }
                }

                JsonObject verificationObject = json["security_status"];
                if (verificationObject != null)
                {
                    verificationMethodStatus = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, JsonValue> pair in verificationObject)
                    {
                        verificationMethodStatus.Add(pair.Key, pair.Value);
                    }
                }

                JsonObject activeObject = json["active_reasons"];
                if (activeObject != null)
                {
                    activeStatus = new PlayTokenStatus();
                    activeStatus.FromJson(activeObject);
                }

                maxSessions = json["max_sessions"];
            }
        }
    }
}
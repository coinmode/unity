using LightJson;

namespace CoinMode.NetApi
{
    public static partial class Players
    {
        public delegate void CreateNewPlayerSuccess(CreateNewPlayerResponse response);

        public static CreatePlayerRequest CreateNewPlayer(string displayName, string proposedEmail, string proposedMobile, string dateOfBirth, string titleId, 
            string referralPlayerId, string referralCampaign, string languageShortcode, string passwordHash, string passwordVersion, 
            CreateNewPlayerSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new CreatePlayerRequest(displayName, proposedEmail, proposedMobile, dateOfBirth, titleId, referralPlayerId, referralCampaign, languageShortcode,
                passwordHash, passwordVersion, onSuccess, onFailure);
        }

        public static CreatePlayerRequest CreateNewPlayer(string displayName, string proposedEmail, string proposedMobile, string dateOfBirth, string titleId, 
            CreateNewPlayerSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new CreatePlayerRequest(displayName, proposedEmail, proposedMobile, dateOfBirth, titleId, null, null, null,
                null, null, onSuccess, onFailure);
        }

        public static CreatePlayerRequest CreateNewPlayer(string displayName, string proposedEmail, string proposedMobile, string titleId,
            CreateNewPlayerSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new CreatePlayerRequest(displayName, proposedEmail, proposedMobile, null, titleId, null, null, null,
                null, null, onSuccess, onFailure);
        }

        public class CreatePlayerRequest : CoinModeRequest<CreateNewPlayerResponse>
        {
            private CreateNewPlayerSuccess onRequestSuccess;

            internal CreatePlayerRequest(string displayName, string proposedEmail, string proposedMobile, string dateOfBirth, string titleId, string referralPlayerId, string referralCampaign,
            string languageShortcode, string passwordHash, string passwordVersion, CreateNewPlayerSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "players/create";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("display_name", displayName);
                requestJson.AddIfNotNull("proposed_email", proposedEmail);
                requestJson.AddIfNotNull("proposed_mobile", proposedMobile);
                requestJson.AddIfNotNull("date_of_birth", dateOfBirth);
                requestJson.AddIfNotNull("title_id", titleId);
                requestJson.AddIfNotNull("referral_player_id", referralPlayerId);
                requestJson.AddIfNotNull("referral_campaign", referralCampaign);
                requestJson.AddIfNotNull("language_shortcode", languageShortcode);
                requestJson.AddIfNotNull("password_hash", passwordHash);
                requestJson.AddIfNotNull("password_version", passwordVersion);
            }

            protected override CreateNewPlayerResponse ConstructSuccessResponse()
            {
                return new CreateNewPlayerResponse();
            }

            protected override void RequestSuccess(CreateNewPlayerResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class CreateNewPlayerResponse : CoinModeResponse
        {
            public string playerSecretUuid { get; private set; } = null;
            public string publicPlayerId { get; private set; } = null;
            public int? epochCreated { get; private set; } = null;
            public string[] valuesSet { get; private set; } = null;

            internal CreateNewPlayerResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                playerSecretUuid = json["player_secret_uuid"];
                publicPlayerId = json["player_id"];
                epochCreated = json["epoch_created"];
                JsonArray valuesArray = json["values_set"];
                if(valuesArray != null)
                {
                    valuesSet = new string[valuesArray.Count];
                    for(int i = 0; i < valuesArray.Count; i++)
                    {
                        valuesSet[i] = valuesArray[i];
                    }
                }
            }
        }

        public delegate void GetPropertiesSuccess(GetPropertiesResponse response);

        public static GetPropertiesRequest GetProperties(string playerPublicId, GetPropertiesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetPropertiesRequest(null, playerPublicId, onSuccess, onFailure);
        }

        public static GetPropertiesRequest GetProperties(string playToken, string playerPublicId, GetPropertiesSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetPropertiesRequest(playToken, playerPublicId, onSuccess, onFailure);
        }

        public class GetPropertiesRequest : CoinModeRequest<GetPropertiesResponse>
        {
            private GetPropertiesSuccess onRequestSuccess;

            internal GetPropertiesRequest(string playToken, string playerPublicId, GetPropertiesSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "players/get_properties";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("about_player_id", playerPublicId);
            }

            protected override GetPropertiesResponse ConstructSuccessResponse()
            {
                return new GetPropertiesResponse();
            }

            protected override void RequestSuccess(GetPropertiesResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetPropertiesResponse : CoinModeResponse
        {
            public string publicPlayerId { get; private set; } = null;
            public string displayName { get; private set; } = null;
            public string avatarImageUrlSmall { get; private set; } = null;
            public string avatarImageUrlLarge { get; private set; } = null;
            public string languageShortcode { get; private set; } = null;
            public string country { get; private set; } = null;
            public string countryCode { get; private set; } = null;
            public string returnedData { get; private set; } = null;
            public bool? mobileVerified { get; private set; } = null;
            public bool? emailVerified { get; private set; } = null;
            public bool? awaitingMobileConfirmation { get; private set; } = null;
            public bool? awaitingEmailConfirmation { get; private set; } = null;
            public int? verificationLevel { get; private set; } = null;
            public string displayCurrency { get; private set; } = null;
            public string defaultWallet { get; private set; } = null;


            internal GetPropertiesResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                publicPlayerId = json["player_id"];
                displayName = json["display_name"];
                avatarImageUrlSmall = json["avatar_image_url_small"];
                avatarImageUrlLarge = json["avatar_image_url_large"];
                languageShortcode = json["language_shortcode"];
                country = json["country"];
                countryCode = json["country_code"];
                returnedData = json["returned_data"];
                mobileVerified = json["mobile_verified"];
                emailVerified = json["email_verified"];
                awaitingMobileConfirmation = json["mobile_expecting_confirmation"];
                awaitingEmailConfirmation = json["email_expecting_confirmation"];
                verificationLevel = json["verification_level"];
                displayCurrency = json["display_currency"];
                defaultWallet = json["default_wallet"];
            }
        }
    }
}
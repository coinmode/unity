using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.NetApi
{
    public static partial class Oauth
    {
        public delegate void DiscordUserInformationSuccess(DiscordUserInformationResponse response);

        public static DiscordUserInformationRequest RequestDiscordUserInformation(string externalSignInId, DiscordUserInformationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new DiscordUserInformationRequest(externalSignInId, onSuccess, onFailure);
        }

        public class DiscordUserInformationRequest : UserInformationRequest<DiscordUserInformationResponse>
        {
            private DiscordUserInformationSuccess onRequestSuccess;

            internal DiscordUserInformationRequest(string externalSignInId, DiscordUserInformationSuccess onSuccess, CoinModeRequestFailure onFailure)
                : base(externalSignInId, "discord", onFailure)
            {
                onRequestSuccess = onSuccess;
            }

            protected override DiscordUserInformationResponse ConstructSuccessResponse()
            {
                return new DiscordUserInformationResponse();
            }

            protected override void RequestSuccess(DiscordUserInformationResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class DiscordUserInformationResponse : UserInformationResponse
        {
            internal DiscordUserInformationResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }

        public delegate void DiscordSignInUrlSuccess(DiscordSignInUrlResponse response);

        public static DiscordSignInUrlRequest RequestDiscordSignInUrl(DiscordSignInUrlSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new DiscordSignInUrlRequest(onSuccess, onFailure);
        }

        public class DiscordSignInUrlRequest : SignInUrlRequest<DiscordSignInUrlResponse>
        {
            private DiscordSignInUrlSuccess onRequestSuccess;

            internal DiscordSignInUrlRequest(DiscordSignInUrlSuccess onSuccess, CoinModeRequestFailure onFailure)
                : base ("discord", onFailure)
            {
                onRequestSuccess = onSuccess;
            }

            protected override DiscordSignInUrlResponse ConstructSuccessResponse()
            {
                return new DiscordSignInUrlResponse();
            }

            protected override void RequestSuccess(DiscordSignInUrlResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class DiscordSignInUrlResponse : SignInUrlResponse
        {
            internal DiscordSignInUrlResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }
    }    
}
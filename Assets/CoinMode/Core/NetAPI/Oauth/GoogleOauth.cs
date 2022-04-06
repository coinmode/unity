using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.NetApi
{
    public static partial class Oauth
    {
        public delegate void GoogleUserInformationSuccess(GoogleUserInformationResponse response);

        public static GoogleUserInformationRequest RequestGoogleUserInformation(string externalSignInId, GoogleUserInformationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GoogleUserInformationRequest(externalSignInId, onSuccess, onFailure);
        }

        public class GoogleUserInformationRequest : UserInformationRequest<GoogleUserInformationResponse>
        {
            private GoogleUserInformationSuccess onRequestSuccess;

            internal GoogleUserInformationRequest(string externalSignInId, GoogleUserInformationSuccess onSuccess, CoinModeRequestFailure onFailure)
                : base (externalSignInId, "google", onFailure)
            {
                onRequestSuccess = onSuccess;
            }

            protected override GoogleUserInformationResponse ConstructSuccessResponse()
            {
                return new GoogleUserInformationResponse();
            }

            protected override void RequestSuccess(GoogleUserInformationResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GoogleUserInformationResponse : UserInformationResponse
        {
            internal GoogleUserInformationResponse() : base () { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }

        public delegate void GoogleSignInUrlSuccess(GoogleSignInUrlResponse response);

        public static GoogleSignInUrlRequest RequestGoogleSignInUrl(GoogleSignInUrlSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GoogleSignInUrlRequest(onSuccess, onFailure);
        }

        public class GoogleSignInUrlRequest : SignInUrlRequest<GoogleSignInUrlResponse>
        {
            private GoogleSignInUrlSuccess onRequestSuccess;

            internal GoogleSignInUrlRequest(GoogleSignInUrlSuccess onSuccess, CoinModeRequestFailure onFailure)
                : base ("google", onFailure)
            {
                onRequestSuccess = onSuccess;
            }

            protected override GoogleSignInUrlResponse ConstructSuccessResponse()
            {
                return new GoogleSignInUrlResponse();
            }

            protected override void RequestSuccess(GoogleSignInUrlResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GoogleSignInUrlResponse : SignInUrlResponse
        {
            internal GoogleSignInUrlResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }
    }    
}
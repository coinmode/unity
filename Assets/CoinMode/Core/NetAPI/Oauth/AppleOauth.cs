using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.NetApi
{
    public static partial class Oauth
    {
        public delegate void AppleUserInformationSuccess(AppleUserInformationResponse response);

        public static AppleUserInformationRequest RequestAppleUserInformation(string externalSignInId, AppleUserInformationSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new AppleUserInformationRequest(externalSignInId, onSuccess, onFailure);
        }

        public class AppleUserInformationRequest : UserInformationRequest<AppleUserInformationResponse>
        {
            private AppleUserInformationSuccess onRequestSuccess;

            internal AppleUserInformationRequest(string externalSignInId, AppleUserInformationSuccess onSuccess, CoinModeRequestFailure onFailure)
                : base(externalSignInId, "apple", onFailure)
            {
                onRequestSuccess = onSuccess;
            }

            protected override AppleUserInformationResponse ConstructSuccessResponse()
            {
                return new AppleUserInformationResponse();
            }

            protected override void RequestSuccess(AppleUserInformationResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class AppleUserInformationResponse : UserInformationResponse
        {
            internal AppleUserInformationResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }

        public delegate void AppleSignInUrlSuccess(AppleSignInUrlResponse response);

        public static AppleSignInUrlRequest RequestAppleSignInUrl(AppleSignInUrlSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new AppleSignInUrlRequest(onSuccess, onFailure);
        }

        public class AppleSignInUrlRequest : SignInUrlRequest<AppleSignInUrlResponse>
        {
            private AppleSignInUrlSuccess onRequestSuccess;

            internal AppleSignInUrlRequest(AppleSignInUrlSuccess onSuccess, CoinModeRequestFailure onFailure)
                : base ("apple", onFailure)
            {
                onRequestSuccess = onSuccess;
            }

            protected override AppleSignInUrlResponse ConstructSuccessResponse()
            {
                return new AppleSignInUrlResponse();
            }

            protected override void RequestSuccess(AppleSignInUrlResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class AppleSignInUrlResponse : SignInUrlResponse
        {
            internal AppleSignInUrlResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }
    }    
}
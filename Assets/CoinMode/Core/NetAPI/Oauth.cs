using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.NetApi
{
    public static partial class Oauth
    {
        public abstract class UserInformationRequest<T> : CoinModeOauthRequest<T> where T : UserInformationResponse 
        {
            public string externalSignInId { get; private set; } = null;

            internal UserInformationRequest(string externalSignInId, string service, CoinModeRequestFailure onFailure)
            {
                requestMethod = RequestMethod.GET;
                apiDir = "auth/" + service + "/user";                
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("externalSignInId", externalSignInId);
            }
        }

        public abstract class UserInformationResponse : CoinModeResponse
        {
            public string id { get; private set; } = null;
            public string email { get; private set; } = null;
            public bool? signedIn { get; private set; } = null;

            internal UserInformationResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                id = json["id"];
                email = json["email"];
                signedIn = json["signedIn"];
            }
        }

        public abstract class SignInUrlRequest<T> : CoinModeOauthRequest<T> where T : SignInUrlResponse
        {
            internal SignInUrlRequest(string service, CoinModeRequestFailure onFailure)
            {
                requestMethod = RequestMethod.GET;
                apiDir = "auth/" + service + "/url";
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
            }
        }

        public abstract class SignInUrlResponse : CoinModeResponse
        {
            public string url { get; private set; } = null;
            public string externalSignInId { get; private set; } = null;

            internal SignInUrlResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                url = json["url"];
                externalSignInId = json["externalSignInId"];
            }
        }
    }    
}
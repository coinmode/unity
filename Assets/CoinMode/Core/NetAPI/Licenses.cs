using LightJson;
using System.Collections.Generic;

namespace CoinMode.NetApi
{
    public static partial class Licenses
    {
        public delegate void GetLicenseSuccess(GetLicenseResponse response);

        public static GetLicenseRequest GetPlayerTerms(GetLicenseSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetLicenseRequest("PlayerTermsToUseCoinMode", onSuccess, onFailure);
        }

        public static GetLicenseRequest Get(string licenseType, GetLicenseSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetLicenseRequest(licenseType, onSuccess, onFailure);
        }

        public class GetLicenseRequest : CoinModeRequest<GetLicenseResponse>
        {
            private GetLicenseSuccess onRequestSuccess;

            internal GetLicenseRequest(string licenseType, GetLicenseSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "licenses/get";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("license_type", licenseType);
            }

            protected override GetLicenseResponse ConstructSuccessResponse()
            {
                return new GetLicenseResponse();
            }

            protected override void RequestSuccess(GetLicenseResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetLicenseResponse : CoinModeResponse
        {
            public LicenseProperties properties = new LicenseProperties();

            internal GetLicenseResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                properties.FromJson(json);
            }
        }
    }
}
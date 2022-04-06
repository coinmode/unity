using LightJson;

namespace CoinMode.NetApi
{
    public static partial class Titles
    {
        public delegate void GetInfoSuccess(GetInfoResponse response);

        public static GetInfoRequest GetInfo(string titleId, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetInfoRequest(titleId, null, null, onSuccess, onFailure);
        }

        public static GetInfoRequest GetInfo(string gameId, string playerId, string loginToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetInfoRequest(gameId, playerId, loginToken, onSuccess, onFailure);
        }        

        public class GetInfoRequest : CoinModeRequest<GetInfoResponse>
        {
            private GetInfoSuccess onRequestSuccess;            

            internal GetInfoRequest(string titleId, string playerId, string loginToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/get_info";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("title_id", titleId);
                requestJson.AddIfNotNull("player_id", playerId);
                requestJson.AddIfNotNull("login_id", loginToken);
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
            public TitleInfo properties { get; } = new TitleInfo();

            internal GetInfoResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                properties.FromJson(json);
            }
        }
    }    
}
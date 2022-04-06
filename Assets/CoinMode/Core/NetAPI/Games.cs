using LightJson;

namespace CoinMode.NetApi
{
    public static partial class Games
    {
        public delegate void GetInfoSuccess(GetInfoResponse response);

        public static GetInfoRequest GetInfo(string gameId, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetInfoRequest(gameId, null, null, onSuccess, onFailure);
        }

        public static GetInfoRequest GetInfo(string gameId, string playerId, string loginToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetInfoRequest(gameId, playerId, loginToken, onSuccess, onFailure);
        }        

        public class GetInfoRequest : CoinModeRequest<GetInfoResponse>
        {
            private GetInfoSuccess onRequestSuccess;            

            internal GetInfoRequest(string gameId, string playerId, string loginToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/get_info";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("game_id", gameId);
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
            public GameInfo properties { get; } = new GameInfo();

            internal GetInfoResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                properties.FromJson(json);
            }
        }
    }    
}
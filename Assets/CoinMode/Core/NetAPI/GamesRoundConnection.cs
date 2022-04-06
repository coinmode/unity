using LightJson;

namespace CoinMode.NetApi
{
    public static partial class GamesRoundConnection
    {
        // Get Connection Info

        public delegate void GetConnectionInfoSuccess(GetConnectionInfoResponse response);

        public static GetConnectionInfoRequest GetConnectionInfo(string roundId, GetConnectionInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetConnectionInfoRequest(roundId, onSuccess, onFailure);
        }

        public class GetConnectionInfoRequest : CoinModeRequest<GetConnectionInfoResponse>
        {
            private GetConnectionInfoSuccess onRequestSuccess;

            internal GetConnectionInfoRequest(string roundId, GetConnectionInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/round/connection/get_info";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("round_id", roundId);
            }

            protected override GetConnectionInfoResponse ConstructSuccessResponse()
            {
                return new GetConnectionInfoResponse();
            }

            protected override void RequestSuccess(GetConnectionInfoResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetConnectionInfoResponse : CoinModeResponse
        {
            public bool? hasServer { get; private set; } = null;
            public string serverStatus { get; private set; } = null;
            public string serverIp { get; private set; } = null;
            public string serverPort { get; private set; } = null;

            internal GetConnectionInfoResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                JsonObject connectionInfo = json["connection_info"];
                if (connectionInfo != null)
                {
                    hasServer = true;
                    serverStatus = connectionInfo["server_status"];
                    JsonObject serverInfo = connectionInfo["server_connection_info"];
                    if (serverInfo != null)
                    {
                        serverIp = serverInfo["ip"];
                        serverPort = serverInfo["port"];
                    }
                }
                else
                {
                    hasServer = false;
                }
            }
        }

        // Set Connection Info

        public delegate void SetConnectionInfoSuccess(SetConnectionInfoResponse response);

        public static SetConnectionInfoRequest SetConnectionStatus(string roundId, string gameServerKey, string status, SetConnectionInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new SetConnectionInfoRequest(roundId, gameServerKey, status, null, onSuccess, onFailure);
        }

        public static SetConnectionInfoRequest SetConnectionInfo(string roundId, string gameServerKey, string status, string properties, SetConnectionInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new SetConnectionInfoRequest(roundId, gameServerKey, status, properties, onSuccess, onFailure);
        }

        public class SetConnectionInfoRequest : CoinModeRequest<SetConnectionInfoResponse>
        {
            private SetConnectionInfoSuccess onRequestSuccess;

            internal SetConnectionInfoRequest(string roundId, string gameServerKey, string connectionStatus, string connectionProperties, 
                SetConnectionInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/round/connection/set_info";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("round_id", roundId);
                requestJson.AddIfNotNull("game_server_key", gameServerKey);
                requestJson.AddIfNotNull("connection_status", connectionStatus);
                requestJson.AddIfNotNull("connection_properties", connectionProperties);
            }

            protected override SetConnectionInfoResponse ConstructSuccessResponse()
            {
                return new SetConnectionInfoResponse();
            }

            protected override void RequestSuccess(SetConnectionInfoResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class SetConnectionInfoResponse : CoinModeResponse
        {
            internal SetConnectionInfoResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }
    }
}
using LightJson;
using System;

namespace CoinMode.NetApi
{
    public static partial class GamesRoundSession
    {
        // Request Session

        public delegate void RequestSessionSuccess(RequestSessionResponse response);

        public static RequestSessionRequest Request (string playToken, string roundId, string additionIp, string passPhrase, RequestSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new RequestSessionRequest(playToken, roundId, additionIp, passPhrase, onSuccess, onFailure);
        }

        public class RequestSessionRequest : CoinModeRequest<RequestSessionResponse>
        {
            private RequestSessionSuccess onRequestSuccess;

            internal RequestSessionRequest(string playToken, string roundId, string additionIp, string passPhrase, RequestSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "titles/games/round/session/request";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("round_id", roundId);
                requestJson.AddIfNotNull("addition_ip", additionIp);
                requestJson.AddIfNotNull("passphrase", passPhrase);
            }

            protected override RequestSessionResponse ConstructSuccessResponse()
            {
                return new RequestSessionResponse();
            }

            protected override void RequestSuccess(RequestSessionResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class RequestSessionResponse : CoinModeResponse
        {
            public string sessionId { get; private set; } = null;
            public string roundId { get; private set; } = null;
            public string gameId { get; private set; } = null;
            public string roundName { get; private set; } = null;
            public double? playFee { get; private set; } = null;
            public double? potContribution { get; private set; } = null;
            public bool? playerCanPlay { get; private set; } = null;

            internal RequestSessionResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                sessionId = json["session_id"];
                roundId = json["round_id"];
                gameId = json["game_id"];
                roundName = json["round_name"];
                playFee = json["fee_play_session"];
                potContribution = json["pot_contribution"];
                playerCanPlay = json["player_can_play"];
            }
        }

        // Start Session

        public delegate void StartSessionSuccess(StartSessionResponse response);

        public static StartSessionRequest Start (string playToken, string sessionId, StartSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new StartSessionRequest(playToken, sessionId, onSuccess, onFailure);
        }

        public class StartSessionRequest : CoinModeRequest<StartSessionResponse>
        {
            private StartSessionSuccess onRequestSuccess;

            internal StartSessionRequest(string playToken, string sessionId, StartSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "titles/games/round/session/start";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("session_id", sessionId);
            }

            protected override StartSessionResponse ConstructSuccessResponse()
            {
                return new StartSessionResponse();
            }

            protected override void RequestSuccess(StartSessionResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class StartSessionResponse : CoinModeResponse
        {
            public string playerId { get; private set; } = null;
            public string sessionId { get; private set; } = null;            
            public int? sessionStarted { get; private set; } = null;
            public int? statusId { get; private set; } = null;
            public string statusText { get; private set; } = null;

            internal StartSessionResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                playerId = json["player_id"];
                sessionId = json["session_id"];                
                sessionStarted = json["session_started"];
                statusId = json["status_id"];
                statusText = json["status_text"];
            }
        }

        // Stop Session

        public delegate void StopSessionSuccess(StopSessionResponse response);

        public static StopSessionRequest Stop(string sessionId, double? score, string encodedData, string formattedScore, JsonObject sessionData, string lockKey, StopSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new StopSessionRequest(sessionId, score, encodedData, formattedScore, sessionData, lockKey, onSuccess, onFailure);
        }

        public static StopSessionRequest Stop (string sessionId, double? score, string formattedScore, JsonObject sessionData, string lockKey, StopSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new StopSessionRequest(sessionId, score, null, formattedScore, sessionData, lockKey, onSuccess, onFailure);
        }

        public class StopSessionRequest : CoinModeRequest<StopSessionResponse>
        {
            private StopSessionSuccess onRequestSuccess;

            internal StopSessionRequest(string sessionId, double? score, string encodedData, string formattedScore, JsonObject sessionData, string lockKey, StopSessionSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "titles/games/round/session/stop";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("session_id", sessionId);
                requestJson.AddIfNotNull("score", score);
                requestJson.AddIfNotNull("encoded_data", encodedData);
                requestJson.AddIfNotNull("formatted_score", formattedScore);
                requestJson.AddIfNotNull("session_data", sessionData);
                requestJson.AddIfNotNull("lock_key", lockKey);
            }

            protected override StopSessionResponse ConstructSuccessResponse()
            {
                return new StopSessionResponse();
            }

            protected override void RequestSuccess(StopSessionResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class StopSessionResponse : CoinModeResponse
        {
            public int? statusId { get; private set; } = null;
            public string statusText { get; private set; } = null;
            public int? sessionEnded { get; private set; } = null;
            public double? score { get; private set; } = null;
            public string formattedScore { get; private set; } = null;
            public JsonObject clientSessionData { get; private set; } = null;
            public JsonObject serverSessionData { get; private set; } = null;
            public bool? requireLockToStart { get; private set; } = null;

            internal StopSessionResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                statusId = json["status_id"];
                statusText = json["status_text"];
                sessionEnded = json["session_ended"];
                score = json["score"];
                formattedScore = json["formatted_score"];
                clientSessionData = json["client_json"];
                serverSessionData = json["server_json"];
                requireLockToStart = json["require_lock_to_start_round"];
            }
        }

        // List Sessions

        public delegate void ListSessionsSuccess(ListSessionsResponse response);

        public static ListSessionsRequest List(string playToken, string roundId, string playerId, ListSessionsSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new ListSessionsRequest(playToken, null, null, roundId, playerId, null, null, onSuccess, onFailure);
        }

        public static ListSessionsRequest List(string playToken, string gameId, string titleId, string roundId, string playerId, int? start, int? count, ListSessionsSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new ListSessionsRequest(playToken, gameId, titleId, roundId, playerId, start, count, onSuccess, onFailure);
        }

        public class ListSessionsRequest : CoinModeRequest<ListSessionsResponse>
        {
            private ListSessionsSuccess onRequestSuccess;

            internal ListSessionsRequest(string playToken, string gameId, string titleId, string roundId, string playerId, int? start, int? count, ListSessionsSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/round/session/list";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("game_id", gameId);
                requestJson.AddIfNotNull("title_id", titleId);
                requestJson.AddIfNotNull("round_id", roundId);
                requestJson.AddIfNotNull("player_id", playerId);
                requestJson.AddIfNotNull("start", start);
                requestJson.AddIfNotNull("count", count);
            }

            protected override ListSessionsResponse ConstructSuccessResponse()
            {
                return new ListSessionsResponse();
            }

            protected override void RequestSuccess(ListSessionsResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class ListSessionsResponse : CoinModeResponse
        {
            public SessionInfo[] sessions { get; private set; } = null;

            internal ListSessionsResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                JsonArray results = json["results"];
                if(results != null)
                {
                    sessions = new SessionInfo[results.Count];
                    for(int i = 0; i < results.Count; i++)
                    {
                        JsonObject sessionObject = results[i].AsJsonObject;
                        if(sessionObject != null)
                        {
                            SessionInfo session = new SessionInfo();
                            session.FromJson(sessionObject);
                            sessions[i] = session;
                        }
                    }
                }
            }
        }
    }
}
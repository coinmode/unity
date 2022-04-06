using LightJson;
using System.Collections.Generic;

namespace CoinMode.NetApi
{
    public static partial class GamesRound
    {
        // Create Round

        public delegate void CreateRoundSuccess(CreateRoundResponse response);

        public static CreateRoundRequest Create (string playToken, string gameId, string gameServerKey, string lockKey, int? maxSessions, 
            bool? publicallyVisible, double? potContribution, string name, string passphrase, string serverURL, JsonObject customJson, 
            string gameServerParams, CreateRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new CreateRoundRequest(playToken, gameId, gameServerKey, lockKey, maxSessions,
            publicallyVisible, potContribution, name, passphrase, serverURL, customJson,
            gameServerParams, onSuccess, onFailure);
        }

        public class CreateRoundRequest : CoinModeRequest<CreateRoundResponse>
        {
            private CreateRoundSuccess onRequestSuccess;

            internal CreateRoundRequest(string playToken, string gameId, string gameServerKey, string lockKey, int? maxSessions,
            bool? publicallyVisible, double? potContribution, string name, string passphrase, string serverURL, JsonObject customJson,
            string gameServerParams, CreateRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "titles/games/round/create";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("game_id", gameId);
                requestJson.AddIfNotNull("game_server_key", gameServerKey);
                requestJson.AddIfNotNull("lock_key", lockKey);
                requestJson.AddIfNotNull("max_sessions", maxSessions);
                requestJson.AddIfNotNull("publically_visible", publicallyVisible);
                requestJson.AddIfNotNull("pot_contribution", potContribution);
                requestJson.AddIfNotNull("name", name);
                requestJson.AddIfNotNull("passphrase", passphrase);
                requestJson.AddIfNotNull("server_url", serverURL);
                if(customJson != null)
                {
                    requestJson.AddIfNotNull("custom_json", customJson.ToString());
                }
                requestJson.AddIfNotNull("game_server_params", gameServerParams);
            }

            protected override CreateRoundResponse ConstructSuccessResponse()
            {
                return new CreateRoundResponse();
            }

            protected override void RequestSuccess(CreateRoundResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class CreateRoundResponse : CoinModeResponse
        {
            public string roundId { get; private set; } = null;
            public string passphrase { get; private set; } = null;
            public string shareUrl { get; private set; } = null;
            public int? remainingRounds { get; private set; } = null;
            public double? durationSeconds { get; private set; } = null;
            public double? maxPlayTime { get; private set; } = null;
            public int? epochStart { get; private set; } = null;
            public int? epochToFinish { get; private set; } = null;
            public string gameId { get; private set; } = null;
            public string serverUrl { get; private set; } = null;
            public string walletId { get; private set; } = null;
            public int? payoutTypeId { get; private set; } = null;
            public JsonObject customJson { get; private set; } = null;
            public bool? hasServer { get; private set; } = null;
            public string serverStatus { get; private set; } = null;
            public string serverIp { get; private set; } = null;
            public string serverPort { get; private set; } = null;

            internal CreateRoundResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                roundId = json["round_id"];
                passphrase = json["passphrase"];
                shareUrl = json["share_url"];
                remainingRounds = json["remaining_rounds"];
                durationSeconds = json["round_duration_in_seconds"];
                maxPlayTime = json["max_time_per_play"];
                epochStart = json["epoch_round_start"];
                epochToFinish = json["epoch_to_finish"];
                gameId = json["game_id"];
                serverUrl = json["server_url"];
                walletId = json["wallet"];
                payoutTypeId = json["payout_type_id"];
                customJson = json["custom_json"];

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

        // Lock Round

        public delegate void LockRoundSuccess(LockRoundResponse response);

        public static LockRoundRequest Lock (string roundId, string gameServerKey, LockRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new LockRoundRequest(roundId, gameServerKey, onSuccess, onFailure);
        }

        public class LockRoundRequest : CoinModeRequest<LockRoundResponse>
        {
            private LockRoundSuccess onRequestSuccess;

            internal LockRoundRequest(string roundId, string gameServerKey, LockRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "titles/games/round/lock";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("round_id", roundId);
                requestJson.AddIfNotNull("game_server_key", gameServerKey);
            }

            protected override LockRoundResponse ConstructSuccessResponse()
            {
                return new LockRoundResponse();
            }

            protected override void RequestSuccess(LockRoundResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class LockRoundResponse : CoinModeResponse
        {
            public string roundId { get; private set; } = null;
            public string lockKey { get; private set; } = null;
            public int? statusId { get; private set; } = null;
            public int? epochRoundLocked { get; private set; } = null;
            public double? fixedPotAmountPaidIn { get; private set; } = null;
            public int? potContributors { get; private set; } = null;
            public double? potContribution { get; private set; } = null;
            public double? potTotal { get; private set; } = null;
            public double? winningPot { get; private set; } = null;
            internal LockRoundResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                roundId = json["round_id"];
                lockKey = json["lock_key"];
                statusId = json["status_id"];
                epochRoundLocked = json["epoch_round_locked"];
                fixedPotAmountPaidIn = json["fixed_pot_amount_paid_in"];
                potContributors = json["pot_contributors"];
                potContribution = json["pot_contribution"];
                potTotal = json["pot_total"];
                winningPot = json["winning_pot"];
            }
        }

        // End Round

        public delegate void EndRoundSuccess(EndRoundResponse response);

        public static EndRoundRequest End (string roundID, string lockKey, bool? forceCloseSessions, EndRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new EndRoundRequest(roundID, null, lockKey, forceCloseSessions, onSuccess, onFailure);
        }

        public static EndRoundRequest End(string roundID, string playToken, string lockKey, bool? forceCloseSessions, EndRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new EndRoundRequest(roundID, playToken, lockKey, forceCloseSessions, onSuccess, onFailure);
        }

        public class EndRoundRequest : CoinModeRequest<EndRoundResponse>
        {
            private EndRoundSuccess onRequestSuccess;

            internal EndRoundRequest(string roundID, string playToken, string lockKey, bool? forceCloseSessions, EndRoundSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                apiDir = "titles/games/round/end";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("round_id", roundID);
                requestJson.AddIfNotNull("play_token", playToken);
                requestJson.AddIfNotNull("lock_key", lockKey);
                requestJson.AddIfNotNull("force_close_sessions", forceCloseSessions);
            }

            protected override EndRoundResponse ConstructSuccessResponse()
            {
                return new EndRoundResponse();
            }

            protected override void RequestSuccess(EndRoundResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class EndRoundResponse : CoinModeResponse
        {
            internal EndRoundResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
            }
        }

        // List Rounds

        public delegate void ListRoundsSuccess(ListRoundsResponse response);

        public static ListRoundsRequest List (string gameId, string playerId, string playtoken, bool? includeReadyToJoin, bool? includeCurrentInPlay, bool? completedOnly, 
            bool? allRounds, int? offset, int? limit, ListRoundsSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new ListRoundsRequest(gameId, playerId, playtoken, includeReadyToJoin, includeCurrentInPlay, completedOnly, allRounds, offset, limit, onSuccess, onFailure);
        }

        // This needs a different response format, might be unintentional
        public static ListRoundsRequest List(string gameId, string playerId, string playtoken, bool? includeReadyToJoin, bool? includeCurrentInPlay, bool? completedOnly,
            int? offset, int? limit, ListRoundsSuccess onSuccess, CoinModeRequestFailure onFailure)
        {            
            return new ListRoundsRequest(gameId, playerId, playtoken, includeReadyToJoin, includeCurrentInPlay, completedOnly, false, offset, limit, onSuccess, onFailure);
        }

        public class ListRoundsRequest : CoinModeRequest<ListRoundsResponse>
        {
            private ListRoundsSuccess onRequestSuccess;

            internal ListRoundsRequest(string gameId, string playerId, string playtoken, bool? includeReadyToJoin, bool? includeCurrentInPlay, bool? completedOnly, 
                bool? allRounds, int? offset, int? limit, ListRoundsSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/round/list";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("game_id", gameId);
                requestJson.AddIfNotNull("player_id", playerId);
                requestJson.AddIfNotNull("play_token", playtoken);                    
                requestJson.AddIfNotNull("include_ready_to_join", includeReadyToJoin);
                requestJson.AddIfNotNull("include_currently_in_play", includeCurrentInPlay);
                requestJson.AddIfNotNull("completed_only", completedOnly);
                requestJson.AddIfNotNull("all_rounds", allRounds);
                requestJson.AddIfNotNull("offset", offset);
                requestJson.AddIfNotNull("limit", limit);
            }

            protected override ListRoundsResponse ConstructSuccessResponse()
            {
                return new ListRoundsResponse();
            }

            protected override void RequestSuccess(ListRoundsResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class ListRoundsResponse : CoinModeResponse
        {
            public string gameId { get; private set; } = null;
            public int? offset { get; private set; } = null;
            public int? limit { get; private set; } = null;
            public int? fullCount { get; private set; } = null;
            public string allErrors { get; private set; } = null;
            public MinimalRoundInfo[] rounds { get; private set; } = null;
            public int? allowRepeatPlayInGame { get; private set; } = null;
            public int? allowRepeatPlayInRound { get; private set; } = null;
            internal ListRoundsResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                gameId = json["game_id"];
                offset = json["offset"];
                limit = json["limit"];
                fullCount = json["full_count"];
                allErrors = json["all_errors"];
                JsonArray roundsArray = json["rounds"];
                if(roundsArray != null)
                {
                    rounds = new MinimalRoundInfo[roundsArray.Count];
                    for(int i = 0; i < roundsArray.Count; i++)
                    {
                        JsonObject roundObject = roundsArray[i].AsJsonObject;
                        if(roundObject != null)
                        {
                            MinimalRoundInfo round = new MinimalRoundInfo();
                            round.FromJson(roundObject);
                            rounds[i] = round;
                        }
                    }                    
                }
                allowRepeatPlayInGame = json["allow_repeat_play_in_game"];
                allowRepeatPlayInRound = json["allow_repeat_play_in_round"];
            }
        }

        // Get Info

        public delegate void GetInfoSuccess(GetInfoResponse response);

        public static GetInfoRequest GetInfo(string roundId, string playToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetInfoRequest(roundId, playToken, onSuccess, onFailure);
        }

        public class GetInfoRequest : CoinModeRequest<GetInfoResponse>
        {
            private GetInfoSuccess onRequestSuccess;

            internal GetInfoRequest(string roundId, string playToken, GetInfoSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/round/get_info";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("round_id", roundId);
                requestJson.AddIfNotNull("play_token", playToken);
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
            public RoundInfo properties { get; } = new RoundInfo();
            internal GetInfoResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                properties.FromJson(json);
            }
        }

        // Get High Scores

        public delegate void GetHighScoresSuccess(GetHighScoresResponse response);

        public static GetHighScoresRequest GetHighScores(string roundId, string sessionId, int? start, int? count, bool? showCondensed, bool? rowsToReturn, 
            GetHighScoresSuccess onSuccess, CoinModeRequestFailure onFailure)
        {
            return new GetHighScoresRequest(roundId, sessionId, start, count, showCondensed, rowsToReturn, onSuccess, onFailure);
        }

        public class GetHighScoresRequest : CoinModeRequest<GetHighScoresResponse>
        {
            private GetHighScoresSuccess onRequestSuccess;

            internal GetHighScoresRequest(string roundId, string sessionId, int? start, int? count, bool? showCondensed, bool? rowsToReturn, 
                GetHighScoresSuccess onSuccess, CoinModeRequestFailure onFailure)
            {
                autoRetry = true;
                apiDir = "titles/games/round/get_highscores";

                onRequestSuccess = onSuccess;
                onRequestFailure = onFailure;

                requestJson = new JsonObject();
                requestJson.AddIfNotNull("round_id", roundId);
                requestJson.AddIfNotNull("session_id", sessionId);
                requestJson.AddIfNotNull("start", start);
                requestJson.AddIfNotNull("count", count);
                requestJson.AddIfNotNull("show_condensed ", showCondensed);
                requestJson.AddIfNotNull("rows_to_return ", rowsToReturn);
            }

            protected override GetHighScoresResponse ConstructSuccessResponse()
            {
                return new GetHighScoresResponse();
            }

            protected override void RequestSuccess(GetHighScoresResponse response)
            {
                onRequestSuccess?.Invoke(response);
                base.RequestSuccess(response);
            }
        }

        public class GetHighScoresResponse : CoinModeResponse
        {
            public HighScoreRoundInfo roundInfo { get; } = new HighScoreRoundInfo();
            public HighScoreSessionInfo[] sessions { get; private set; } = null;
            internal GetHighScoresResponse() { }

            internal override void FromJson(JsonObject json)
            {
                base.FromJson(json);
                JsonObject roundObject = json["round_info"].AsJsonObject;
                if(roundObject != null)
                {
                    roundInfo.FromJson(roundObject);
                }
                JsonArray sessionsArray = json["sessions"].AsJsonArray;
                if(sessionsArray != null)
                {
                    sessions = new HighScoreSessionInfo[sessionsArray.Count];
                    for(int i = 0; i < sessionsArray.Count; i++)
                    {
                        JsonObject sessionObject = sessionsArray[i].AsJsonObject;
                        if(sessionObject != null)
                        {
                            HighScoreSessionInfo sessionInfo = new HighScoreSessionInfo();
                            sessionInfo.FromJson(sessionObject);
                            sessions[i] = sessionInfo;
                        }
                    }
                }
            }
        }
    }
}
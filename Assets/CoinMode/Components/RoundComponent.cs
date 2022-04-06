using CoinMode.NetApi;
using LightJson;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CoinMode
{
    public delegate void RoundEvent(RoundComponent round);
    public delegate void RoundFailureEvent(RoundComponent round, CoinModeError error);

    public delegate void RoundSessionEvent(RoundComponent round, SessionInfo[] sessions);    

    [System.Serializable]
    public class RoundComponent : CoinModeComponent
    {
        public enum RoundState
        {
            Clean,
            GameIdAssigned,            
            RoundIdAssigned,
            Creating,
            Created,
            GettingInfo,
            GettingHighScores,
            Ending,
            Ready,
            GettingConnectionStatus,
            SettingConnectionStatus,
            GettingPlayerSessions,
        }

        public GameComponent game { get; private set; } = null;

        // Round Info
        public string roundId 
        {
            get { return _roundId; } 
            private set
            {
                if(_roundId != value)
                {
                    string oldId = _roundId;
                    _roundId = value;
                    if(game != null)
                    {
                        game.RoundComponentIdAssigned(this, oldId);
                        if(state < RoundState.RoundIdAssigned)
                        {
                            state = RoundState.RoundIdAssigned;
                        }
                    }
                }
            }

        }
        public string _roundId = "";

        public int epochStarted { get; private set; } = 0;
        public int epochToFinish { get; private set; } = 0;
        public int epochEnded { get; private set; } = 0;
        public bool playerCanPlay { get; private set; } = false;
        public PlayStatus status { get; private set; } = PlayStatus.Unknown;
        public PayoutType payoutType { get; private set; } = PayoutType.None;
        public JsonObject customJson { get; private set; } = null;
        public bool hasReward { get { return potContribution > 0.0D || winningPot > 0.0D; } }

        // Game Info
        public string gameId { get; private set; } = "";
        public string gameName { get; private set; } = "";

        // Costs and Rewards
        public double playFee { get; private set; } = 0.0D;
        public double winningPot { get; private set; } = 0.0D;
        public double potContribution { get; private set; } = 0.0D;

        // Passphrase
        public bool requiresPassphrase { get; private set; } = false;

        public ServerInfo serverInfo { get; private set; } = null;

        public bool roundExpired
        {
            get
            {
                DateTimeOffset epochNow = new DateTimeOffset(DateTime.UtcNow);
                return epochNow.ToUnixTimeSeconds() > epochToFinish;
            }
        }

        public bool advertDataAvailable { get { return advertData != null && advertData.Count > 0; } }
        public ReadOnlyDictionary<string, AdvertData> advertData { get; private set; } = null;

        public RoundState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private RoundState _state = RoundState.Clean;
        private RoundState previousState = RoundState.Clean;

        private RoundEvent createRoundSuccess = null;
        private RoundFailureEvent createRoundFailure = null;

        private RoundEvent getInfoSuccess = null;
        private RoundFailureEvent getInfoFailure = null;

        private RoundEvent getHighScoresSuccess = null;
        private RoundFailureEvent getHighScoresFailure = null;

        private RoundEvent endRoundSuccess = null;
        private RoundFailureEvent endRoundFailure = null;

        private RoundEvent getConnectionStatusSuccess = null;
        private RoundFailureEvent getConnectionStatusFailure = null;

        private RoundSessionEvent listPlayerSessionsSuccess = null;
        private RoundFailureEvent listPlayerSessionsFailure = null;        

        public IEnumerable<HighScore> highScores
        {
            get
            {
                for(int i = 0; i < _highScores.Count; i++)
                {
                    yield return _highScores[i];
                }
            }
        }
        private List<HighScore> _highScores = new List<HighScore>();

        // This is only used when the local user is responsible for creating the round, in this case the passphrase is
        // stored here
        public string localPassphrase { get; private set; } = null;

        internal RoundComponent(GameComponent game, string roundId)
        {
            if (game == null)
            {
                CoinModeLogging.LogWarning("RoundComponent", "RoundComponent()", "Provided game component for round is null");
            }
            
            this.game = game;
            if (!string.IsNullOrWhiteSpace(roundId) && game != null)
            {
                _roundId = roundId != null ? roundId : "";
                state = RoundState.RoundIdAssigned;
            }
            else
            {
                state = this.game != null ? RoundState.GameIdAssigned : RoundState.Clean;
            }            
        }

        public bool Create(PlayerComponent player, string lockKey, bool publicalllyVisbile, double potContribution, string name, string passphrase, JsonObject customJson, string gameServerParams,
            RoundEvent onSuccess, RoundFailureEvent onFailure)
        {
            if (state != RoundState.GameIdAssigned && state != RoundState.RoundIdAssigned)
            {
                if (state == RoundState.Creating)
                {
                    createRoundSuccess -= onSuccess;
                    createRoundSuccess += onSuccess;

                    createRoundFailure -= onFailure;
                    createRoundFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "Create", "Cannot create round while round component is {1}", state.ToString());
                    return false;
                }
            }

            if (player == null)
            {
                CoinModeLogging.LogWarning("RoundComponent", "ListRounds", "Cannot create round, player is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.playToken))
            {
                CoinModeLogging.LogWarning("RoundComponent", "ListRounds", "Cannot create round, players playToken is empty!");
                return false;
            }

            createRoundSuccess = onSuccess;
            createRoundFailure = onFailure;

            state = RoundState.Creating;
            CoinModeManager.SendRequest(GamesRound.Create(player.playToken, game.gameId, "", lockKey, null, publicalllyVisbile, potContribution,
                name, passphrase, null, customJson, gameServerParams, OnCreateRoundSuccess, OnCreateRoundFailure));
            return true;
        }

        private void OnCreateRoundSuccess(GamesRound.CreateRoundResponse response)
        {
            roundId = response.roundId != null ? response.roundId : "";
            epochStarted = response.epochStart != null ? response.epochStart.Value : 0;
            epochToFinish = response.epochToFinish != null ? response.epochToFinish.Value : 0;
            playerCanPlay = true;
            status = PlayStatus.InProgress;
            customJson = response.customJson;
            gameId = response.gameId != null ? response.gameId : "";
            localPassphrase = response.passphrase;
            serverInfo = response.hasServer != null && response.hasServer.Value ? new ServerInfo() : null;
            if (serverInfo != null)
            {
                serverInfo.status = response.serverStatus != null ? response.serverStatus : "";
                serverInfo.ip = response.serverIp != null ? response.serverIp : "";
                serverInfo.port = response.serverPort != null ? response.serverPort : "";
            }

            state = RoundState.Created;
            createRoundSuccess?.Invoke(this);
        }

        private void OnCreateRoundFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            createRoundFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool GetInfo(PlayerComponent player, RoundEvent onSuccess, RoundFailureEvent onFailure)
        {
            if (state != RoundState.RoundIdAssigned && state != RoundState.Created && state != RoundState.Ready)
            {
                if (state == RoundState.GettingInfo)
                {
                    getInfoSuccess -= onSuccess;
                    getInfoSuccess += onSuccess;

                    getInfoFailure -= onFailure;
                    getInfoFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "GetInfo", "Cannot get round info while round is {0}", state.ToString());
                    return false;
                }                
            }

            if (player == null)
            {
                CoinModeLogging.LogWarning("RoundComponent", "GetInfo", "Cannot get round info, player is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(player.playToken))
            {
                CoinModeLogging.LogWarning("RoundComponent", "GetInfo", "Cannot get round info, players playToken is empty!");
                return false;
            }

            getInfoSuccess = onSuccess;
            getInfoFailure = onFailure;

            state = RoundState.GettingInfo;

            CoinModeManager.SendRequest(GamesRound.GetInfo(roundId, player.playToken, OnGetRoundInfoSuccess, OnGetRoundInfoFailure));
            return true;
        }

        private void OnGetRoundInfoSuccess(GamesRound.GetInfoResponse response)
        {
            if (response.properties != null)
            {
                if (response.properties.gameId == game.gameId)
                {
                    roundId = response.properties.roundId != null ? response.properties.roundId : "";
                    epochStarted = response.properties.epochStarted != null ? response.properties.epochStarted.Value : 0;
                    epochToFinish = response.properties.epochToFinish != null ? response.properties.epochToFinish.Value : 0;
                    epochEnded = response.properties.epochEnded != null ? response.properties.epochEnded.Value : 0;
                    playerCanPlay = response.properties.playerCanPlay != null ? response.properties.playerCanPlay.Value : false;
                    status = response.properties.statusId != null ? (PlayStatus)response.properties.statusId.Value : PlayStatus.Unknown;
                    customJson = response.properties.customJson;
                    gameId = response.properties.gameId != null ? response.properties.gameId : "";
                    gameName = response.properties.gameName != null ? response.properties.gameName : "";
                    playFee = response.properties.playFee != null ? response.properties.playFee.Value : 0.0D;
                    winningPot = response.properties.winningPot != null ? response.properties.winningPot.Value : 0.0D;
                    potContribution = response.properties.potContribution != null ? response.properties.potContribution.Value : 0.0D;
                    requiresPassphrase = response.properties.requirePassphrase != null ? response.properties.requirePassphrase.Value : false;
                    serverInfo = response.properties.hasServer != null && response.properties.hasServer.Value ? new ServerInfo() : null;
                    payoutType = response.properties.roundTypeId != null ? (PayoutType)response.properties.roundTypeId : PayoutType.None;
                    if(serverInfo != null)
                    {
                        serverInfo.status = response.properties.serverStatus != null ? response.properties.serverStatus : "";
                        serverInfo.ip = response.properties.serverIp != null ? response.properties.serverIp : "";
                        serverInfo.port = response.properties.serverPort != null ? response.properties.serverPort : "";
                    }

                    if(response.properties.advertData != null && response.properties.advertData.Count > 0)
                    {
                        advertData = new ReadOnlyDictionary<string, AdvertData>(response.properties.advertData);
                    }                    

                    state = RoundState.Ready;
                    getInfoSuccess?.Invoke(this);
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "OnGetRoundInfoSuccess", "Returned round {0} does not belong to expected game {1}, it belongs to {2}",
                        response.properties.roundId, game.gameId, response.properties.gameId);

                    state = previousState;
                    getInfoFailure?.Invoke(this, new CoinModeError(CoinModeErrorBase.ErrorType.Client, "INAVLID_ROUND_RETURNED", "The round returned did not meet the expected parameters."));
                }
            }
            else
            {
                CoinModeLogging.LogWarning("RoundComponent", "OnGetRoundInfoSuccess", "Returned round has no valid properties",
                            response.properties.roundId, game.gameId, response.properties.gameId);

                state = previousState;
                getInfoFailure?.Invoke(this, new CoinModeError(CoinModeErrorBase.ErrorType.Client, "INAVLID_ROUND_RETURNED", "The round returned did not meet the expected parameters."));
            }
        }

        private void OnGetRoundInfoFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getInfoFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool GetHighscores(RoundEvent onSuccess, RoundFailureEvent onFailure)
        {
            return GetHighscores(null, onSuccess, onFailure);
        }

        public bool GetHighscores(SessionComponent sessionToHighlight, RoundEvent onSuccess, RoundFailureEvent onFailure)
        {
            if (state != RoundState.RoundIdAssigned && state != RoundState.Ready)
            {
                if(state == RoundState.GettingHighScores)
                {
                    getHighScoresSuccess -= onSuccess;
                    getHighScoresSuccess += onSuccess;

                    getHighScoresFailure -= onFailure;
                    getHighScoresFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "GetHighscores", "Cannot get high scores while round is {0}", state.ToString());
                    return false;
                }                
            }

            getHighScoresSuccess = onSuccess;
            getHighScoresFailure = onFailure;

            state = RoundState.GettingHighScores;

            string sessionId = sessionToHighlight != null ? sessionToHighlight.sessionId : null;

            CoinModeManager.SendRequest(GamesRound.GetHighScores(roundId, sessionId, null, 50, null, null, OnGetHighScoresSuccess, OnGetHighScoresFailure));
            return false;
        }

        private void OnGetHighScoresSuccess(GamesRound.GetHighScoresResponse response)
        {
            _highScores.Clear();

            if (response.roundInfo.gameId == game.gameId)
            {
                roundId = response.roundInfo.roundId != null ? response.roundInfo.roundId : "";
                epochStarted = response.roundInfo.epochStarted != null ? response.roundInfo.epochStarted.Value : 0;
                epochToFinish = response.roundInfo.epochToFinish != null ? response.roundInfo.epochToFinish.Value : 0;
                epochEnded = response.roundInfo.epochEnded != null ? response.roundInfo.epochEnded.Value : 0;
                status = response.roundInfo.statusId != null ? (PlayStatus)response.roundInfo.statusId.Value : PlayStatus.Unknown;
                customJson = response.roundInfo.customJson;
                gameId = response.roundInfo.gameId != null ? response.roundInfo.gameId : "";
                gameName = response.roundInfo.gameName != null ? response.roundInfo.gameName : "";
                playFee = response.roundInfo.playFee != null ? response.roundInfo.playFee.Value : 0.0D;
                winningPot = response.roundInfo.winningPot != null ? response.roundInfo.winningPot.Value : 0.0D;
                potContribution = response.roundInfo.potContribution != null ? response.roundInfo.potContribution.Value : 0.0D;

                int groupedPosition = -1;

                for (int i = 0; i < response.sessions.Length; i++)
                {
                    PlayStatus sessionStatus = response.sessions[i].statusId != null ? (PlayStatus)response.sessions[i].statusId : PlayStatus.Unknown;
                    if (sessionStatus == PlayStatus.Completed)
                    {
                        if(groupedPosition > -1)
                        {
                            if (response.sessions[i].score != _highScores[_highScores.Count - 1].score)
                            {
                                groupedPosition = response.sessions[i].position != null ? response.sessions[i].position.Value : 0;
                            }
                        }
                        else
                        {
                            groupedPosition = response.sessions[i].position != null ? response.sessions[i].position.Value : 0;                            
                        }

                        _highScores.Add(new HighScore(response.sessions[i], groupedPosition));                                                
                    }
                }

                state = RoundState.Ready;
                getHighScoresSuccess?.Invoke(this);
            }
            else
            {
                CoinModeLogging.LogWarning("RoundComponent", "OnGetHighScoresSuccess", "Returned round {0} does not belong to expected game {1}, it belongs to {2}",
                    response.roundInfo.roundId, game.gameId, response.roundInfo.gameId);

                state = previousState;
            }            
        }

        private void OnGetHighScoresFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getHighScoresFailure?.Invoke(this, new CoinModeError(response));            
        }

        public int GetHighScoresCount()
        {
            return _highScores.Count;
        }

        public HighScore GetHighScoreAtIndex(int index)
        {
            if (index >= 0 && index < _highScores.Count)
            {
                return _highScores[index];
            }
            return null;
        }

        public int GetHighScorePosition(PlayerComponent player)
        {
            int position = -1;
            for (int i = 0; i < _highScores.Count; i++)
            {
                if (player.publicId == _highScores[i].playerId && (_highScores[i].position < position || position == -1))
                {
                    position = _highScores[i].position;
                }
            }
            return position;
        }

        public int GetGroupedHighScorePosition(PlayerComponent player)
        {
            int position = -1;
            for (int i = 0; i <_highScores.Count; i++)
            {
                if (player.publicId == _highScores[i].playerId && (_highScores[i].position < position || position == -1))
                {
                    position = _highScores[i].groupedPosition;
                }
            }
            return position;
        }

        public HighScore GetHighestScoreForPlayer(PlayerComponent player)
        {
            HighScore temp = null;
            for (int i = 0; i < _highScores.Count; i++)
            {
                if (player.publicId == _highScores[i].playerId && (temp == null || _highScores[i].position < temp.position))
                {
                    temp = _highScores[i];
                }
            }
            return temp;
        }

        public HighScore GetHighlightedScore()
        {
            HighScore temp = null;
            for (int i = 0; i < _highScores.Count; i++)
            {
                if (_highScores[i].highlighted)
                {
                    temp = _highScores[i];
                    break;
                }
            }
            return temp;
        }

        public bool ConstructSession(out SessionComponent session)
        {
            session = null;
            if (state != RoundState.Ready)
            {
                CoinModeLogging.LogWarning("RoundComponent", "ConstructSession", "Cannot construct session for round while round {0} is {1}", roundId, state.ToString());
                return false;
            }

            session = new SessionComponent();
            session.AssignRound(this);
            return true;
        }

        public bool ConstructSessionFromExisting(SessionInfo existingSession, out SessionComponent session)
        {
            session = null;
            if (state != RoundState.Ready)
            {
                CoinModeLogging.LogWarning("RoundComponent", "ConstructSessionFromExisting", "Cannot construct session for round while round {0} is {1}", roundId, state.ToString());
                return false;
            }

            if(existingSession == null)
            {
                CoinModeLogging.LogWarning("RoundComponent", "ConstructSessionFromExisting", "Cannot construct session from existing, existing session is null");
                return false;
            }

            session = new SessionComponent();
            session.AssignRound(this);
            session.InitFromExisting(existingSession);
            return true;
        }

        public bool End(string lockKey, bool forceCloseSessions, RoundEvent onSuccess, RoundFailureEvent onFailure)
        {
            if (state != RoundState.RoundIdAssigned && state != RoundState.Ready)
            {
                if (state == RoundState.Ending)
                {
                    endRoundSuccess -= onSuccess;
                    endRoundSuccess += onSuccess;

                    endRoundFailure -= onFailure;
                    endRoundFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "End", "Cannot end round while round is {0}", state.ToString());
                    return false;
                }
            }

            endRoundSuccess = onSuccess;
            endRoundFailure = onFailure;

            state = RoundState.Ending;

            CoinModeManager.SendRequest(GamesRound.End(roundId, lockKey, forceCloseSessions, OnEndRoundSuccess, OnEndRoundFailure));
            return true;
        }

        private void OnEndRoundSuccess(GamesRound.EndRoundResponse response)
        {
            state = RoundState.Ready;
            endRoundSuccess?.Invoke(this);
        }

        private void OnEndRoundFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            endRoundFailure?.Invoke(this, new CoinModeError(response));
        }        

        public bool GetConnectionStatus(RoundEvent onSuccess, RoundFailureEvent onFailure)
        {
            if (state != RoundState.RoundIdAssigned && state != RoundState.Ready)
            {
                if (state == RoundState.GettingConnectionStatus)
                {
                    getConnectionStatusSuccess -= onSuccess;
                    getConnectionStatusSuccess += onSuccess;

                    getConnectionStatusFailure -= onFailure;
                    getConnectionStatusFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "GetConnectionStatus", "Cannot get connection status for round round while round is {0}", state.ToString());
                    return false;
                }
            }

            if (serverInfo == null)
            {
                CoinModeLogging.LogWarning("RoundComponent", "GetConnectionStatus", "Cannot get connection status for round round, serverInfo is null");
                return false;
            }

            getConnectionStatusSuccess = onSuccess;
            getConnectionStatusFailure = onFailure;

            state = RoundState.GettingConnectionStatus;

            CoinModeManager.SendRequest(GamesRoundConnection.GetConnectionInfo(roundId, OnGetConnectionStatusSuccess, OnGetConnectionStatusFailure));
            return true;
        }

        private void OnGetConnectionStatusSuccess(GamesRoundConnection.GetConnectionInfoResponse response)
        {
            if(serverInfo != null && response.hasServer.Value)
            {
                serverInfo.status = response.serverStatus != null ? response.serverStatus : "";
                serverInfo.ip = response.serverIp != null ? response.serverIp : "";
                serverInfo.port = response.serverPort != null ? response.serverPort : "";
            }

            state = previousState;
            getConnectionStatusSuccess?.Invoke(this);
        }

        private void OnGetConnectionStatusFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            getConnectionStatusFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool ListPlayerSessions(PlayerComponent player, RoundSessionEvent onSuccess, RoundFailureEvent onFailure)
        {
            if (state != RoundState.RoundIdAssigned && state != RoundState.Ready)
            {
                if (state == RoundState.GettingPlayerSessions)
                {
                    listPlayerSessionsSuccess -= onSuccess;
                    listPlayerSessionsSuccess += onSuccess;

                    listPlayerSessionsFailure -= onFailure;
                    listPlayerSessionsFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("RoundComponent", "ListPlayerSessions", "Cannot get player sessions for round while round is {0}", state.ToString());
                    return false;
                }
            }

            listPlayerSessionsSuccess = onSuccess;
            listPlayerSessionsFailure = onFailure;

            state = RoundState.GettingPlayerSessions;

            CoinModeManager.SendRequest(GamesRoundSession.List(player.playToken, roundId, player.publicId, OnListSessionsSuccess, OnListSessionsFailure));
            return true;
        }        

        private void OnListSessionsSuccess(GamesRoundSession.ListSessionsResponse response)
        {
            state = previousState;
            listPlayerSessionsSuccess?.Invoke(this, response.sessions);
        }

        private void OnListSessionsFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            listPlayerSessionsFailure?.Invoke(this, new CoinModeError(response));
        }

        public string GetRoundExpiryText()
        {
            DateTimeOffset epochNow = new DateTimeOffset(DateTime.UtcNow);
            TimeSpan seconds = TimeSpan.FromSeconds(Mathf.Max(0.0F, epochToFinish - epochNow.ToUnixTimeSeconds()));
            if (seconds.TotalDays >= 1.0D)
            {
                return string.Format("Round will finish in {0:dd\\.hh\\:mm\\:ss} days.", seconds);
            }
            else if (seconds.TotalHours >= 1.0D)
            {
                return string.Format("Round will finish in {0:hh\\:mm\\:ss} hours.", seconds);
            }
            else if (seconds.TotalMinutes >= 1.0D)
            {
                return string.Format("Round will finish in {0:mm\\:ss} minutes.", seconds);
            }
            else
            {
                return string.Format("Round will finish in {0:ss} seconds.", seconds);
            }
        }
    }
}

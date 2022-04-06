using CoinMode.NetApi;
using LightJson;

namespace CoinMode
{
    public delegate void SessionEvent(SessionComponent session);
    public delegate void SessionFailureEvent(SessionComponent session, CoinModeError error);

    public delegate void SessionLeaderboardEvent(HighScorePositionInfo position);

    [System.Serializable]
    public class SessionComponent : CoinModeComponent
    {
        public enum SessionState
        {
            Clean = 0,
            RoundIdAssigned = 1,
            Requesting = 2,
            Requested = 3,
            Starting = 4,
            Started = 5,
            Stopping = 6,
            Stopped = 7,
            Error = 8,
        }

        public SessionState state
        {
            get { return _state; }
            private set
            {
                previousState = _state;
                _state = value;
            }
        }
        private SessionState _state = SessionState.Clean;
        private SessionState previousState = SessionState.Clean;

        internal SessionComponent() { }

        public RoundComponent round { get; private set; } = null;
        public string roundId { get; private set; } = "";
        public string sessionId { get; private set; } = "";

        public double score { get; private set; } = 0.0D;
        public string formattedScore { get; private set; } = "";
        public JsonObject sessionData { get; private set; } = null;

        public SessionLeaderboardEvent onLeaderboardPositionUpdated = null;

        private SessionEvent requestSuccess = null;
        private SessionFailureEvent requestFailure = null;

        private SessionEvent startSuccess = null;
        private SessionFailureEvent startFailure = null;

        private SessionEvent stopSuccess = null;
        private SessionFailureEvent stopFailure = null;

        private int currentHighScorePosition = -1;

        public string localPassphrase { get; private set; }

        internal bool AssignRound(RoundComponent round)
        {
            if (state != SessionState.Clean)
            {
                CoinModeLogging.LogWarning("SessionComponent", "AssignRound", "Cannot set round while {0}", state.ToString());
                return false;
            }

            if (round == null)
            {
                CoinModeLogging.LogWarning("SessionComponent", "AssignRound", "Cannot set round , provided round is null");
                return false;
            }

            if (string.IsNullOrEmpty(round.roundId))
            {
                CoinModeLogging.LogWarning("SessionComponent", "AssignRound", "Cannot set round, provided rounds id is invalid");
                return false;
            }

            state = SessionState.RoundIdAssigned;
            this.round = round;
            roundId = round.roundId;
            return true;
        }

        internal bool InitFromExisting(SessionInfo existingSession)
        {
            if (state != SessionState.Clean && state != SessionState.RoundIdAssigned)
            {
                CoinModeLogging.LogWarning("SessionComponent", "InitFromExisting", "Cannot init session from existing while session is {0}", state.ToString());
                return false;
            }

            roundId = existingSession.roundId != null ? existingSession.roundId : "";
            sessionId = existingSession.sessionId != null ? existingSession.sessionId : "";
            score = existingSession.score != null ? existingSession.score.Value : 0;
            //formattedScore = existingSession.formattedScore != null ? existingSession.formattedScore : "";
            sessionData = existingSession.serverResults;
            PlayStatus status = existingSession.sessionStatus != null ? (PlayStatus)existingSession.sessionStatus : PlayStatus.Unknown;
            switch (status)
            {
                case PlayStatus.WaitingToPlay:
                    _state = SessionState.Requested;
                    break;
                case PlayStatus.InProgress:
                    _state = SessionState.Started;
                    break;
                case PlayStatus.Completed:
                    _state = SessionState.Stopped;
                    break;
                default:
                    _state = SessionState.Error;
                    break;
            }
            return true;
        }

        public bool Request(PlayerComponent player, string passphrase, string ip, SessionEvent onSuccess, SessionFailureEvent onFailure)
        {
            if (state != SessionState.RoundIdAssigned && state != SessionState.Stopped)
            {
                if (state == SessionState.Requesting)
                {
                    requestSuccess -= onSuccess;
                    requestSuccess += onSuccess;

                    requestFailure -= onFailure;
                    requestFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("SessionComponent", "Request", "Cannot request new session while {0}", state.ToString());
                    return false;
                }
            }

            if (!IsPlayerAndPlayTokenValid(player))
            {
                CoinModeLogging.LogWarning("SessionComponent", "Request", "Cannot request session, player or play token is invalid!");
                return false;
            }

            requestSuccess = onSuccess;
            requestFailure = onFailure;

            localPassphrase = passphrase;

            state = SessionState.Requesting;
            CoinModeManager.SendRequest(GamesRoundSession.Request(player.playToken, roundId, ip, passphrase, OnRequestSuccess, OnRequestFailure));
            return true;
        }

        private void OnRequestSuccess(GamesRoundSession.RequestSessionResponse response)
        {
            state = SessionState.Requested;
            sessionId = response.sessionId;
            requestSuccess?.Invoke(this);
        }

        private void OnRequestFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            requestFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool Start(PlayerComponent player, SessionEvent onSuccess, SessionFailureEvent onFailure)
        {
            if (state != SessionState.Requested)
            {
                if (state == SessionState.Starting)
                {
                    startSuccess -= onSuccess;
                    startSuccess += onSuccess;

                    startFailure -= onFailure;
                    startFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("SessionComponent", "Start", "Cannot start session while {0}", state.ToString());
                    return false;
                }
            }

            if (!IsPlayerAndPlayTokenValid(player))
            {
                CoinModeLogging.LogWarning("SessionComponent", "Start", "Cannot start session, player or play token is invalid!");
                return false;
            }

            startSuccess = onSuccess;
            startFailure = onFailure;

            state = SessionState.Starting;
            CoinModeManager.SendRequest(GamesRoundSession.Start(player.playToken, sessionId, OnStartSuccess, OnStartFailure));
            return true;
        }

        private void OnStartSuccess(GamesRoundSession.StartSessionResponse response)
        {
            state = SessionState.Started;
            startSuccess?.Invoke(this);
        }

        private void OnStartFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            startFailure?.Invoke(this, new CoinModeError(response));
        }

        public bool UpdateScore(double score)
        {
            if (state != SessionState.Started)
            {
                CoinModeLogging.LogWarning("SessionComponent", "SetScore", "Cannot set score while {0}", state.ToString());
                return false;
            }

            if (round.GetHighScoresCount() > 0 && onLeaderboardPositionUpdated != null)
            {
                int start = currentHighScorePosition > -1 ? currentHighScorePosition : round.GetHighScoresCount() - 1;
                int highestBeaten = -1;
                for (int i = start; i >= 0; i--)
                {
                    if (score > round.GetHighScoreAtIndex(i).score)
                    {
                        highestBeaten = i;
                    }
                }

                if (highestBeaten != currentHighScorePosition)
                {
                    currentHighScorePosition = highestBeaten;
                    HighScore lowScore = round.GetHighScoreAtIndex(highestBeaten);
                    HighScore highScore = highestBeaten > 0 ? round.GetHighScoreAtIndex(highestBeaten - 1) : null;
                    HighScorePositionInfo newPosition = new HighScorePositionInfo(score, lowScore.groupedPosition, lowScore, highScore);
                    onLeaderboardPositionUpdated.Invoke(newPosition);
                }
            }

            this.score = score;
            this.formattedScore = score.ToString();
            return true;
        }

        public bool SetScore(double score)
        {
            return SetScore(score, score.ToString());
        }

        public bool SetScore(double score, string formattedScore)
        {
            if (state != SessionState.Started)
            {
                CoinModeLogging.LogWarning("SessionComponent", "SetScore", "Cannot set score while {0}", state.ToString());
                return false;
            }

            this.score = score;
            this.formattedScore = formattedScore;
            return true;
        }

        public bool SetSessionData(JsonObject sessionData)
        {
            if (state != SessionState.Started)
            {
                CoinModeLogging.LogWarning("SessionComponent", "SetScore", "Cannot set score while {0}", state.ToString());
                return false;
            }

            this.sessionData = sessionData;
            return true;
        }

        public bool Stop(double score, string formattedScore, JsonObject sessionData, SessionEvent onSuccess, SessionFailureEvent onFailure)
        {
            SetScore(score, formattedScore);
            SetSessionData(sessionData);
            return Stop(onSuccess, onFailure);
        }

        public bool Stop(SessionEvent onSuccess, SessionFailureEvent onFailure)
        {
            if (state != SessionState.Started)
            {
                if (state == SessionState.Stopping)
                {
                    stopSuccess -= onSuccess;
                    stopSuccess += onSuccess;

                    stopFailure -= onFailure;
                    stopFailure += onFailure;
                    return true;
                }
                else
                {
                    CoinModeLogging.LogWarning("SessionComponent", "Stop", "Cannot stop session while {0}", state.ToString());
                    return false;
                }
            }

            stopSuccess = onSuccess;
            stopFailure = onFailure;

            state = SessionState.Stopping;
            string encodedData = CoinModeEncryption.EncryptString(score.ToString(), CoinModeEncryption.CreateHashedKey(sessionId));
            string encryptedFormattedScore = CoinModeEncryption.EncryptString(formattedScore, CoinModeEncryption.CreateHashedKey(sessionId));
            CoinModeManager.SendRequest(GamesRoundSession.Stop(sessionId, score, encodedData, encryptedFormattedScore, sessionData, null, OnStopSuccess, OnStopFailure));
            return true;
        }

        private void OnStopSuccess(GamesRoundSession.StopSessionResponse response)
        {
            state = SessionState.Stopped;
            stopSuccess?.Invoke(this);
        }

        private void OnStopFailure(CoinModeErrorResponse response)
        {
            state = previousState;
            stopFailure?.Invoke(this, new CoinModeError(response));
        }
    }
}

using LightJson;

namespace CoinMode
{
    public enum PlayerAuthMode
    {
        None,
        UuidOrEmail,
        Discord,
        Google,
        Apple
    }

    public enum CoinModeEnvironment
    {
        Production,
        Staging
    }

    public struct HighScorePositionInfo
    {
        public double score { get; private set; }

        public int position { get; private set; }

        public HighScore closestLowScore { get; private set; }

        public HighScore closestHighScore { get; private set; }

        public string GetOvertakenString()
        {
            if(closestLowScore != null)
            {
                return string.Format("You have overtaken {0}'s score of {1} You are placed {2}{3}",
                    closestLowScore.displayName, closestLowScore.score, position, HighScore.GetOrdinal(position));
            }
            return string.Format("You are now placed {0}{1}", position, HighScore.GetOrdinal(position));
        }

        public string GetTargetString()
        {
            if(closestHighScore != null)
            {
                return string.Format("You are {0} away from {1}'s score of {2}",
                    closestHighScore.score - score, closestHighScore.displayName, closestHighScore.score);
            }
            return string.Format("You are now placed {0}{1}", position, HighScore.GetOrdinal(position));
        }

        public HighScorePositionInfo(double score, int position, HighScore closestLowScore, HighScore closestHighScore)
        {
            this.score = score;
            this.position = position;
            this.closestLowScore = closestLowScore;
            this.closestHighScore = closestHighScore;
        }
    }

    [System.Serializable]
    public class HighScore
    {
        public double score { get; private set; } = 0.0D;
        public string formattedScore { get; private set; } = "";
        public int position { get; private set; } = 0;
        public int groupedPosition { get; private set; } = 0;
        public string displayName { get; private set; } = "";
        public string playerId { get; private set; } = "";
        public string avatarImageUrl { get; private set; } = "";
        public PlayStatus sessionStatus { get; private set; } = PlayStatus.Unknown;
        public double paidOut { get; private set; } = 0.0D;
        public JsonObject sessionData { get; private set; } = null;
        public bool highlighted { get; private set; } = false;

        internal HighScore(HighScoreSessionInfo sessionInfo, int groupedPosition)
        {
            score = sessionInfo.score != null ? sessionInfo.score.Value : 0;
            formattedScore = sessionInfo.formattedScore != null ? sessionInfo.formattedScore : "";
            position = sessionInfo.position != null ? sessionInfo.position.Value : 0;
            displayName = sessionInfo.displayName != null ? sessionInfo.displayName : "";
            playerId = sessionInfo.playerId != null ? sessionInfo.playerId : "";
            avatarImageUrl = sessionInfo.avatarImageUrlSmall != null ? sessionInfo.avatarImageUrlSmall : "";
            sessionStatus = sessionInfo.statusId != null ? (PlayStatus)sessionInfo.statusId : PlayStatus.Unknown;
            paidOut = sessionInfo.paidOut != null ? sessionInfo.paidOut.Value : 0.0D;
            this.groupedPosition = groupedPosition;
            sessionData = sessionInfo.clientResults;
            highlighted = sessionInfo.selected != null ? sessionInfo.selected.Value : false;
        }

        static public string GetOrdinal(int position)
        {
            switch (position % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }

            switch (position % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }
    }

    [System.Serializable]
    public class ServerInfo
    {
        public string status { get; internal set; }
        public string ip { get; internal set; }
        public string port { get; internal set; }
    }

#if UNITY_2019_3_OR_NEWER
    [System.Serializable]
    public struct RoundInviteInfo
    {
        public string challengingPlayer { get { return _challengingPlayer; } }
        private string _challengingPlayer;
        public string challengingScore { get { return _challengingScore; } }
        private string _challengingScore;
        public string passphrase { get { return _passphrase; } }
        private string _passphrase;

        public RoundInviteInfo(string challengingPlayer, string challengingScore, string passphrase)
        {
            _challengingPlayer = challengingPlayer;
            _challengingScore = challengingScore;
            _passphrase = passphrase;
        }

        public RoundInviteInfo(DeepLinkParams deepLinkParams)
        {
            deepLinkParams.ContainsPlayerDisplayName(out _challengingPlayer);
            deepLinkParams.ContainsFormattedScore(out _challengingScore);
            deepLinkParams.ContainsPassphrase(out _passphrase);
        }
    }
#endif

#if UNITY_2019_3_OR_NEWER
    [System.Flags]
    public enum ChallengeType
    {
        /// <summary>Does not support challenges.</summary>
        None = 0,
        /// <summary>Creates a new round and challenges between invited players only.</summary>
        PvP = 1,
        /// <summary>Invites player to an existing round to play against their score.</summary>
        RoundInvite = 2,
    }
#endif
}
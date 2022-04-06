using LightJson;
using System.Collections.Generic;
using UnityEngine.Events;

namespace CoinMode.UI
{
    public struct JoinRoundScreenData
    {
        public PlayerComponent player { get; private set; }
        public GameComponent game { get; private set; }
        public RoundComponent round { get; private set; }
        public SessionComponent session { get; private set; }
        public bool startSessionImmediately { get; private set; }

        public bool isChallenge { get; private set; }
        public string challengingPlayer { get; private set; }
        public string challengingScore { get; private set; }
        public string passphrase { get; private set; }

        public JoinRoundScreenData(PlayerComponent player, GameComponent game, RoundComponent round, SessionComponent session, bool startSessionImmediately)
        {
            this.player = player;
            this.game = game;
            this.round = round;
            this.session = session;
            this.startSessionImmediately = startSessionImmediately;
            isChallenge = false;
            challengingPlayer = null;
            challengingScore = null;
            passphrase = null;
        }

        public JoinRoundScreenData(PlayerComponent player, GameComponent game, RoundComponent round, SessionComponent session, bool startSessionImmediately, 
            bool isChallenge, string challengingPlayer, string challengingScore, string passphrase)
        {
            this.player = player;
            this.game = game;
            this.round = round;
            this.session = session;
            this.startSessionImmediately = startSessionImmediately;
            this.isChallenge = isChallenge;
            this.challengingPlayer = challengingPlayer;
            this.challengingScore = challengingScore;
            this.passphrase = passphrase;
        }
    }

    public class HighScoreScreenConfig
    {
        public static HighScoreScreenConfig defaultConfig = new HighScoreScreenConfig();

#if UNITY_2019_3_OR_NEWER
        public ChallengeType supportedChallengeType { get; private set; } = ChallengeType.None;
#endif
        public bool clearAdvertDataOnClose { get; private set; } = false;
        public bool useExistingScores { get; internal set; } = false;

        private HighScoreScreenConfig() { }
            
        public HighScoreScreenConfig(bool clearAdvertDataOnClose, bool useExistingScores)
        {
#if UNITY_2019_3_OR_NEWER
            supportedChallengeType = ChallengeType.None;
#endif
            this.clearAdvertDataOnClose = clearAdvertDataOnClose;
            this.useExistingScores = useExistingScores;
        }

#if UNITY_2019_3_OR_NEWER
        public HighScoreScreenConfig(ChallengeType supportedChallengeType, bool clearAdvertDataOnClose, bool useExistingScores)
        {
            this.supportedChallengeType = supportedChallengeType;
            this.clearAdvertDataOnClose = clearAdvertDataOnClose;
            this.useExistingScores = useExistingScores;
        }

        public bool SupportsChallengeType(ChallengeType type)
        {
            return supportedChallengeType.HasFlag(type);
        }
#endif
    }

    public struct HighScoreScreenData
    {
        public PlayerComponent player { get; private set; }
        public GameComponent game { get; private set; }
        public RoundComponent round { get; private set; }
        public SessionComponent session { get; private set; }
        public JsonObject challengeCustomJson { get; private set; }
        public HighScoreScreenConfig screenConfig { get; private set; }

        public bool supportsChallenge
        {
            get
            {
                if(game != null)
                {
                    return (screenConfig.supportedChallengeType.HasFlag(ChallengeType.PvP) &&
                        CoinModeSettings.GameSupportsPvpChallenge(game.localAlias)) ||
                        (screenConfig.supportedChallengeType.HasFlag(ChallengeType.RoundInvite) &&
                        CoinModeSettings.GameSupportsRoundInvite(game.localAlias));
                }
                return false;
            }
        }

        public HighScoreScreenData(PlayerComponent player, GameComponent game, RoundComponent round, JsonObject challengeCustomJson,
            HighScoreScreenConfig screenConfig)
        {
            this.player = player;
            this.game = game;
            this.round = round;
            session = null;
            this.challengeCustomJson = challengeCustomJson;
            this.screenConfig = screenConfig != null ? screenConfig : HighScoreScreenConfig.defaultConfig;
        }

        public HighScoreScreenData(PlayerComponent player, GameComponent game, RoundComponent round, SessionComponent session, JsonObject challengeCustomJson,
            HighScoreScreenConfig screenConfig)
        {
            this.player = player;
            this.game = game;
            this.round = round;
            this.session = session;
            this.challengeCustomJson = challengeCustomJson;
            this.screenConfig = screenConfig != null ? screenConfig : HighScoreScreenConfig.defaultConfig;
        }

        public bool SupportsChallengeType(ChallengeType type)
        {
            switch (type)
            {
                case ChallengeType.RoundInvite:
                    return screenConfig.supportedChallengeType.HasFlag(ChallengeType.RoundInvite) &&
                        CoinModeSettings.GameSupportsRoundInvite(game.localAlias);
                case ChallengeType.PvP:
                    return screenConfig.supportedChallengeType.HasFlag(ChallengeType.PvP) &&
                        CoinModeSettings.GameSupportsPvpChallenge(game.localAlias);
                default:
                    return screenConfig.supportedChallengeType.HasFlag(type);
            }
        }
    }
}

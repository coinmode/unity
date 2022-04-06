#if UNITY_2019_3_OR_NEWER
using CoinMode.NetApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoinMode
{
    public class DeepLinkUtilities
    {
        public const string urlHost = "coinmode";
        public const string challengeKey = "pc";
        public const string roundInviteKey = "ri";

        public static string CreateChallengeLink(SessionComponent challengeSession)
        {
            return CreateShareLink(challengeKey, challengeSession.round, challengeSession);
        }

        public static string CreateChallengeLink(RoundComponent challengeRound)
        {
            return CreateShareLink(challengeKey, challengeRound, null);
        }

        public static string CreateRoundInviteLink(SessionComponent challengeSession)
        {
            return CreateShareLink(roundInviteKey, challengeSession.round, challengeSession);
        }

        public static string CreateRoundInviteLink(RoundComponent challengeRound)
        {
            return CreateShareLink(roundInviteKey, challengeRound, null);
        }

        private static string CreateShareLink(string type, RoundComponent challengeRound, SessionComponent challengeSession)
        {
            if (challengeRound != null)
            {
                string baseUrl = challengeSession != null ? CoinModeManager.GetShareUrl(challengeSession) : CoinModeManager.GetShareUrl(challengeRound);
                StringBuilder urlBuilder = new StringBuilder(baseUrl);
                urlBuilder.Append("?");
                urlBuilder.Append(type);
                return urlBuilder.ToString();
            }
            return "";
        }
    }

    public class DeepLinkParams
    {
        public string urlScheme { get; private set; } = "";

        public Dictionary<string, string> namedValues { get; private set; } = null;
        public HashSet<string> unnamedValues { get; private set; } = null;

        public DeepLinkParams(string scheme, string deeplinkUrl)
        {
            urlScheme = scheme;
            namedValues = new Dictionary<string, string>();
            unnamedValues = new HashSet<string>();
            string[] splitUrl = deeplinkUrl.Split('?');
            if (splitUrl.Length == 2)
            {
                // New format
                string[] splitParams = splitUrl[1].Split('&');
                PopulateValues(0, splitParams, namedValues, unnamedValues);
            }
            else if(splitUrl.Length > 2)
            {
                // Old format
                PopulateValues(1, splitUrl, namedValues, unnamedValues);
            }
            else
            {
                CoinModeLogging.LogError("DeepLinkParams", "DeepLinkParams", "Unable to populate deeplink params from provided url!");
            }
        }

        private void PopulateValues(int startIndex, string[] urlParams, Dictionary<string, string> namedValues, HashSet<string> unnamedValues)
        {
            for (int i = startIndex; i < urlParams.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(urlParams[i]))
                {
                    int equalsIndex = urlParams[i].IndexOf('=');
                    if (equalsIndex >= 1 && urlParams[i].Length > equalsIndex + 1)
                    {
                        int valueLength = urlParams[i].Length - equalsIndex - 1;
                        namedValues[urlParams[i].Substring(0, equalsIndex)] = urlParams[i].Substring(equalsIndex + 1, valueLength);
                    }
                    else if (equalsIndex == -1)
                    {
                        unnamedValues.Add(urlParams[i]);
                    }
                }
            }
        }

        public bool FindNamedValue(string name, out string value)
        {
            return namedValues.TryGetValue(name, out value);
        }

        public bool DoesValueExist(string value)
        {
            return unnamedValues.Contains(value);
        }

        public bool IsPlayerChallenge()
        {
            return unnamedValues != null && unnamedValues.Contains(DeepLinkUtilities.challengeKey);
        }

        public bool IsRoundInvite()
        {
            return unnamedValues != null && unnamedValues.Contains(DeepLinkUtilities.roundInviteKey);
        }

        public bool IsSharedRound()
        {
            return unnamedValues != null && (unnamedValues.Contains(DeepLinkUtilities.challengeKey) ||
                unnamedValues.Contains(DeepLinkUtilities.roundInviteKey));
        }

        public bool ContainsGameId(out string gameId)
        {
            return namedValues.TryGetValue("g", out gameId);
        }

        public bool ContainsRoundId(out string roundId)
        {
            return namedValues.TryGetValue("r", out roundId);
        }

        public bool ContainsSessionId(out string sessionId)
        {
            return namedValues.TryGetValue("s", out sessionId);
        }

        public bool ContainsGameAlias(out string gameAlias)
        {
            return namedValues.TryGetValue("ga", out gameAlias);
        }

        public bool ContainsPlayerDisplayName(out string displayName)
        {
            return namedValues.TryGetValue("pn", out displayName);
        }

        public bool ContainsFormattedScore(out string score)
        {
            return namedValues.TryGetValue("sc", out score);
        }

        public bool ContainsPassphrase(out string passphrase)
        {
            return namedValues.TryGetValue("pw", out passphrase);
        }

        private bool TryCreateRound(out RoundComponent roundComponent)
        {
            roundComponent = null;
            string gameId, roundId;
            if (namedValues.TryGetValue("g", out gameId) && namedValues.TryGetValue("r", out roundId))
            {
                GameComponent game = CoinModeManager.titleComponent.TryGetGameFromId(gameId);
                if (game != null)
                {
                    roundComponent = game.FindOrConstructRound(roundId);
                    return true;
                }
            }
            return false;
        }

        public RoundComponent TryCreateRound()
        {
            RoundComponent round;
            TryCreateRound(out round);
            return round;
        }        
    }
}
#endif
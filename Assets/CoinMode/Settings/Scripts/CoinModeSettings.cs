using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif
using UnityEngine;

namespace CoinMode
{
    [DefaultExecutionOrder(-202)]
    public class CoinModeSettings : ScriptableObject
    {
        [System.Serializable]
        public struct GameSetup
        {
            [Tooltip("A unique local alias used throughout the plugin to identify your game and associate CoinMode game id.")]
            public string localGameAlias;
            [Tooltip("The unique identifier for this game as assigned on the CoinMode production website.")]
            public string productionCoinModeGameId;
            [Tooltip("The unique identifier for this game as assigned on the CoinMode staging website.")]
            public string stagingCoinModeGameId;

#if UNITY_2019_3_OR_NEWER
            [Tooltip("Controls whether or not the plugin allows the automatic sharing and inviting of players to rounds in this game.")]
            public bool supportsInvites;
            [Tooltip("Controls whether or not the plugin allows the automatic handling of pvp challenges for this game.")]
            public bool supportsPvpChallenge;
            [Tooltip("Whether or not pvp challenges should use this game or a separate game.")]
            public bool useDifferentGame;
            [Tooltip("The local unique alias for the game linked to this as a pvp challenge template.")]
            public int pvpChallengeGameIndex;

            [Tooltip("The deeplink url scheme used when sharing pvp challenges or inviting to rounds.")]
            public int deeplinkSchemeIndex;

            public int linkedGameIndex;
#endif
        }

        public static string titleId 
        {
            get
            {
                switch (environment)
                {
                    case CoinModeEnvironment.Production:
                        if(string.IsNullOrWhiteSpace(instance._productionTitleId))
                        {
                            CoinModeLogging.LogError("CoinModeSettings", "titleId", "There is no title ID set up for production!");
                        }
                        return instance._productionTitleId;
                    case CoinModeEnvironment.Staging:
                        if (string.IsNullOrWhiteSpace(instance._stagingTitleId))
                        {
                            CoinModeLogging.LogError("CoinModeSettings", "titleId", "There is no title ID set up for staging!");
                        }
                        return instance._stagingTitleId;
                }
                return instance._productionTitleId; 
            } 
        }

        internal static List<GameSetup> games { get { return instance._games; } }

        public static string defaultGameId 
        { 
            get 
            { 
                if(instance._games.Count > 0)
                {
                    if(instance._defaultGameIdIndex >= 0 && instance._defaultGameIdIndex < instance._games.Count)
                    {
                        switch (environment)
                        {
                            default:
                            case CoinModeEnvironment.Production:
                                if (string.IsNullOrWhiteSpace(instance._games[instance._defaultGameIdIndex].productionCoinModeGameId))
                                {
                                    CoinModeLogging.LogError("CoinModeSettings", "defaultGameId", 
                                        "There is no game ID set up for production for the default game!");
                                }
                                return instance._games[instance._defaultGameIdIndex].productionCoinModeGameId;
                            case CoinModeEnvironment.Staging:
                                if (string.IsNullOrWhiteSpace(instance._games[instance._defaultGameIdIndex].stagingCoinModeGameId))
                                {
                                    CoinModeLogging.LogError("CoinModeSettings", "defaultGameId",
                                        "There is no game ID set up for staging for the default game!");
                                }
                                return instance._games[instance._defaultGameIdIndex].stagingCoinModeGameId;
                        }                        
                    }
                    else
                    {
                        switch (environment)
                        {
                            default:
                            case CoinModeEnvironment.Production:
                                if (string.IsNullOrWhiteSpace(instance._games[0].productionCoinModeGameId))
                                {
                                    CoinModeLogging.LogError("CoinModeSettings", "defaultGameId",
                                        "There is no game ID set up for production for the default game!");
                                }
                                return instance._games[0].productionCoinModeGameId;
                            case CoinModeEnvironment.Staging:
                                if (string.IsNullOrWhiteSpace(instance._games[0].stagingCoinModeGameId))
                                {
                                    CoinModeLogging.LogError("CoinModeSettings", "defaultGameId",
                                        "There is no game ID set up for staging for the default game!");
                                }
                                return instance._games[0].stagingCoinModeGameId;                            
                        }                        
                    }                    
                }
                else
                {
                    CoinModeLogging.LogError("CoinModeSettings", "defaultGameId", "There are no games set up for the current project!");
                    return "";
                }
            } 
        }

        public static string defaultGameAlias
        {
            get
            {
                if (instance._games.Count > 0)
                {
                    if (instance._defaultGameIdIndex >= 0 && instance._defaultGameIdIndex < instance._games.Count)
                    {
                        return instance._games[instance._defaultGameIdIndex].localGameAlias;
                    }
                    else
                    {
                        return instance._games[0].localGameAlias;
                    }
                }
                else
                {
                    CoinModeLogging.LogError("CoinModeSettings", "defaultGameId", "There are no games set up for the current project!");
                    return "";
                }
            }
        }

        internal static bool allowUserCreatedRoundsForMoney { get { return instance._allowUserCreatedRoundsForMoney; } }

        internal static CoinModeEnvironment environment { get { return instance._environment; } }

        internal static Vector2 referenceResolution { get { return instance._referenceResolution; } }

        internal static CoinModeMenu.MessageDisplayMode menuMessageDisplayMode { get { return instance._menuMessageDisplayMode; } }

        internal static CoinModeLogging.LoggingMode loggingMode { get { return instance._loggingMode; } }

        internal static Analytics.Settings analyticsSettings { get { return instance._analyticsSettings; } }

#if UNITY_2019_3_OR_NEWER
        internal static List<string> deeplinkUrlSchemes { get { return instance._deeplinkUrlSchemes; } }
#endif

        internal static CoinModeSettings instance
        { 
            get
            {
                if(_instance == null)
                {
                    InitSettings();
                }
                return _instance;
            }
        }
        private static CoinModeSettings _instance = null;

        public static string GetGameId(string localGameAlias)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias)
                {
                    switch (environment)
                    {
                        default:
                        case CoinModeEnvironment.Production:
                            if (string.IsNullOrWhiteSpace(instance._games[i].productionCoinModeGameId))
                            {
                                CoinModeLogging.LogError("CoinModeSettings", "GetGameId",
                                    "There is no game ID set up for production for the game {0}!", localGameAlias);
                            }
                            return instance._games[i].productionCoinModeGameId;
                        case CoinModeEnvironment.Staging:
                            if (string.IsNullOrWhiteSpace(instance._games[i].stagingCoinModeGameId))
                            {
                                CoinModeLogging.LogError("CoinModeSettings", "GetGameId",
                                    "There is no game ID set up for staging for the game  {0}!", localGameAlias);
                            }
                            return instance._games[i].stagingCoinModeGameId;

                    }
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GetGameId", "The local game alias {0} could not be found in settings!", localGameAlias);
            return "";
        }

        public static string GetLocalGameAlias(string gameId)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                string envGameId = "";
                switch (environment)
                {
                    default:
                    case CoinModeEnvironment.Production:
                        envGameId = instance._games[i].productionCoinModeGameId;
                        break;
                    case CoinModeEnvironment.Staging:
                        envGameId = instance._games[i].stagingCoinModeGameId;
                        break;

                }
                if (envGameId == gameId)
                {
                    return instance._games[i].localGameAlias;
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GetLocalGameAlias", "The gameId ({0}) does not have a linked local alias in CoinMode Settings!", gameId);
            return "";
        }

#if UNITY_2019_3_OR_NEWER
        public static bool GameSupportsRoundInvite(string localGameAlias)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias)
                {
                    return instance._games[i].supportsInvites;
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GameSupportsRoundInvite", "The local game alias {0} could not be found in settings!", localGameAlias);
            return false;
        }
        public static bool GameSupportsPvpChallenge(string localGameAlias)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias)
                {
                    return instance._games[i].supportsPvpChallenge;
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GameSupportsChallenge", "The local game alias {0} could not be found in settings!", localGameAlias);
            return false;
        }
        public static ChallengeType GameSupportedChallengeTypes(string localGameAlias)
        {
            ChallengeType challengeTypes = ChallengeType.None;
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias)
                {
                    if(instance._games[i].supportsPvpChallenge)
                    {
                        challengeTypes |= ChallengeType.PvP;
                    }

                    if (instance._games[i].supportsInvites)
                    {
                        challengeTypes |= ChallengeType.RoundInvite;
                    }
                    return challengeTypes;
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GameSupportedChallengeTypes", "The local game alias {0} could not be found in settings!", localGameAlias);
            return challengeTypes;
        }

        public static string GetPvpChallengeGameId(string localGameAlias)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias && instance._games[i].supportsPvpChallenge)
                {
                    if(instance._games[i].useDifferentGame)
                    {
                        if(instance._games[i].pvpChallengeGameIndex >= 0 && instance._games[i].pvpChallengeGameIndex < instance._games.Count)
                        {
                            switch (environment)
                            {
                                default:
                                case CoinModeEnvironment.Production:
                                    if (string.IsNullOrWhiteSpace(instance._games[instance._games[i].pvpChallengeGameIndex].productionCoinModeGameId))
                                    {
                                        CoinModeLogging.LogError("CoinModeSettings", "GetGameId",
                                            "There is no game ID set up for production for the game {0}!", localGameAlias);
                                    }
                                    return instance._games[instance._games[i].pvpChallengeGameIndex].productionCoinModeGameId;
                                case CoinModeEnvironment.Staging:
                                    if (string.IsNullOrWhiteSpace(instance._games[instance._games[i].pvpChallengeGameIndex].stagingCoinModeGameId))
                                    {
                                        CoinModeLogging.LogError("CoinModeSettings", "GetGameId",
                                            "There is no game ID set up for staging for the game  {0}!", localGameAlias);
                                    }
                                    return instance._games[instance._games[i].pvpChallengeGameIndex].stagingCoinModeGameId;

                            }
                        }
                        else
                        {
                            CoinModeLogging.LogError("CoinModeSettings", "GetChallengeGameId", "Challenge game not found for {0}!", localGameAlias);
                        }                        
                    }
                    else
                    {
                        switch (environment)
                        {
                            default:
                            case CoinModeEnvironment.Production:
                                if (string.IsNullOrWhiteSpace(instance._games[i].productionCoinModeGameId))
                                {
                                    CoinModeLogging.LogError("CoinModeSettings", "GetGameId",
                                        "There is no game ID set up for production for the game {0}!", localGameAlias);
                                }
                                return instance._games[i].productionCoinModeGameId;
                            case CoinModeEnvironment.Staging:
                                if (string.IsNullOrWhiteSpace(instance._games[i].stagingCoinModeGameId))
                                {
                                    CoinModeLogging.LogError("CoinModeSettings", "GetGameId",
                                        "There is no game ID set up for staging for the game  {0}!", localGameAlias);
                                }
                                return instance._games[i].stagingCoinModeGameId;

                        }
                    }
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GetChallengeGameId", "The local game alias {0} could not be found in settings!", localGameAlias);
            return "";
        }

        public static string GetPvpChallengeGameAlias(string localGameAlias)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias && instance._games[i].supportsPvpChallenge)
                {
                    if (instance._games[i].useDifferentGame)
                    {
                        if (instance._games[i].pvpChallengeGameIndex >= 0 && instance._games[i].pvpChallengeGameIndex < instance._games.Count)
                        {
                            return instance._games[instance._games[i].pvpChallengeGameIndex].localGameAlias;
                        }
                        else
                        {
                            CoinModeLogging.LogError("CoinModeSettings", "GetChallengeGameId", "Challenge game not found for {0}!", localGameAlias);
                        }
                    }
                    else
                    {
                        return instance._games[i].localGameAlias;
                    }
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GetChallengeGameId", "The local game alias {0} could not be found in settings!", localGameAlias);
            return "";
        }

        public static string GetGameDeepLinkUrlScheme(string localGameAlias)
        {
            for (int i = 0; i < instance._games.Count; i++)
            {
                if (instance._games[i].localGameAlias == localGameAlias)
                {
                    if(instance._games[i].linkedGameIndex >= 0 && instance._games[i].linkedGameIndex < instance._games.Count)
                    {
                        // Get deep link scheme from game linked to as this is the source scheme
                        if(instance._games[instance._games[i].linkedGameIndex].deeplinkSchemeIndex >= 0 &&
                            instance._games[instance._games[i].linkedGameIndex].deeplinkSchemeIndex < deeplinkUrlSchemes.Count)
                        {
                            return deeplinkUrlSchemes[instance._games[instance._games[i].linkedGameIndex].deeplinkSchemeIndex];
                        }
                    }
                    else if(instance._games[i].supportsInvites)
                    {
                        if (instance._games[i].deeplinkSchemeIndex >= 0 &&
                            instance._games[i].deeplinkSchemeIndex < deeplinkUrlSchemes.Count)
                        {
                            return deeplinkUrlSchemes[instance._games[i].deeplinkSchemeIndex];
                        }
                    }
                    CoinModeLogging.LogError("CoinModeSettings", "GetChallengeGameDeepLinkUrlScheme",
                            "No deep link url scheme has been set for {0}!", localGameAlias);
                    return "";
                }
            }
            CoinModeLogging.LogError("CoinModeSettings", "GetChallengeGameDeepLinkUrlScheme", "The local game alias {0} could not be found in settings!", localGameAlias);
            return "";
        }
#endif

        internal static void InitSettings()
        {
            CoinModeSettings settingsObject = Resources.Load("CoinModeSettings", typeof(CoinModeSettings)) as CoinModeSettings;
            if (settingsObject == null)
            {
                settingsObject = ScriptableObject.CreateInstance<CoinModeSettings>();
#if UNITY_EDITOR
                if (!Directory.Exists(Application.dataPath + "/CoinMode/Settings/Resources"))
                {
                    Directory.CreateDirectory(Application.dataPath + "/CoinMode/Settings/Resources");
                }
                AssetDatabase.CreateAsset(settingsObject, "Assets/CoinMode/Settings/Resources/CoinModeSettings.asset");
#if UNITY_2020_3_OR_NEWER
                AssetDatabase.SaveAssetIfDirty(settingsObject);
#else
                EditorUtility.SetDirty(settingsObject);
                AssetDatabase.SaveAssets();
#endif
                EditorUtility.FocusProjectWindow();
#endif
            }
            _instance = settingsObject;
        }

        [SerializeField]
        [HideInInspector]
        private string _productionTitleId = "";

        [SerializeField]
        [HideInInspector]
        private string _stagingTitleId = "";

        [SerializeField]
        [HideInInspector]
        private List<GameSetup> _games = new List<GameSetup>();

        [SerializeField]
        [HideInInspector]
        private int _defaultGameIdIndex = 0;

        [SerializeField]
        [HideInInspector]
        private bool _allowUserCreatedRoundsForMoney = true;

        [SerializeField]
        [HideInInspector]
        private CoinModeEnvironment _environment = CoinModeEnvironment.Staging;

        [SerializeField]
        [HideInInspector]
        private Vector2 _referenceResolution = new Vector2(1024.0F, 768.0F);

        [SerializeField]
        [HideInInspector]
        private CoinModeLogging.LoggingMode _loggingMode = CoinModeLogging.LoggingMode.LogAll;

        [SerializeField]
        [HideInInspector]
        private CoinModeMenu.MessageDisplayMode _menuMessageDisplayMode = CoinModeMenu.MessageDisplayMode.All;

        [SerializeField]
        [HideInInspector]
        private Analytics.Settings _analyticsSettings = new Analytics.Settings();

#if UNITY_2019_3_OR_NEWER
        [SerializeField]
        [HideInInspector]
        private List<string> _deeplinkUrlSchemes = new List<string>();
#endif

#if UNITY_EDITOR
        internal void EditorSaveObject(bool force = false)
        {
#if UNITY_2020_3_OR_NEWER
            if (force) EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
#else
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        internal void EditorSetProductionTitleID(string titleId)
        {
            _productionTitleId = titleId;
        }

        internal string EditorGetProductionTitleID()
        {
            return _productionTitleId;
        }

        internal void EditorSetStagingTitleID(string titleId)
        {
            _stagingTitleId = titleId;
        }

        internal string EditorGetStagingTitleID()
        {
            return _stagingTitleId;
        }

        internal int GetGameSetupIndex(string localAlias)
        {
            for (int i = 0; i < _games.Count; i++)
            {
                if (_games[i].localGameAlias == localAlias)
                {
                    return i;
                }
            }
            return -1;
        }

        internal void EditorSetGameSetup(GameSetup setup, int index)
        {
            if(index >= 0 && index < _games.Count)
            {
                _games[index] = setup;
#if UNITY_2019_3_OR_NEWER
                if(setup.supportsPvpChallenge && setup.pvpChallengeGameIndex >= 0 && setup.pvpChallengeGameIndex < _games.Count)
                {
                    GameSetup pvpChallengeSetup = _games[setup.pvpChallengeGameIndex];
                    pvpChallengeSetup.linkedGameIndex = index;
                    _games[setup.pvpChallengeGameIndex] = pvpChallengeSetup;
                }
#endif
            }
        }

#if UNITY_2019_3_OR_NEWER
        internal void EditorClearUnlinkedPvpChallengeGames()
        {
            for(int i = 0; i < _games.Count; i++)
            {
                if(_games[i].linkedGameIndex >= 0)
                {
                    bool clear = true;
                    if(_games[i].linkedGameIndex < _games.Count && _games[_games[i].linkedGameIndex].supportsPvpChallenge && 
                        _games[_games[i].linkedGameIndex].useDifferentGame && _games[_games[i].linkedGameIndex].pvpChallengeGameIndex == i)
                    {
                        clear = false;
                    }

                    if(clear)
                    {
                        GameSetup setup = _games[i];
                        setup.linkedGameIndex = -1;
                        _games[i] = setup;
                    }
                }
            }
        }
#endif

        internal void EditorAddGameSetup()
        {
            _games.Add(new GameSetup());
        }

        internal void EditorRemoveGameSetup()
        {
            if(_games.Count > 0)
            {
                _games.RemoveAt(_games.Count - 1);
                if(_defaultGameIdIndex >= _games.Count)
                {
                    _defaultGameIdIndex--;
                }
            }            
        }

        internal void EditorSetDefaultGameIdIndex(int index)
        {
            _defaultGameIdIndex = index;
        }

        internal int EditorGetDefaultGameIdIndex()
        {
            return _defaultGameIdIndex;
        }

        internal void EditorSetAllowUserCreatedRoundsForMoney(bool allow)
        {
            _allowUserCreatedRoundsForMoney = allow;
        }

        internal void EditorSetEnvironment(CoinModeEnvironment environment)
        {
            _environment = environment;
        }

        internal void EditorSetReferenceResolution(Vector2 refResolution)
        {
            _referenceResolution = refResolution;
        }

        internal void EditorSetLoggingMode(CoinModeLogging.LoggingMode mode)
        {
            _loggingMode = mode;
        }

        internal void EditorSetMenuMessageDisplayMode(CoinModeMenu.MessageDisplayMode mode)
        {
            _menuMessageDisplayMode = mode;
        }

#if UNITY_2019_3_OR_NEWER
        internal void EditorAddDeepLinkUrlScheme(string urlScheme)
        {
            if(!string.IsNullOrWhiteSpace(urlScheme))
            {
                _deeplinkUrlSchemes.Add(urlScheme);
                CoinModeDeepLinkSetup.AddDeepLinkUrlScheme(CoinModeDeepLinkSetup.DeepLinkPlatform.Android, urlScheme);
                CoinModeDeepLinkSetup.AddDeepLinkUrlScheme(CoinModeDeepLinkSetup.DeepLinkPlatform.iOS, urlScheme);
            }            
        }

        internal void EditorRemoveDeepLinkUrlScheme(string urlScheme)
        {
            if(!string.IsNullOrWhiteSpace(urlScheme) && _deeplinkUrlSchemes.Contains(urlScheme))
            {
                _deeplinkUrlSchemes.Remove(urlScheme);
                CoinModeDeepLinkSetup.RemoveDeepLinkUrlScheme(CoinModeDeepLinkSetup.DeepLinkPlatform.Android, urlScheme);
                CoinModeDeepLinkSetup.RemoveDeepLinkUrlScheme(CoinModeDeepLinkSetup.DeepLinkPlatform.iOS, urlScheme);
            }
        }
#endif
#endif
    }
}

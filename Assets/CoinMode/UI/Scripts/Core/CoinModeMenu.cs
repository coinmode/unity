using CoinMode.UI;
using CoinMode.NetApi;
using LightJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CoinMode
{
    public delegate void LoginEvent(PlayerComponent player);
    public delegate void LoginFailureEvent(PlayerComponent player, CoinModeError error);

    public delegate void LogoutEvent();

    public delegate void PlayGameEvent(SessionComponent session);
    public delegate void PlayGameFailureEvent(SessionComponent session, CoinModeError error);

    public delegate void PlayGameServerEvent(SessionComponent session, string ip, string port);
    public delegate void PlayGameServerFailureEvent(SessionComponent session, string ip, string port, CoinModeError error);

    [AddComponentMenu("CoinMode/CM Menu")]
    public class CoinModeMenu : CoinModeScreenController
    {
        public enum MessageDisplayMode
        {
            All = 0,
            SuccessAndError = 1,
            ErrorOnly = 2,
        }

        public enum MessageType
        {
            Message = 0,
            Success = 1,
            Error = 2,
        }

        public enum State
        {
            Clean,
            Ready,
            InUse,
        }

        public enum GameState
        {
            Unknown,
            WaitingForLogin,
            MenuOpen,
            WaitingForPlay,
            Playing
        }

        private struct Depenedcies
        {
            public PlayerComponent player { get; private set; }

            public GameComponent currentGame { get; private set; }
            public RoundComponent currentRound { get; private set; }
            public SessionComponent currentSession { get; private set; }

            public Depenedcies(PlayerComponent player)
            {
                this.player = player;
                currentGame = null;
                currentRound = null;
                currentSession = null;
            }

            public void Clear()
            {
                currentGame = null;
                currentRound = null;
                currentSession = null;
            }

            public bool UpdateGame(GameComponent game)
            {
                if (game != null)
                {
                    currentGame = game;
                    currentRound = null;
                    currentSession = null;
                    return true;
                }
                CoinModeLogging.LogWarning("CoinModeMenu.Dependencies", "UpdateGame", "Cannot set game dependency, game is null");
                return false;
            }

            public bool UpdateRound(RoundComponent round)
            {
                if (round != null)
                {
                    currentGame = round.game;
                    currentRound = round;
                    currentSession = null;
                    return true;
                }
                CoinModeLogging.LogWarning("CoinModeMenu.Dependencies", "UpdateGame", "Cannot set round dependency, round is null");
                return false;
            }

            public bool UpdateSession(SessionComponent session)
            {
                if (session != null)
                {
                    currentGame = session.round.game;
                    currentRound = session.round;
                    currentSession = session;
                    return true;
                }
                CoinModeLogging.LogWarning("CoinModeMenu.Dependencies", "UpdateGame", "Cannot set session dependency, session is null");
                return false;
            }
        }

        private static CoinModeMenu instance
        {
            get
            {
                if (_instance == null)
                {
                    UnityEngine.Object menuPrefab = Resources.Load("CoinModeMenu", typeof(CoinModeMenu));
                    if (menuPrefab != null)
                    {
                        GameObject canvasGo = new GameObject("CoinModeMenuCanvas");
                        DontDestroyOnLoad(canvasGo);

                        Canvas canvas = canvasGo.AddComponent<Canvas>();
                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        canvas.sortingOrder = 101;
                        CanvasScaler canvasScaler = canvasGo.AddComponent<CanvasScaler>();
                        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        canvasScaler.referenceResolution = CoinModeSettings.referenceResolution;
                        canvasScaler.matchWidthOrHeight = 0.5F;
                        canvasScaler.referencePixelsPerUnit = 100.0F;
                        canvasGo.AddComponent<GraphicRaycaster>();

                        GameObject menuGo = Instantiate(menuPrefab, canvasGo.transform) as GameObject;
                        if (menuGo != null)
                        {
                            menuGo.SetActive(true);
                        }
                    }
                }
                return _instance;
            }
        }
        private static CoinModeMenu _instance = null;

        [SerializeField]
        private Image menuBackgroundImage = null;

        [SerializeField]
        private RectTransform menuBackgroundPattern = null;

        [SerializeField]
        private Image menuBackgroundPatternImage = null;

        [SerializeField]
        private bool useBackgroundFade = true;

        [SerializeField]
        private PlayerMenuButton playerMenuButton = null;

        [SerializeField]
        private LoadingSpinnerCircle loadingObject = null;

        [SerializeField]
        private DatePicker datePickerTemplate = null;

        [SerializeField]
        private CoinModeModalWindow modalWindowTemplate = null;

        private PlayerComponent player { get { return dependencies.player; } }
        private GameComponent currentGame { get { return dependencies.currentGame; } }
        private RoundComponent currentRound { get { return dependencies.currentRound; } }
        private SessionComponent currentSession { get { return dependencies.currentSession; } }

        private Depenedcies dependencies = new Depenedcies(null);
        private bool startSessionImmediately = false;
        private bool displayResults = false;
        private JsonObject challengeCustomJson = null;

#if UNITY_2019_3_OR_NEWER
        private RoundInviteInfo inviteInfo = new RoundInviteInfo();
#endif

        private LoginEvent loginSuccess = null;
        private LoginFailureEvent loginFailure = null;
        private LogoutEvent onLogout = null;

        private PlayGameEvent playGameSuccess = null;
        private PlayGameFailureEvent playGameFailure = null;

#if UNITY_2019_3_OR_NEWER
        private PlayGameFailureEvent challengeAFriendFailure = null;
#endif

        private CoinModeEvent setCurrentRoundSuccess = null;
        private RoundEvent getCurrentRoundInfoSuccess = null;

        private CanvasGroup fadeCanvas = null;

        public new State state { get { return _menuState; } private set { _menuState = value; } }
        private State _menuState = State.Clean;

        private CoinModeModalWindow modalWindow
        {
            get
            {
                if (_modalWindow == null)
                {
                    if (modalWindowTemplate != null)
                    {
                        _modalWindow = Instantiate(modalWindowTemplate, transform.parent);
                        _modalWindow.transform.SetAsLastSibling();
                        _modalWindow.gameObject.SetActive(true);
                        _modalWindow.CloseImmediately();
                    }
                }
                return _modalWindow;
            }
        }
        private static CoinModeModalWindow _modalWindow = null;

        private DatePicker datePicker
        {
            get
            {
                if (_datePicker == null)
                {
                    if (datePickerTemplate != null)
                    {
                        _datePicker = Instantiate(datePickerTemplate, transform.parent);
                        _datePicker.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);

                        RectTransform menuTransform = _datePicker.transform as RectTransform;
                        if (menuTransform != null)
                        {
                            menuTransform.anchorMin = new Vector2(0.0F, 0.0F);
                            menuTransform.anchorMax = new Vector2(1.0F, 1.0F);
                            menuTransform.offsetMin = new Vector2(0.0F, 0.0F);
                            menuTransform.offsetMax = new Vector2(0.0F, 0.0F);
                        }

                        _datePicker.gameObject.SetActive(false);
                    }
                }
                return _datePicker;
            }
        }
        private DatePicker _datePicker;

        protected override Type defaultScreen { get { return null; } }
        protected override object defaultData { get { return null; } }
        public override bool requiresData { get; } = false;

        private void Initialize()
        {
            if (menuBackgroundImage != null)
            {
                menuBackgroundImage.color = CoinModeMenuStyle.backgroundColor;
                if (CoinModeMenuStyle.backgroundSprite != null)
                {
                    menuBackgroundImage.sprite = CoinModeMenuStyle.backgroundSprite;
                }
            }

            if (menuBackgroundPattern != null && menuBackgroundPatternImage != null)
            {
                menuBackgroundPatternImage.sprite = CoinModeMenuStyle.backgroundPatternSprite;
                menuBackgroundPattern.gameObject.SetActive(menuBackgroundPatternImage.sprite != null);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Initialize();
        }
#endif

        private void Update()
        {
            if (gameObject.activeInHierarchy)
            {
                UpdateUI(Time.unscaledDeltaTime);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            Initialize();

            if (_instance == null)
            {
                _instance = this;
                state = State.Ready;

                SceneManager.activeSceneChanged += OnSceneChanged;
            }
            else if (_instance != this)
            {
                CoinModeLogging.LogWarning("CoinModeMenu", "Awake",
                    "A coin mode menu was destroyed because there was more than one instance.");
                Destroy(this);
            }
        }

        protected override void Start()
        {
            base.Start();

            if (useBackgroundFade)
            {
                GameObject fadeGo = new GameObject("BackgroundFade");
                fadeGo.transform.parent = transform.parent;
                fadeGo.transform.SetSiblingIndex(transform.GetSiblingIndex());
                fadeGo.transform.localScale = Vector3.one;
                Image image = fadeGo.AddComponent<Image>();
                image.color = new Color(0.0F, 0.0F, 0.0F, 0.66F);
                fadeCanvas = fadeGo.AddComponent<CanvasGroup>();
                fadeCanvas.alpha = canvasGroup.alpha;
                fadeCanvas.blocksRaycasts = false;
                fadeCanvas.interactable = false;
                RectTransform fadeTransform = fadeGo.transform as RectTransform;
                if (fadeTransform != null)
                {
                    fadeTransform.anchorMin = new Vector2(0.0F, 0.0F);
                    fadeTransform.anchorMax = new Vector2(1.0F, 1.0F);
                    fadeTransform.offsetMin = new Vector2(0.0F, 0.0F);
                    fadeTransform.offsetMax = new Vector2(0.0F, 0.0F);
                }
            }

            if (playerMenuButton != null)
            {
                playerMenuButton.onClick.AddListener(PlayerButtonAction);
            }

            HidePlayerMenuButton();
            HideLoading();
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if (fadeCanvas != null && fadeCanvas.alpha != canvasGroup.alpha)
            {
                fadeCanvas.alpha = canvasGroup.alpha;
                fadeCanvas.blocksRaycasts = canvasGroup.blocksRaycasts;
            }
        }

        private void OnApplicationQuit()
        {
            if (player != null && player.state >= PlayerComponent.PlayerState.Ready)
            {
                LeaveTitleLobby();
            }

            GameState gameState = GameState.Unknown;
            string stateInfo = "";
            if (isOpen)
            {
                gameState = GameState.MenuOpen;
                string[] screenPath = activeScreen.ToString().Split('.');
                stateInfo = screenPath[screenPath.Length - 1];
            }
            else if (player == null || player.state != PlayerComponent.PlayerState.Ready)
            {
                gameState = GameState.WaitingForLogin;
            }
            else if (currentSession != null)
            {
                gameState = GameState.Playing;
                stateInfo = currentSession.state.ToString();
            }
            else
            {
                gameState = GameState.WaitingForPlay;
            }
            string finalState = string.IsNullOrWhiteSpace(stateInfo) ? gameState.ToString() :
                string.Format("{0} - {1}", gameState.ToString(), stateInfo);
            Analytics.RecordQuit(finalState);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            //ResetMenuColor();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        protected override void OnOpen(object data)
        {
            if (state != State.InUse)
            {
                state = State.InUse;
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return false;
        }

        protected override void OnClose()
        {
            base.OnClose();
            //ResetMenuColor();
            if (state != State.Ready)
            {
                state = State.Ready;
            }
        }

        private void OnSceneChanged(Scene current, Scene next)
        {
            if (isOpen)
            {
                Close();
            }
            if (_modalWindow != null && _modalWindow.isOpen)
            {
                _modalWindow.Close();
            }
        }

        // Public Static Functionality
        public static bool OpenLogin(LoginEvent onSuccess, LoginFailureEvent onFailure, LogoutEvent onLogout)
        {
            return instance.OpenLogin_Internal(onSuccess, onFailure, onLogout);
        }

        public static bool OpenPlayGame(bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            return instance.OpenPlayGame_Internal(CoinModeSettings.defaultGameAlias, null, startSessionImmediately, onSuccess, onFailure);
        }

        public static bool OpenPlayGame(string localGameAlias, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            return instance.OpenPlayGame_Internal(localGameAlias, null, startSessionImmediately, onSuccess, onFailure);
        }

        public static bool OpenPlayGame(RoundComponent round, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            return instance.OpenPlayGame_Internal("", round, startSessionImmediately, onSuccess, onFailure);
        }

        public static bool OpenRoundList(bool startSessionOnPlay, PlayGameEvent onPlaySuccess, PlayGameFailureEvent onPlayFailure)
        {
            return instance.OpenRoundList_Internal(CoinModeSettings.defaultGameAlias, null, startSessionOnPlay, onPlaySuccess, onPlayFailure);
        }

        public static bool OpenRoundList(string localGameAlias, bool startSessionOnPlay, PlayGameEvent onPlaySuccess, PlayGameFailureEvent onPlayFailure)
        {
            return instance.OpenRoundList_Internal(localGameAlias, null, startSessionOnPlay, onPlaySuccess, onPlayFailure);
        }

        public static bool OpenRoundList(GameComponent game, bool startSessionOnPlay, PlayGameEvent onPlaySuccess, PlayGameFailureEvent onPlayFailure)
        {
            return instance.OpenRoundList_Internal("", game, startSessionOnPlay, onPlaySuccess, onPlayFailure);
        }

        public static bool OpenHighScores()
        {
            return instance.OpenHighScores_Internal(CoinModeSettings.defaultGameAlias, null);
        }

        public static bool OpenHighScores(string localGameAlias)
        {
            return instance.OpenHighScores_Internal(localGameAlias, null);
        }

        public static bool OpenHighScores(RoundComponent round)
        {
            return instance.OpenHighScores_Internal("", round);
        }

        public static bool OpenCreateRound(bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure, ParameterCollection roundParams = null,
            RoundSetupScreen.ScreenConfig screenConfig = null)
        {
            return instance.OpenCreateRound_Internal(CoinModeSettings.defaultGameAlias, null, null, startSessionImmediately, onSuccess, onFailure, roundParams, screenConfig);
        }

        /// <summary>
        /// Opens the create round dialog
        /// </summary>
        /// <param name="gameId">The id of the game to create a round for</param>
        /// <param name="onSuccess">Delegate response if the operation is successful, signature is: </param>
        /// <param name="onFailure">Delegate response if the operation fails or the user closes the dialog before completion</param>
        /// <returns></returns>
        public static bool OpenCreateRound(string localGameAlias, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure,
            ParameterCollection roundParams = null, RoundSetupScreen.ScreenConfig screenConfig = null)
        {
            return instance.OpenCreateRound_Internal(localGameAlias, null, null, startSessionImmediately, onSuccess, onFailure, roundParams, screenConfig);
        }

        /// <summary>
        /// Opens the create round dialog with optional custom json object
        /// </summary>
        /// <param name="gameId">The id of the game to create a round for</param>
        /// <param name="customJson">JsonObject used to associate extra data with the round to be returned when rounds/get_info is called</param>
        /// <param name="onSuccess">Delegate response if the operation is successful</param>
        /// <param name="onFailure">Delegate response if the operation fails or the user closes the dialog before completion</param>
        /// <returns></returns>
        public static bool OpenCreateRound(string localGameAlias, bool startSessionImmediately, JsonObject customJson, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure,
            ParameterCollection roundParams = null, RoundSetupScreen.ScreenConfig screenConfig = null)
        {
            return instance.OpenCreateRound_Internal(localGameAlias, null, customJson, startSessionImmediately, onSuccess, onFailure, roundParams, screenConfig);
        }

        /// <summary>
        /// Opens the create round dialog for games that utilise "url_create_game_server"
        /// </summary>
        /// <param name="gameId">The id of the game to create a round for</param>
        /// <param name="gameServerParams">String of command line arguments that are passed to the api when running the games server executable</param>
        /// <param name="onSuccess">Delegate response if the operation is successful</param>
        /// <param name="onFailure">Delegate response if the operation fails or the user closes the dialog before completion</param>
        /// <returns></returns>
        public static bool OpenCreateRound(string localGameAlias, string gameServerParams, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure,
            ParameterCollection roundParams = null, RoundSetupScreen.ScreenConfig screenConfig = null)
        {
            return instance.OpenCreateRound_Internal(localGameAlias, gameServerParams, null, false, onSuccess, onFailure, roundParams, screenConfig);
        }

        /// <summary>
        /// Opens the create round dialog for games that utilise "url_create_game_server", allows for custom json data to be associated with the round also
        /// </summary>
        /// <param name="gameId">The id of the game to create a round for</param>
        /// <param name="gameServerParams">String of command line arguments that are passed to the api when running the games server executable</param>
        /// <param name="customJson">JsonObject used to associate extra data with the round to be returned when rounds/get_info is called</param>
        /// <param name="onSuccess">Delegate response if the operation is successful</param>
        /// <param name="onFailure">Delegate response if the operation fails or the user closes the dialog before completion</param>
        /// <returns></returns>
        public static bool OpenCreateRound(string localGameAlias, string gameServerParams, JsonObject customJson, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure,
            ParameterCollection roundParams = null, RoundSetupScreen.ScreenConfig screenConfig = null)
        {
            return instance.OpenCreateRound_Internal(localGameAlias, gameServerParams, customJson, false, onSuccess, onFailure, roundParams, screenConfig);
        }

#if UNITY_2019_3_OR_NEWER
        public static bool OpenCreatePvpChallenge(bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            return instance.OpenCreatePvpChallenge_Internal(startSessionImmediately, CoinModeSettings.defaultGameAlias, null, null, onSuccess, onFailure);
        }

        /// <summary>
        /// Opens the challenge a friend dialog, this implementation is to be used when the challenge session is yet to be started
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public static bool OpenCreatePvpChallenge(bool startSessionImmediately, string localGameAlias, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            return instance.OpenCreatePvpChallenge_Internal(startSessionImmediately, localGameAlias, null, null, onSuccess, onFailure);
        }

        /// <summary>
        /// Opens the challenge a friend dialog, this implementation requires an existing session and is to be used when a challenge is to be created
        /// after an existing session has been stopped
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="existingSession"></param>
        /// <param name="customRoundData"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public static bool OpenCreatePvpChallenge(string localGameAlias, SessionComponent existingSession, JsonObject customRoundData, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            if (existingSession == null)
            {
                CoinModeLogging.LogWarning("CoinModeMenu", "OpenChallengeAFriend", "Cannot challenge a friend to an existing session when session is null");
                return false;
            }
            return instance.OpenCreatePvpChallenge_Internal(true, localGameAlias, existingSession, customRoundData, onSuccess, onFailure);
        }

        public static bool OpenAcceptRoundInvite(RoundComponent round, RoundInviteInfo inviteInfo, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            return instance.OpenAcceptRoundInvite_Internal(round, inviteInfo, startSessionImmediately, onSuccess, onFailure);
        }
#endif

        public static bool SubmitSessionScore(SessionComponent session, bool displayResults)
        {
            return instance.SubmitSessionScore_Internal(session, displayResults, null);
        }

        public static bool SubmitSessionScore(SessionComponent session, JsonObject challengeCustomJson)
        {
            return instance.SubmitSessionScore_Internal(session, true, challengeCustomJson);
        }

        public static bool OpenRoundResults(RoundComponent round)
        {
            return instance.OpenRoundResults_Internal(round);
        }

        public static bool OpenShareInstallLink()
        {
            string title = "Invite to install " + Application.productName;
            return instance.OpenShareLink_Internal(title, title, CoinModeManager.GetInstallUrl());
        }

        public static bool OpenShareScreen(string screenTitle, string shareSheetTitle, string shareContent)
        {
            return instance.OpenShareLink_Internal(screenTitle, shareSheetTitle, shareContent);
        }

        public static bool OpenSendFunds(UserComponent targetUser)
        {
            return instance.OpenSendFunds_Internal(targetUser);
        }

        public static bool OpenPlayerMenu()
        {
            return instance.OpenPlayerMenu_Internal();
        }

        public static bool OpenUserInfo(string publicId)
        {
            return instance.OpenUserInfo_Internal(publicId);
        }

        public static bool OpenLobbyPlayers()
        {
            if (!instance.isOpen || instance.state == State.Ready)
            {
                NearbyPlayersScreen.ScreenData data = new NearbyPlayersScreen.ScreenData(instance.player, LocationType.GameLobby, CoinModeManager.titleComponent.titleId,
                    null, null, null, null);
                return instance.OpenToScreen<NearbyPlayersScreen>(data);
            }
            return false;
        }

        public static void OpenDatePicker(DateTime date, DateSelected onDateSelected, bool allowFuture = true, int? minYear = null, int? maxYear = null)
        {
            instance.datePicker.onDateSelected = onDateSelected;
            instance.datePicker.gameObject.SetActive(true);
            instance.datePicker.Init(allowFuture, minYear, maxYear);
            instance.datePicker.SetSelectedDate(date);
        }

        public static void CloseDatePicker()
        {
            instance.datePicker.onDateSelected = null;
            instance.datePicker.gameObject.SetActive(false);
        }

        public static bool IsOpen()
        {
            return instance.isOpen;
        }

        // Private functionality
        private bool OpenLogin_Internal(LoginEvent onSuccess, LoginFailureEvent onFailure, LogoutEvent onLogout)
        {
            if (!isOpen || state == State.Ready)
            {
                loginSuccess = onSuccess;
                loginFailure = onFailure;
                this.onLogout = onLogout;

                PlayerComponent player;
                CoinModeManager.ConstructPlayer(out player);
                dependencies = new Depenedcies(player);
                if (playerMenuButton != null)
                {
                    playerMenuButton.AssignPlayer(player);
                }

                if (!string.IsNullOrEmpty(CoinModePlayerPrefs.GetLastLoggedInPlayer()))
                {
                    return OpenToScreen<ContinueScreen>(player);
                }
                else
                {
                    return OpenToScreen<LoginScreen>(player);
                }
            }
            return false;
        }

        private bool OpenPlayGame_Internal(string localGameAlias, RoundComponent round, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                GameComponent game;
                if (round != null)
                {
                    dependencies.UpdateRound(round);
                }
                else
                {
                    if (CoinModeManager.titleComponent.TryGetGame(localGameAlias, out game))
                    {
                        dependencies.UpdateGame(game);
                    }
                    else
                    {
                        CoinModeLogging.LogWarning("CoinModeMenu", "OpenPlayGameDialog", "Cannot open play game dialog, game {0} not found in title {1}",
                            localGameAlias, CoinModeManager.titleComponent.titleId);
                        return false;
                    }
                }

                this.startSessionImmediately = startSessionImmediately;
                playGameSuccess = onSuccess;
                playGameFailure = onFailure;

                if (currentRound != null)
                {
                    return GetCurrentRoundInfo(OnPlayGameGetRoundInfoSuccess);
                }
                else
                {
                    return SetCurrentRoundFromList(delegate ()
                    {
                        GetCurrentRoundInfo(OnPlayGameGetRoundInfoSuccess);
                    });
                }
            }
            return false;
        }

        private bool OpenRoundList_Internal(string localGameAlias, GameComponent game, bool startSessionOnPlay, PlayGameEvent onPlaySuccess, PlayGameFailureEvent onPlayFailure)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                if (game != null)
                {
                    dependencies.UpdateGame(game);
                }
                else
                {
                    if (CoinModeManager.titleComponent.TryGetGame(localGameAlias, out game))
                    {
                        dependencies.UpdateGame(game);
                    }
                    else
                    {
                        CoinModeLogging.LogWarning("CoinModeMenu", "OpenRoundList_Internal", "Cannot open round list, game {0} not found in title {1}",
                            localGameAlias, CoinModeManager.titleComponent.titleId);
                        return false;
                    }
                }

                startSessionImmediately = startSessionOnPlay;
                playGameSuccess = onPlaySuccess;
                playGameFailure = onPlayFailure;

                if (currentGame != null)
                {
                    RoundListScreen.ScreenData screenData = new RoundListScreen.ScreenData(player, currentGame);
                    OpenToScreen<RoundListScreen>(screenData);
                }
            }
            return false;
        }

        private bool OpenHighScores_Internal(string localGameAlias, RoundComponent round)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                if (round != null)
                {
                    dependencies.UpdateRound(round);
                    HighScoreScreenData data = new HighScoreScreenData(player, currentGame, currentRound, null, null);
                    return instance.OpenToScreen<HighScoreScreen>(data);
                }
                else
                {
                    GameComponent game;
                    if (CoinModeManager.titleComponent.TryGetGame(localGameAlias, out game))
                    {
                        dependencies.UpdateGame(game);
                        return SetCurrentRoundFromList(delegate ()
                        {
                            HighScoreScreenData data = new HighScoreScreenData(player, currentGame, currentRound, null, null);
                            instance.SwitchScreen<HighScoreScreen>(data);
                        });
                    }
                    else
                    {
                        CoinModeLogging.LogWarning("CoinModeMenu", "OpenHighScores", "Cannot open high scores dialog, game {0} not found in title {1}",
                        localGameAlias, CoinModeManager.titleComponent.titleId);
                        return false;
                    }
                }
            }
            return false;
        }

        private bool OpenCreateRound_Internal(string localGameAlias, string gameServerParams, JsonObject customJson, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure,
            ParameterCollection roundParams, RoundSetupScreen.ScreenConfig screenConfig)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                playGameSuccess = onSuccess;
                playGameFailure = onFailure;

                GameComponent game;
                if (CoinModeManager.titleComponent.TryGetGame(localGameAlias, out game))
                {
                    dependencies.UpdateGame(game);
                    RoundSetupScreen.ScreenData data = new RoundSetupScreen.ScreenData(instance.player, instance.currentGame, startSessionImmediately, customJson, 
                        gameServerParams, roundParams, screenConfig);
                    return instance.OpenToScreen<RoundSetupScreen>(data);
                }
                else
                {
                    CoinModeLogging.LogWarning("CoinModeMenu", "OpenCreateRound", "Cannot open create round for game {0}, not found in title {1}",
                        localGameAlias, CoinModeManager.titleComponent.titleId);
                }
            }
            return false;
        }

#if UNITY_2019_3_OR_NEWER
        private bool OpenCreatePvpChallenge_Internal(bool startSessionImmediately, string localGameAlias, SessionComponent existingSession, JsonObject customRoundData, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                if (existingSession != null && existingSession.state != SessionComponent.SessionState.Stopped)
                {
                    CoinModeLogging.LogWarning("CoinModeMenu", "OpenChallengeAFriend", "Cannot challenge a friend to an existing session when session is {0}",
                        existingSession.state.ToString());
                    return false;
                }

                GameComponent game;
                if (CoinModeManager.titleComponent.TryGetGame(localGameAlias, out game))
                {
                    dependencies.UpdateGame(game);
                }
                else
                {
                    CoinModeLogging.LogWarning("CoinModeMenu", "OpenChallengeAFriend", "Cannot challenge a friend, game {0} not found in title {1}",
                        localGameAlias, CoinModeManager.titleComponent.titleId);
                    return false;
                }

                playGameSuccess = onSuccess;
                playGameFailure = onFailure;

                RoundSetupScreen.ScreenData data = new RoundSetupScreen.ScreenData(instance.player, instance.currentGame, startSessionImmediately, customRoundData, null,
                    existingSession, null, RoundSetupScreen.ScreenConfig.defaultChallengeConfig);
                return instance.OpenToScreen<RoundSetupScreen>(data);
            }
            return false;
        }

        private bool OpenAcceptRoundInvite_Internal(RoundComponent round, RoundInviteInfo inviteInfo, bool startSessionImmediately, PlayGameEvent onSuccess, PlayGameFailureEvent onFailure)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                dependencies.UpdateRound(round);

                this.inviteInfo = inviteInfo;
                this.startSessionImmediately = startSessionImmediately;
                playGameSuccess = onSuccess;
                playGameFailure = onFailure;

                return GetCurrentRoundInfo(OnGetInvitedRoundInfoSuccess);
            }
            return false;
        }
#endif

        private bool SubmitSessionScore_Internal(SessionComponent session, bool displayResults, JsonObject challengeCustomJson)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                if (session.state != SessionComponent.SessionState.Stopped)
                {
                    if (session.state == SessionComponent.SessionState.Started)
                    {
                        dependencies.UpdateSession(session);

                        this.displayResults = displayResults;
                        this.challengeCustomJson = challengeCustomJson;
                        return OpenLoadingScreen(session.Stop(OnSessionStopSuccess, OnSessionStopFailure), RetryStopSession, "Submitting score!", "Failed to submit score!");
                    }
                }
            }
            CoinModeLogging.LogWarning("CoinModeMenu", "SubmitScore", "Cannot submit score for session {0}, while session is {1}", session.sessionId, session.state);
            return false;
        }

        private bool OpenRoundResults_Internal(RoundComponent round)
        {
            if (!isOpen || state == State.Ready)
            {
                if (!CheckPlayerAndPlayTokenAreValid())
                {
                    return false;
                }

                dependencies.UpdateRound(round);
                HighScoreScreenConfig config = new HighScoreScreenConfig(true, false);
                HighScoreScreenData data = new HighScoreScreenData(instance.player, instance.currentGame, instance.currentRound, null, config);
                instance.OpenToScreen<ResultsScreen>(data);
            }
            return false;
        }

        private bool OpenShareLink_Internal(string screenTitle, string shareSheetTitle, string shareContent)
        {
            if (!isOpen || state == State.Ready)
            {
                ShareScreen.ScreenData data = new ShareScreen.ScreenData(screenTitle, shareSheetTitle, shareContent);
                return OpenToScreen<ShareScreen>(data);
            }
            return false;
        }

        private bool OpenSendFunds_Internal(UserComponent targetUser)
        {
            if (state != State.Clean)
            {
                string currentWalletId = instance.currentGame != null ? instance.currentGame.walletId : instance.player.defaultWalletId;
                SetupTransferScreen.ScreenData data = new SetupTransferScreen.ScreenData(instance.player, targetUser, currentWalletId);
                return isOpen ? SwitchScreen<SetupTransferScreen>(data) : OpenToScreen<SetupTransferScreen>(data);
            }
            return false;
        }

        private bool OpenPlayerMenu_Internal()
        {
            if (state != State.Clean)
            {
                PlayerInfoScreen.ScreenData screenData = new PlayerInfoScreen.ScreenData(instance.player, instance.currentGame);
                return isOpen ? SwitchScreen<PlayerInfoScreen>(screenData) : OpenToScreen<PlayerInfoScreen>(screenData);
            }
            return false;
        }

        private bool OpenUserInfo_Internal(string publicId)
        {
            if (state != State.Clean)
            {
                UserInfoScreen.ScreenData screenData = new UserInfoScreen.ScreenData(instance.player, publicId);
                return isOpen ? SwitchScreen<UserInfoScreen>(screenData) : OpenToScreen<UserInfoScreen>(screenData);
            }
            return false;
        }

        private void PlayerButtonAction()
        {
            OpenPlayerMenu_Internal();
        }

        private void OpenModalWindow(CoinModeModalWindow.WindowData data)
        {
            if (modalWindow.state == WindowState.Open || modalWindow.state == WindowState.Opening)
            {
                modalWindow.UpdateData(data);
            }
            else
            {
                modalWindow.Open(data);
            }
        }

        // Internal Menu Responses
        // [Luke] TODO: Consider refactoring to pass these events as delegates to each UI screen instead of relying on internal functions        
        internal void PromptReLogin()
        {
            PlayerComponent player;
            CoinModeManager.ConstructPlayer(out player);
            dependencies = new Depenedcies(player);
            if (playerMenuButton != null)
            {
                playerMenuButton.AssignPlayer(player);
            }
            OpenToScreen<LoginScreen>(player);
        }

        internal void OnLoginSuccess()
        {
            ShowPlayerMenuButton();
            Close();
            loginSuccess?.Invoke(player);
            Analytics.RecordUserLogin(player.authMode);
        }

        internal void OnLoginFailure(CoinModeErrorBase.ErrorType errorType, string errorCode, string userMessage)
        {
            CoinModeError error = new CoinModeError(errorType, errorCode, userMessage);
            loginFailure?.Invoke(player, error);
        }

        internal void OnPlayGameSuccess()
        {
            Close();
            playGameSuccess?.Invoke(currentSession);
        }

        internal void OnPlayGameFailure(CoinModeErrorBase.ErrorType errorType, string errorCode, string userMessage)
        {
            CoinModeError error = new CoinModeError(errorType, errorCode, userMessage);
            playGameFailure?.Invoke(currentSession, error);
        }

        internal void PlayGameFromRoundList(RoundComponent round)
        {
            dependencies.UpdateRound(round);
            GetCurrentRoundInfo(OnPlayGameGetRoundInfoSuccess);
        }

        internal void HighScoresFromRoundList(RoundComponent round)
        {
            dependencies.UpdateRound(round);
            HighScoreScreenData data = new HighScoreScreenData(player, currentGame, currentRound, null, null);
            instance.OpenToScreen<HighScoreScreen>(data);
        }

        internal void OnCreateRoundSuccess(SessionComponent session)
        {
            dependencies.UpdateSession(session);
            Close();
            playGameSuccess?.Invoke(currentSession);
        }

        internal void OnCreateRoundFailure(CoinModeErrorBase.ErrorType errorType, string errorCode, string userMessage)
        {
            CoinModeError error = new CoinModeError(errorType, errorCode, userMessage);
            playGameFailure?.Invoke(currentSession, error);
        }

#if UNITY_2019_3_OR_NEWER
        internal void OpenCreatePvpChallengeFromHighScores(string localGameAlias, SessionComponent existingSession, JsonObject customRoundData, PlayGameFailureEvent onFailure)
        {
            GameComponent game;
            if (CoinModeManager.titleComponent.TryGetGame(localGameAlias, out game))
            {
                dependencies.UpdateGame(game);
            }
            else
            {
                CoinModeLogging.LogWarning("CoinModeMenu", "OpenChallengeFromHighScores", "Cannot challenge a friend, game {0} not found in title {1}",
                    localGameAlias, CoinModeManager.titleComponent.titleId);
                return;
            }

            challengeAFriendFailure = onFailure;

            RoundSetupScreen.ScreenData data = new RoundSetupScreen.ScreenData(instance.player, instance.currentGame, startSessionImmediately, customRoundData, null,
                existingSession, null, RoundSetupScreen.ScreenConfig.defaultChallengeConfig);
            SwitchScreen<RoundSetupScreen>(data);
        }

        internal void OnChallengeFromExistingSuccess(SessionComponent session)
        {
            ShareScreen.ScreenData data = new ShareScreen.ScreenData("Share Challenge", "Share Challenge", DeepLinkUtilities.CreateChallengeLink(session));
            SwitchScreen<ShareScreen>(data, true);
        }

        internal void OnChallengeFromExistingFailure(CoinModeErrorBase.ErrorType errorType, string errorCode, string userMessage)
        {
            CoinModeError error = new CoinModeError(errorType, errorCode, userMessage);
            challengeAFriendFailure?.Invoke(currentSession, error);
        }
#endif

        // Internal functionality
        internal void OpenModalWindow(string message, UnityAction retryAction, MessageType messageType = MessageType.Message)
        {
            List<CoinModeModalWindow.ModalWindowAction> actions = new List<CoinModeModalWindow.ModalWindowAction>();
            actions.Add(new CoinModeModalWindow.ModalWindowAction("Retry", retryAction));

            CoinModeModalWindow.WindowData data = new CoinModeModalWindow.WindowData(message, actions, GetModalWindowBackgroundColor(messageType));
            OpenModalWindow(data);
        }

        internal void OpenModalWindow(string message, List<CoinModeModalWindow.ModalWindowAction> actions, Color? backgroundColor = null)
        {
            CoinModeModalWindow.WindowData data = new CoinModeModalWindow.WindowData(message, actions, backgroundColor);
            OpenModalWindow(data);
        }

        internal void DisplayMessage(string message, MessageType messageType = MessageType.Message)
        {
            if ((int)CoinModeSettings.menuMessageDisplayMode <= (int)messageType)
            {
                Color? windowColor = null;
                if (messageType == MessageType.Success)
                {
                    windowColor = CoinModeMenuStyle.messageSuccessColor;
                }
                else if (messageType == MessageType.Error)
                {
                    windowColor = CoinModeMenuStyle.messageFailureColor;
                }
                CoinModeModalWindow.WindowData data = new CoinModeModalWindow.WindowData(message, null, windowColor);
                OpenModalWindow(data);
            }
        }

        internal Color GetModalWindowBackgroundColor(MessageType messageType)
        {
            switch (messageType)
            {
                default:
                case MessageType.Message:
                    return CoinModeMenuStyle.backgroundColor;
                case MessageType.Success:
                    return CoinModeMenuStyle.messageSuccessColor;
                case MessageType.Error:
                    return CoinModeMenuStyle.messageFailureColor;
            }
        }

        internal PlayerComponent ResetPlayer()
        {
            PlayerComponent player;
            CoinModeManager.ConstructPlayer(out player);
            dependencies = new Depenedcies(player);
            if (playerMenuButton != null)
            {
                playerMenuButton.AssignPlayer(player);
            }
            return player;
        }

        internal void ShowPlayerMenuButton()
        {
            if (playerMenuButton != null)
            {
                playerMenuButton.gameObject.SetActive(true);
                playerMenuButton.AssignPlayer(player);
            }
        }

        internal void HidePlayerMenuButton()
        {
            if (playerMenuButton != null) playerMenuButton.gameObject.SetActive(false);
        }

        internal void ShowLoading()
        {
            if (loadingObject != null)
            {
                loadingObject.gameObject.SetActive(true);
            }
        }

        internal void HideLoading()
        {
            if (loadingObject != null)
            {
                loadingObject.gameObject.SetActive(false);
            }
        }

        internal void Logout(UnityAction logoutConfirmed)
        {
            List<CoinModeModalWindow.ModalWindowAction> actions = new List<CoinModeModalWindow.ModalWindowAction>();
            UnityAction onConfirm = null;
            onConfirm += logoutConfirmed;
            onConfirm += LogoutConfirmed;
            actions.Add(new CoinModeModalWindow.ModalWindowAction("Yes", onConfirm));
            actions.Add(new CoinModeModalWindow.ModalWindowAction("No", null));
            CoinModeModalWindow.WindowData data = new CoinModeModalWindow.WindowData("Are you sure?", actions);
            modalWindow.Open(data);
        }

        internal void OpenWebsiteProfile()
        {
            string url = CoinModeManager.GetProfileUrl(player);
            Application.OpenURL(url);
        }

        internal void OpenWebsiteDepositFunds()
        {
            string url = CoinModeManager.GetDepositFundsUrl(player);
            Application.OpenURL(url);
        }

        // API 
        private bool SetCurrentRoundFromList(CoinModeEvent onSuccess)
        {
            setCurrentRoundSuccess = onSuccess;

            return OpenLoadingScreen(currentGame.ListRounds(player, true, true, false, OnListRoundsSuccess, OnListRoundsFailure),
                RetrySetCurrentRoundFromList, "Finding available round!", "Failed to list rounds!");
        }

        private void RetrySetCurrentRoundFromList()
        {
            UpdateLoadingScreen(currentGame.ListRounds(player, true, true, false, OnListRoundsSuccess, OnListRoundsFailure),
                "Finding available round!", "Failed to list rounds!");
        }

        private void OnListRoundsSuccess(GameComponent game, MinimalRoundInfo[] rounds)
        {
            if (rounds != null && rounds.Length > 0)
            {
                dependencies.UpdateRound(currentGame.FindOrConstructRound(rounds[0].roundId));
                setCurrentRoundSuccess.Invoke();
            }
            else
            {
                OnListRoundsFailure(game, new CoinModeError(CoinModeErrorBase.ErrorType.Client, "NO AVAILABLE ROUNDS", "No available rounds found!"));
            }
        }

        private void OnListRoundsFailure(GameComponent game, CoinModeError error)
        {
            GetScreen<LoadingScreen>().SetScreenState(LoadingScreen.LoadingScreenState.Failed, error.userMessage);
        }

        private bool GetCurrentRoundInfo(RoundEvent onSuccess)
        {
            getCurrentRoundInfoSuccess = onSuccess;
            return OpenLoadingScreen(currentRound.GetInfo(player, onSuccess, OnGetRoundInfoFailure), RetryGetCurrentRoundInfo,
                "Getting round info!", "Failed to get round info!");
        }

        private void RetryGetCurrentRoundInfo()
        {
            UpdateLoadingScreen(currentRound.GetInfo(player, getCurrentRoundInfoSuccess, OnGetRoundInfoFailure),
                "Getting round info!", "Failed to get round info!");
        }

        private void OnGetRoundInfoFailure(RoundComponent round, CoinModeError error)
        {
            GetScreen<LoadingScreen>().SetScreenState(LoadingScreen.LoadingScreenState.Failed, error.userMessage);
        }

        private void OnPlayGameGetRoundInfoSuccess(RoundComponent round)
        {
            ConstructSessionAndSetDependencies(round);
            JoinRoundScreenData data = new JoinRoundScreenData(player, currentGame, currentRound, currentSession, startSessionImmediately);
            OpenRoundInfoScreen(data);
        }

#if UNITY_2019_3_OR_NEWER
        private void OnGetInvitedRoundInfoSuccess(RoundComponent round)
        {
            ConstructSessionAndSetDependencies(round);
            JoinRoundScreenData data = new JoinRoundScreenData(player, currentGame, currentRound, currentSession, startSessionImmediately, true,
                inviteInfo.challengingPlayer, inviteInfo.challengingScore, inviteInfo.passphrase);
            OpenRoundInfoScreen(data);
        }
#endif

        private void ConstructSessionAndSetDependencies(RoundComponent round)
        {
            SessionComponent session;
            round.ConstructSession(out session);
            dependencies.UpdateSession(session);
        }

        private void OpenRoundInfoScreen(JoinRoundScreenData data)
        {
            if (currentRound.advertDataAvailable)
            {
                CoinModeManager.advertisementComponent.SetCurrentAdvertData(currentRound.advertData);
            }
            SwitchScreen<RoundInfoScreen>(data);
        }

        private void RetryStopSession()
        {
            UpdateLoadingScreen(currentSession.Stop(OnSessionStopSuccess, OnSessionStopFailure), "Submitting score!", "Failed to submit score!");
        }

        private void OnSessionStopFailure(SessionComponent session, CoinModeError error)
        {
            GetScreen<LoadingScreen>().SetScreenState(LoadingScreen.LoadingScreenState.Failed, error.userMessage);
        }

        private void OnSessionStopSuccess(SessionComponent session)
        {
            if (displayResults)
            {
                ChallengeType supportedChallenges =
                    ChallengeType.PvP | ChallengeType.RoundInvite;
                HighScoreScreenConfig config = new HighScoreScreenConfig(supportedChallenges, true, false);
                HighScoreScreenData data = new HighScoreScreenData(player, session.round.game, session.round, session, challengeCustomJson, config);
                OpenToScreen<ResultsScreen>(data);
                challengeCustomJson = null;
            }
            else
            {
                CoinModeManager.advertisementComponent.ClearCurrentAdvertData();
                Close();
            }
        }

        private void LeaveTitleLobby()
        {
            CoinModeManager.SendLocationRequest(Location.RemovePlayerLocation(player.publicId, null, null));
        }

        // Menu Helpers
        private void LogoutConfirmed()
        {
            CoinModeManager.ClearPlayerCache();
            ResetPlayer();
            HidePlayerMenuButton();
            Close();
            LeaveTitleLobby();
            onLogout?.Invoke();
        }

        private bool CheckPlayerAndPlayTokenAreValid()
        {
            if (player == null)
            {
                CoinModeLogging.LogWarning("CoinModeMenu", "CheckPlayerAndPlayTokenAreValid", "Player component is null");
                return false;
            }

            if (player.playTokenState != PlayTokenComponent.PlayTokenState.Verified)
            {
                CoinModeLogging.LogWarning("CoinModeMenu", "CheckPlayerAndPlayTokenAreValid", "Playtoken is {0}", player.playTokenState.ToString());
                return false;
            }

            if (player.state != PlayerComponent.PlayerState.Ready && player.state != PlayerComponent.PlayerState.SessionAssigned)
            {
                CoinModeLogging.LogWarning("CoinModeMenu", "CheckPlayerAndPlayTokenAreValid", "Player is {0}", player.state.ToString());
                return false;
            }
            return true;
        }

        private bool OpenLoadingScreen(bool operationSuccess, UnityAction retryAction, string successMessage, string errorMessage)
        {
            return OpenToScreen<LoadingScreen>(new LoadingScreen.ScreenData(operationSuccess, retryAction, successMessage, errorMessage));
        }

        private void UpdateLoadingScreen(bool operationSuccess, string successMessage, string errorMessage)
        {
            GetScreen<LoadingScreen>().SetScreenState(operationSuccess, successMessage, errorMessage);
        }

        // Menu customization
        public void ResetMenuColor()
        {
            if (menuBackgroundImage != null)
            {
                menuBackgroundImage.color = CoinModeMenuStyle.backgroundColor;
                //menuBackgroundImage.material.SetColor("_Color1", Color.white);
                //menuBackgroundImage.material.SetColor("_Color2", Color.white);
            }
        }

        public void SetMenuColor(Color color)
        {
            if (menuBackgroundImage != null)
            {
                menuBackgroundImage.color = color;
            }
        }
    }
}
using CoinMode.NetApi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace CoinMode
{
    /// <summary>
    /// Simple delegate signature used for any generic CoinMode event with no return type or input parameters.
    /// </summary>
    public delegate void CoinModeEvent();

    /// <summary>
    /// Core CoinMode Manager component class.
    /// Responsible for sending cmapi requests and storing references to some core components.
    /// </summary>
    /// <remarks>
    /// An instance of this component class is automatically created if one does not already exist in the scene
    /// when the <see cref="CoinMode.CoinModeManager.instance"/> is accessed.
    /// </remarks>
    [DefaultExecutionOrder(-201)]
    [AddComponentMenu("CoinMode/CM Manager")]
    public class CoinModeManager : MonoBehaviour
    {
        /// <summary>
        /// Enum used to describe the current state of the manager class.
        /// </summary>
        /// <remarks>
        /// Much of the CoinMode functionality relies on title & wallet information
        /// the manager is not deemed to be completely ready until the appropriate data has been retrieved.
        /// For example, you cannot construct a <see cref="CoinMode.PlayTokenComponent"/> until the title 
        /// environment is initialised.
        /// See <see cref="CoinMode.TitleComponent"/> & <see cref="CoinMode.WalletComponent"/>
        /// The manager can always be used to send requests to cmapi regardless of its state.
        /// </remarks>
        public enum State
        {
            /// <summary>Manager has been constructed and is in a "clean" state.</summary>
            Clean,
            /// <summary>The title environment is being initialised.</summary>
            /// <remarks>
            /// See <see cref="CoinMode.CoinModeManager.InitialiseTitleEnvironment(CoinModeEvent , CoinModeFailure)">
            /// </remarks>
            InitialisingTitle,
            /// <summary>The managers title environment is ready and the manager can be used fully.</summary>
            Ready,
        }

        private const string productionWebsiteURL = "https://coinmode.com/";
        private const string testWebsiteURL = "https://coinmode-staging.com/";

        private const string productionDefaultWallet = "bitcoin_main";
        private const string testDefaultWallet = "bitcoin_test";

        /// <summary>
        /// Returns the current URL for the CoinMode website.
        /// </summary>
        /// <remarks>
        /// Return value switches between staging and production URLs based on current environment.
        /// See <see cref="CoinMode.CoinModeSettings.environment">
        /// </remarks>
        public static string coinModeWebsiteURL
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        {
                            return productionWebsiteURL;
                        }
                    case CoinModeEnvironment.Staging:
                        {
                            return testWebsiteURL;
                        }
                    default:
                        {
                            return productionWebsiteURL;
                        }
                }
            }
        }

        /// <summary>
        /// Returns the current default wallet id.
        /// </summary>
        /// <remarks>
        /// Return value switches between staging and production URLs based on current environment.
        /// See <see cref="CoinMode.CoinModeSettings.environment">
        /// </remarks>
        public static string defaultWallet
        {
            get
            {
                switch (CoinModeSettings.environment)
                {
                    case CoinModeEnvironment.Production:
                        {
                            return productionDefaultWallet;
                        }
                    case CoinModeEnvironment.Staging:
                        {
                            return testDefaultWallet;
                        }
                    default:
                        {
                            return testDefaultWallet;
                        }
                }
            }
        }

        /// <summary>
        /// Returns the currency code for the default local currency: "usd".
        /// </summary>
        public static string defaultLocalCurrency
        {
            get { return "usd"; }
        }

        /// <summary>
        /// Returns the current singleton instance of the manager component class.
        /// </summary>
        /// <remarks>
        /// This will check the currently active scenes to 
        /// </remarks>
        private static CoinModeManager instance
        {
            get
            {
                if(!Application.isPlaying)
                {
                    throw new System.Exception("CoinModeManager.insance should not be accessed during editor time, the instance will not be created!");
                }

                if (cleaningUp)
                {
                    throw new System.Exception("CoinModeManager.insance should not be accessed once the existing object has been cleaned up!");
                }

                if (_instance == null)
                {
                    CoinModeManager[] managers = Resources.FindObjectsOfTypeAll<CoinModeManager>();

                    for (int i = 0; i < managers.Length; i++)
                    {
                        if (managers[i].gameObject.scene.IsValid())
                        {
                            managers[i].Init();
                            break;
                        }
                    }
                }

                if (_instance == null)
                {
                    CreateManagerMonoBehavior();
                }

                return _instance;
            }
        }
        private static CoinModeManager _instance = null;

        private static bool cleaningUp = false;

        public static State state 
        { 
            get { return instance._state; } 
            private set
            {
                instance._state = value;
            }
        }

        public static TitleComponent titleComponent
        {
            get
            {
                if (instance._titleComponent == null)
                {
                    instance._titleComponent = new TitleComponent();
                }
                return instance._titleComponent;
            }
        }

        public static WalletComponent walletComponent
        {
            get
            {
                if (instance._walletComponent == null)
                {
                    instance._walletComponent = new WalletComponent();
                }
                return instance._walletComponent;
            }
        }

        public static AdvertisementComponent advertisementComponent
        {
            get
            {
                if (instance._advertisementComponent == null)
                {
                    instance._advertisementComponent = new AdvertisementComponent();
                }
                return instance._advertisementComponent;
            }
        }

        public static CoinModePlayerPrefs.CachedPlayerData recentPlayerCache { get { return instance._recentPlayerCache; } }

        public static IEnumerable<PlayerComponent> players
        {
            get
            {
                foreach (KeyValuePair<int, PlayerComponent> p in instance._players)
                {
                    yield return p.Value;
                }
            }
        }

        private RequestManager requestManager = null;
        private DownloadManager downloadManager = null;

        private State _state = State.Clean;    

        private TitleComponent _titleComponent = null;                
        private WalletComponent _walletComponent = null;        
        private AdvertisementComponent _advertisementComponent = null;
        
        private Dictionary<int, PlayerComponent> _players = new Dictionary<int, PlayerComponent>();        
        
        private CoinModePlayerPrefs.CachedPlayerData _recentPlayerCache = null;

        private CoinModeEvent initialiseTitleEnvSuccess;
        private CoinModeFailure initialiseTitleEnvFailure;

        private int lastLocalPlayerId = -1;
        private bool initialised = false;

        private static bool CreateManagerMonoBehavior()
        {
            if(_instance == null)
            {
                GameObject go = new GameObject("CoinModeManager");
                CoinModeManager manager = go.AddComponent<CoinModeManager>();
                manager.Init();
                return true;
            }
            else
            {
                CoinModeLogging.LogWarning("CoinModeManager", "CreateManager",
                    "Cannot create manager mono behavior, a valid instance already exists.");
            }
            return false;
        }                  

        public static void SendRequest<T>(CoinModeRequest<T> request) where T : CoinModeResponse
        {
            instance.requestManager.SendRequest(request);
        }

        public static void SendLocationRequest<T>(CoinModeLocationRequest<T> request) where T : CoinModeResponse
        {
            instance.requestManager.SendLocationRequest(request);
        }

        public static void SendOauthRequest<T>(CoinModeOauthRequest<T> request) where T : CoinModeResponse
        {
            instance.requestManager.SendOauthRequest(request);
        }

        public static void DownloadTexture(string url, TextureEvent onSuccess, CoinModeFailure onFailure)
        {
            instance.downloadManager.DownloadTexture(url, onSuccess, onFailure);           
        }

        public static bool InitialiseTitleEnvironment(CoinModeEvent onSuccess, CoinModeFailure onFailure)
        {
            if(state == State.Clean)
            {
                instance.initialiseTitleEnvSuccess = onSuccess;
                instance.initialiseTitleEnvFailure = onFailure;

                state = State.InitialisingTitle;
                
                if (titleComponent.state == TitleComponent.TitleState.Clean)
                {
                    titleComponent.GetTitleInfo(instance.OnGetTitleInfoSuccess, instance.OnGetTitleInfoFailure);
                }
                else if (walletComponent.walletState == WalletComponent.WalletState.Clean)
                {
                    walletComponent.GetWalletData(instance.OnGetWalletDataSuccess, instance.OnGetWalletDataFailure);
                }
                return true;
            }
            return false;
        }

        public static bool ConstructPlayer(out PlayerComponent player)
        {
            bool result = titleComponent.ConstructPlayer(instance.GetLocalPlayerId(), out player);
            if(player != null)
            {
                instance._players.Add(player.localId, player);
            }            
            return result;
        }

        public static bool TryGetFirstPlayer(out PlayerComponent player)
        {
            player = null;
            int id = 0;
            foreach(KeyValuePair<int,PlayerComponent> p in instance._players)
            {
                if(p.Key < id || player == null)
                {
                    id = p.Key;
                    player = p.Value;
                }
            }
            return player != null;
        }

        public static bool TryGetPlayer(int localId, out PlayerComponent player)
        {
            return instance._players.TryGetValue(localId, out player);
        }

        public static bool RemovePlayer(PlayerComponent player)
        {
            return RemovePlayer(player.localId);
        }

        public static bool RemovePlayer(int localId)
        {
            return instance._players.Remove(localId);
        }

        public static void ClearPlayerCache()
        {
            CoinModePlayerPrefs.ClearCache();
            CoinModePlayerPrefs.SaveToPlayerPrefs();
            if(Application.isPlaying)
            {
                instance._recentPlayerCache = null;
            }            
        }

        public static bool ConstructUser(out UserComponent user)
        {
            user = null;
            if (state == State.Ready)
            {
                user = new UserComponent();
                return true;
            }
            return false;
        }

        public static string GetInstallUrl()
        {
            return "https://install.coinmode.com/t/" + titleComponent.titleId;
        }

        public static string GetShareUrl(RoundComponent round)
        {
            System.Text.StringBuilder urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append("https://install.coinmode.com/r/");
            urlBuilder.Append(round.roundId);
            if(!string.IsNullOrWhiteSpace(round.localPassphrase))
            {
                urlBuilder.Append("/");
                urlBuilder.Append(round.localPassphrase);
            }
            return urlBuilder.ToString();
        }

        public static string GetShareUrl(SessionComponent session)
        {
            System.Text.StringBuilder urlBuilder = new System.Text.StringBuilder();
            urlBuilder.Append("https://install.coinmode.com/s/");
            urlBuilder.Append(session.sessionId);
            if (!string.IsNullOrWhiteSpace(session.localPassphrase))
            {
                urlBuilder.Append("/");
                urlBuilder.Append(session.localPassphrase);
            }
            return urlBuilder.ToString();
        }

        public static string GetProfileUrl(PlayerComponent player)
        {
            return BuildPlayerUrl(player, "&firstpage=settings");
        }

        public static string GetDepositFundsUrl(PlayerComponent player)
        {
            return BuildPlayerUrl(player, "&firstpage=walletdepositfunds");
        }

        private static string BuildPlayerUrl(PlayerComponent player, string parameters)
        {
            switch (player.authMode)
            {
                default:
                case PlayerAuthMode.UuidOrEmail:
                    return coinModeWebsiteURL + "login?uuid=" + player.loginId + parameters;
                case PlayerAuthMode.Google:
                    return coinModeWebsiteURL + "login?from=unity_profile&service=google";
                case PlayerAuthMode.Discord:
                    return coinModeWebsiteURL + "login?from=unity_profile&service=discord";
            }
        }

        // [Luke] TODO: Limit functionality to mobile only
        public static void OpenPlatformShare(string title, string content)
        {
            NativeShare share = new NativeShare();
            //share.SetTitle("Invite to " + Application.productName + "!");
            //share.SetSubject("Invite to " + Application.productName + "!");
            //share.SetUrl("https://install.coinmode.com/" + titleComponent.titleId);
            share.SetTitle(title);
            share.SetSubject(title);
            share.SetUrl(content);
            share.Share();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                CoinModeLogging.LogWarning("CoinModeManager", "Awake",
                    "A coin mode manager MonoBehaviour was destroyed because there was more than one instance.");
                Destroy(this);
            }
            else
            {
                Init();
            }
        }

        private void Update()
        {
            if(initialised)
            {
                requestManager.TickRetries(Time.deltaTime);
            }            
        }

        private void Init()
        {
            if (!initialised)
            {
                initialised = true;

                _instance = this;
                DontDestroyOnLoad(gameObject);

                CoinModePlayerPrefs.LoadFromPlayerPrefs();
                CoinModePlayerPrefs.TryGetCachedPlayerData(CoinModePlayerPrefs.GetLastLoggedInPlayer(), out _recentPlayerCache);

                requestManager = new RequestManager(this);
                downloadManager = new DownloadManager(this);
            }
        }

        private void OnDestroy()
        {
            if (_instance != null && _instance == this)
            {
                cleaningUp = true;
            }
        }

        private int GetLocalPlayerId()
        {
            instance.lastLocalPlayerId++;
            return instance.lastLocalPlayerId;
        }

        private void OnGetTitleInfoSuccess(TitleComponent title)
        {
            walletComponent.GetWalletData(OnGetWalletDataSuccess, OnGetWalletDataFailure);
        }

        private void OnGetTitleInfoFailure(TitleComponent title, CoinModeError error)
        {
            state = State.Clean;
            initialiseTitleEnvFailure?.Invoke(error);
        }

        private void OnGetWalletDataSuccess(WalletComponent walletComp)
        {
            state = State.Ready;
            initialiseTitleEnvSuccess?.Invoke();
        }

        private void OnGetWalletDataFailure(WalletComponent walletComp, CoinModeError error)
        {
            state = State.Clean;
            initialiseTitleEnvFailure?.Invoke(error);
        }
    }
}
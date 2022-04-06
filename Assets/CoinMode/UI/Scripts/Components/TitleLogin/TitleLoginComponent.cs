using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoinMode.UI
{
    [System.Serializable]
    public class TitleLoginEvent : UnityEvent<PlayerComponent> { }

    [System.Serializable]
    public class TitleLogoutEvent : UnityEvent { }

    [DefaultExecutionOrder(-200)]
    [AddComponentMenu("CoinMode/UI/CM TitleLoginComponent")]
    public class TitleLoginComponent : CoinModeUIBehaviour
    {
        public static TitleLoginComponent activeComponent { get; private set; } = null;

        [SerializeField]
        private float size = 1.0F;

        [SerializeField]
        private TitleLoginButton loginButton = null;

        [SerializeField]
        private Text loginText = null;

        [SerializeField]
        private LoadingSpinnerCircle loadingSpinner = null;

        [SerializeField]
        private HorizontalLayoutGroup poweredByContainer = null;

        [SerializeField]
        private Text poweredByText = null;

        [SerializeField]
        private Text coinModeText = null;

        [SerializeField]
        private Text loginErrorText = null;

        public TitleLoginEvent loginEvent = null;
        public TitleLogoutEvent logoutEvent = null;

        private float buttonWidth = 262.0F;
        private float buttonHeight = 72.0F;
        private float spinnerSize = 45.0F;
        private int loginFontSize = 32;
        private int errorFontSize = 18;
        private int poweredByFontSize = 10;
        private int coinModeFontSize = 14;
        private int poweredByLogoSpacing = 4;
        private float poweredContainerPosY = 4.0F;
        private float loginTextPosY = -4.75F;
        private float spinnerPosY = -6.85F;
        //private string poweredByString = "<size={0}><i>Powered by</i></size> <size={1}><b>CoinMode</b></size>";

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            EditorApplication.update -= EditorApplicationUpdate;
            EditorApplication.update += EditorApplicationUpdate;
        }

        private void EditorApplicationUpdate()
        {
            SetSize(size);
        }
#endif

        protected override void Awake()
        {
            SetSize(size);
            activeComponent = this;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(LoginAction);
            }
            if (loginErrorText != null)
            {
                loginErrorText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if(gameObject.activeInHierarchy)
            {
                UpdateUI(Time.unscaledDeltaTime);
            }            
        }

        private void LoginAction()
        {
            if (CoinModeManager.state != CoinModeManager.State.Ready)
            {
                if (CoinModeManager.InitialiseTitleEnvironment(OnInitialiseTitleSuccess, OnInitialiseTitleFailure))
                {
                    loginButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
                    if (loginErrorText != null)
                    {
                        loginErrorText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                OnInitialiseTitleSuccess();
            }
        }

        private void OnInitialiseTitleSuccess()
        {
            loginButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            CoinModeMenu.OpenLogin(OnTitleLoginSucess, OnTitleLoginFailure, OnCoinModeLogout);
        }

        private void OnCoinModeLogout()
        {
            logoutEvent?.Invoke();
        }

        private void OnInitialiseTitleFailure(CoinModeError error)
        {
            loginButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            if (loginErrorText != null)
            {
                loginErrorText.gameObject.SetActive(true);
                loginErrorText.text = error.userMessage;
            }
        }

        private void OnTitleLoginSucess(PlayerComponent player)
        {
            loginEvent?.Invoke(player);
        }

        private void OnTitleLoginFailure(PlayerComponent player, CoinModeError error)
        {
            
        }

        private void SetSize(float size)
        {
            if(loginButton != null)
            {
                RectTransform loginButtonRect = loginButton.GetComponent<RectTransform>();
                loginButtonRect.sizeDelta = new Vector2(buttonWidth * size, buttonHeight * size);
            }            

            if(loadingSpinner != null)
            {
                RectTransform spinnerRect = loadingSpinner.GetComponent<RectTransform>();
                spinnerRect.sizeDelta = new Vector2(spinnerSize * size, spinnerSize * size);
                Vector2 anchoredPos = spinnerRect.anchoredPosition;
                anchoredPos.y = spinnerPosY * size;
                spinnerRect.anchoredPosition = anchoredPos;
            }

            if (loginText != null)
            {
                loginText.fontSize = Mathf.CeilToInt(loginFontSize * size);
                RectTransform rt = loginText.transform as RectTransform;
                if (rt != null)
                {
                    Vector2 anchoredPos = rt.anchoredPosition;
                    anchoredPos.y = loginTextPosY * size;
                    rt.anchoredPosition = anchoredPos;
                }
            }
            if (loginErrorText != null) loginErrorText.fontSize = Mathf.CeilToInt(errorFontSize * size);
            if (poweredByText != null) poweredByText.fontSize = Mathf.CeilToInt(poweredByFontSize * size);
            if (coinModeText != null) coinModeText.fontSize = Mathf.CeilToInt(coinModeFontSize* size);
            if (poweredByContainer != null)
            {
                poweredByContainer.spacing = poweredByLogoSpacing * size;
                RectTransform rt = poweredByContainer.transform as RectTransform;
                if(rt != null)
                {
                    Vector2 anchoredPos = rt.anchoredPosition;
                    anchoredPos.y = poweredContainerPosY * size;
                    rt.anchoredPosition = anchoredPos;
                }
            }

        }

        //private string GetPoweredBy(int poweredBySize, int coinModeSize)
        //{
        //    return string.Format(poweredByString, poweredBySize, coinModeSize);
        //}
    }
}

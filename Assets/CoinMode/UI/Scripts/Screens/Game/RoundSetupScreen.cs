using CoinMode.NetApi;
using LightJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundSetupScreen")]
    public class RoundSetupScreen : CreateRoundFrameworkScreen
    {
        public class ScreenConfig
        {
            private const string defaultScreenTitle = "Create a Round";
            private const string defaultRoundVerb = "round";
#if UNITY_2019_3_OR_NEWER
            private const string challengeScreenTitle = "Challenge a Friend";
            private const string challengeRoundVerb = "challenge";
#endif

            public static ScreenConfig defaultConfig = new ScreenConfig(defaultScreenTitle, defaultRoundVerb);
#if UNITY_2019_3_OR_NEWER
            public static ScreenConfig defaultChallengeConfig = new ScreenConfig(challengeScreenTitle, challengeRoundVerb);
#endif

            public bool allowPassphrase { get; private set; } = true;
            public bool controlMaxSessions { get; private set; } = false;
            public string screenTitle { get; private set; } = "";
            public string roundVerb { get; private set; } = "";

            public bool allowPotContribution { get { return _allowPotContribution && CoinModeSettings.allowUserCreatedRoundsForMoney; } }
            private bool _allowPotContribution = true;

            private ScreenConfig() { }

            private ScreenConfig(string screenTitle, string roundVerb)
            {
                this.screenTitle = screenTitle;
                this.roundVerb = roundVerb;
            }

            private ScreenConfig(string screenTitle, string roundVerb, bool allowPassphrase, bool controlMaxSessions, bool allowPotContribution)
            {
                this.allowPassphrase = allowPassphrase;
                this.controlMaxSessions = controlMaxSessions;
                _allowPotContribution = allowPotContribution;
                this.screenTitle = screenTitle;
                this.roundVerb = roundVerb;
            }

            public static ScreenConfig CreateScreenConfig(bool allowPassphrase, bool controlMaxSessions, bool allowPotContribution)
            {
                return new ScreenConfig(defaultScreenTitle, defaultRoundVerb, allowPassphrase, controlMaxSessions, allowPotContribution);
            }

#if UNITY_2019_3_OR_NEWER
            public static ScreenConfig CreateChallengeScreenConfig(bool allowPassphrase, bool controlMaxSessions, bool allowPotContribution)
            {
                return new ScreenConfig(challengeScreenTitle, challengeRoundVerb, allowPassphrase, controlMaxSessions, allowPotContribution);
            }
#endif

            public static ScreenConfig CreateCustomScreenConfig(string screenTitle, string roundVerb, bool allowPassphrase, bool controlMaxSessions, bool allowPotContribution)
            {
                return new ScreenConfig(screenTitle, roundVerb, allowPassphrase, controlMaxSessions, allowPotContribution);
            }
        }

        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public GameComponent game { get; private set; }
            public JsonObject json { get; private set; }
            public string serverArgs { get; private set; }
            public bool startSessionImmediately { get; private set; }
            public ParameterCollection paramCollection { get; private set; }
#if UNITY_2019_3_OR_NEWER
            public SessionComponent existingChallengeSession { get; private set; }
#endif
            public ScreenConfig screenConfig { get; private set; }

            public ScreenData(PlayerComponent player, GameComponent game, bool startSessionImmediately, JsonObject json, string serverArgs,
                ParameterCollection paramCollection, ScreenConfig screenConfig)
            {
                this.player = player;
                this.game = game;
                this.json = json;
                this.serverArgs = serverArgs;
                this.startSessionImmediately = startSessionImmediately;
                this.paramCollection = paramCollection;
#if UNITY_2019_3_OR_NEWER
                this.existingChallengeSession = null;
#endif
                this.screenConfig = screenConfig != null ? screenConfig : ScreenConfig.defaultConfig;
            }

#if UNITY_2019_3_OR_NEWER
            public ScreenData(PlayerComponent player, GameComponent game, bool startSessionImmediately, JsonObject json, string serverArgs,
                SessionComponent existingChallengeSession, ParameterCollection paramCollection, ScreenConfig screenConfig)
            {
                this.player = player;
                this.game = game;
                this.json = json;
                this.serverArgs = serverArgs;
                this.startSessionImmediately = startSessionImmediately;
                this.existingChallengeSession = existingChallengeSession;
                this.paramCollection = paramCollection;
                this.screenConfig = screenConfig != null ? screenConfig : ScreenConfig.defaultConfig;
            }
#endif
        }

        [SerializeField] private CoinModeText createRoundTitle = null;
        [SerializeField] private CoinModeText challengeText = null;

        [SerializeField] private CoinModeInputField roundNameField = null;
        [SerializeField] private CoinModeText roundCostText = null;
        [SerializeField] private RectTransform potEntryPanel = null;
        [SerializeField] private CoinModeCurrencyInputField roundAmountField = null;
        [SerializeField] private CoinModeToggle roundPassphraseToggle = null;
        [SerializeField] private CoinModeInputField roundPassphraseField = null;

        [SerializeField] private CoinModeButton nextButton = null;
        [SerializeField] private CoinModeButton cancelButton = null;

        [SerializeField] private WalletDisplayComponent walletDisplay = null;

        public override bool requiresData { get; } = true;

        protected override CoinModeButton invokingButton { get { return nextButton; } }
        protected override PlayerComponent player { get { return screenData.player; } }
        protected override GameComponent game { get { return screenData.game; } }

        protected override string roundName { get { return _roundName; } }
        private string _roundName = "";
        protected override double potContribution { get { return _potContribution; } }
        private double _potContribution = 0.0D;
        protected override string localPassphrase 
        { 
            get 
            { 
                return screenData.screenConfig.allowPassphrase ? _localPassphrase : null; 
            } 
        }
        private string _localPassphrase = "";
        
        protected override JsonObject customJson { get { return screenData.json; } }
        protected override string serverArgs { get { return screenData.serverArgs; } }
        protected override bool startSessionImmediately { get { return screenData.startSessionImmediately; } }
        protected override SessionComponent existingChallengeSession { get { return screenData.existingChallengeSession; } }


        private ScreenData screenData = new ScreenData();

        private bool requiresPassphrase = false;
        private Wallet currentGameWallet = null;
        private CurrencyConversion conversion = new CurrencyConversion();

        protected override void Awake()
        {
            base.Awake();

            if (roundNameField != null) roundNameField.onEndEdit.AddListener(EditRoundNameDone);
            if (roundPassphraseField != null) roundPassphraseField.onEndEdit.AddListener(EditRoundPassphraseDone);
            if (roundPassphraseToggle != null) roundPassphraseToggle.onValueChanged.AddListener(OnLockToggleChanged);
            if (roundAmountField != null) roundAmountField.onEndEdit.AddListener(EditRoundAmountDone);
            if (nextButton != null) nextButton.onClick.AddListener(NextAction);
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelAction);

            if (roundPassphraseToggle != null)
            {
                roundPassphraseToggle.gameObject.SetActive(false);
                roundPassphraseToggle.isOn = false;
            }

            if (roundPassphraseField != null) roundPassphraseField.gameObject.SetActive(false);

#if !UNITY_2019_3_OR_NEWER
            if (challengeText != null)
            {
                challengeText.gameObject.SetActive(false);
            }
#endif
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);

#if UNITY_2019_3_OR_NEWER
            if (challengeText != null)
            {
                if (screenData.existingChallengeSession != null)
                {
                    challengeText.gameObject.SetActive(true);
                    challengeText.text = string.Format("Challenge other players with your score of {0}", screenData.existingChallengeSession.formattedScore);
                }
                else
                {
                    challengeText.gameObject.SetActive(false);
                }
            }
#endif

            if (createRoundTitle != null)
            {
                createRoundTitle.text = screenData.screenConfig.screenTitle;
            }

            requiresPassphrase = false;
            if (roundPassphraseToggle != null)
            {
                roundPassphraseToggle.gameObject.SetActive(screenData.screenConfig.allowPassphrase);
                requiresPassphrase = roundPassphraseToggle.isOn;
            }

            if (roundPassphraseField != null)
            {
                roundPassphraseField.gameObject.SetActive(screenData.screenConfig.allowPassphrase && requiresPassphrase);
            }

            if (screenData.game != null)
            {
                screenData.game.TryGetGameWallet(out currentGameWallet);
            }

            if (potEntryPanel != null)
            {
                potEntryPanel.gameObject.SetActive(screenData.screenConfig.allowPotContribution);
            }

            if (roundAmountField != null)
            {
                roundAmountField.SetSourceWallet(currentGameWallet);
                roundAmountField.SetLocalCurrency(screenData.player.displayCurrencyKey);
            }

            if (roundCostText != null)
            {
                if (screenData.game.createRoundFee > 0.0D || screenData.game.playFee > 0.0D)
                {
                    double cost = screenData.game.createRoundFee + screenData.game.playFee;
                    CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(currentGameWallet, cost, screenData.player.displayCurrencyKey,
                        out conversion);
                    string costText = string.Format("Creating this {0} costs {1}", screenData.screenConfig.roundVerb, conversion.targetCurrencyString);
                    roundCostText.SetText(costText);
                }
                else
                {
                    string costText = string.Format("Creating this {0} is free!", screenData.screenConfig.roundVerb);
                    roundCostText.SetText(costText);
                }
            }

            if (walletDisplay != null)
            {
                Wallet currency;
                screenData.game.TryGetGameWallet(out currency);
                walletDisplay.SetUp(screenData.player, currency);
                walletDisplay.UpdateBalance();
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            if (roundPassphraseField != null)
            {
                roundPassphraseField.SetInputText("");
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void EditRoundNameDone(string roundName)
        {
            _roundName = string.IsNullOrWhiteSpace(roundName) ? null : roundName;
        }

        private void EditRoundAmountDone(string text)
        {
            _potContribution = roundAmountField.valueAsWalletBaseUnit;
        }

        private void EditRoundPassphraseDone(string passphrase)
        {
            _localPassphrase = string.IsNullOrWhiteSpace(passphrase) ? null : passphrase;
        }

        private void OnLockToggleChanged(bool isOn)
        {
            requiresPassphrase = isOn;
            if (roundPassphraseField != null)
            {
                roundPassphraseField.gameObject.SetActive(requiresPassphrase);
            }
        }

        private void NextAction()
        {
            if (!IsRoundNameValid())
            {
                controller.DisplayMessage("Enter round name!", CoinModeMenu.MessageType.Error);
                return;
            }

            if (requiresPassphrase && !IsPasswordValid())
            {
                controller.DisplayMessage("Enter password!", CoinModeMenu.MessageType.Error);
                return;
            }

            if(screenData.paramCollection != null && screenData.paramCollection.parameters.Count > 0)
            {
                RoundParamsScreen.ScreenData data = new RoundParamsScreen.ScreenData(screenData.player, screenData.game, screenData.startSessionImmediately,
                    screenData.screenConfig.screenTitle, _roundName, _potContribution, _localPassphrase, screenData.json, screenData.serverArgs,
                    screenData.paramCollection);
                controller.SwitchScreen<RoundParamsScreen>(data);
            }
            else
            {
                if (screenData.game.createRoundFee > 0.0D || screenData.game.playFee > 0.0D ||
                potContribution > 0.0D)
                {
                    CurrencyConversion conversion;
                    Wallet gameWallet = null;
                    screenData.game.TryGetGameWallet(out gameWallet);

                    double cost = screenData.game.createRoundFee + screenData.game.playFee + potContribution;
                    CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(gameWallet, cost, screenData.player.displayCurrencyKey, out conversion);

                    List<CoinModeModalWindow.ModalWindowAction> actions = new List<CoinModeModalWindow.ModalWindowAction>();
                    actions.Add(new CoinModeModalWindow.ModalWindowAction("Yes", new UnityAction(CreateRound)));
                    actions.Add(new CoinModeModalWindow.ModalWindowAction("No", null));
                    controller.OpenModalWindow(string.Format("Creating this round costs {0}\nDo you want to continue?", conversion.targetCurrencyString), actions);
                }
                else
                {
                    CreateRound();
                }
            }            
        }

        private void CancelAction()
        {
            if (controller.ReturnToPreviousScreen().ControllerClosed())
            {
#if UNITY_2019_3_OR_NEWER
                if (screenData.existingChallengeSession != null)
                {
                    controller.OnChallengeFromExistingFailure(CoinModeErrorBase.ErrorType.Generic, "USER_EXIT", "User chose to close dialog");
                }
                else
                {
                    controller.OnCreateRoundFailure(CoinModeErrorBase.ErrorType.Generic, "USER_EXIT", "User chose to close dialog");
                }
#else
                controller.OnCreateRoundFailure(CoinModeErrorBase.ErrorType.Generic, "USER_EXIT", "User chose to close dialog");
#endif
                if (screenData.player.sessionComponent != null && screenData.player.sessionComponent == createdSession)
                {
                    screenData.player.ClearSession();
                }
            }
        }

        private bool IsPasswordValid()
        {
            return !string.IsNullOrWhiteSpace(localPassphrase);
        }

        private bool IsRoundNameValid()
        {
            return !string.IsNullOrWhiteSpace(roundName);
        }
    }
}
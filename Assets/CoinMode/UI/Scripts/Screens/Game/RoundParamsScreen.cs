using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LightJson;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundParamsScreen")]
    public class RoundParamsScreen : CreateRoundFrameworkScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public GameComponent game { get; private set; }
            public JsonObject json { get; private set; }
            public string serverArgs { get; private set; }
            public bool startSessionImmediately { get; private set; }
            public string screenTitle { get; private set; }
            public string roundName { get; private set; }
            public double potContribution { get; private set; }
            public string localPassphrase { get; private set; }
            public ParameterCollection paramCollection { get; private set; }
#if UNITY_2019_3_OR_NEWER
            public SessionComponent existingChallengeSession { get; private set; }
#endif

            public ScreenData(PlayerComponent player, GameComponent game, bool startSessionImmediately, string screenTitle, string roundName, 
                double potContribution, string localPassphrase, JsonObject json, string serverArgs, ParameterCollection parameters)
            {
                this.player = player;
                this.game = game;
                this.json = json;
                this.serverArgs = serverArgs;
                this.startSessionImmediately = startSessionImmediately;
                this.screenTitle = screenTitle;
                this.roundName = roundName;
                this.potContribution = potContribution;
                this.localPassphrase = localPassphrase;
                this.paramCollection = parameters;
#if UNITY_2019_3_OR_NEWER
                existingChallengeSession = null;
#endif
            }

#if UNITY_2019_3_OR_NEWER
            public ScreenData(PlayerComponent player, GameComponent game, bool startSessionImmediately, string screenTitle, string roundName, 
                double potContribution, string localPassphrase, JsonObject json, string serverArgs, SessionComponent existingChallengeSession, 
                ParameterCollection parameters)
            {
                this.player = player;
                this.game = game;
                this.json = json;
                this.serverArgs = serverArgs;
                this.startSessionImmediately = startSessionImmediately;
                this.screenTitle = screenTitle;
                this.roundName = roundName;
                this.potContribution = potContribution;
                this.localPassphrase = localPassphrase;
                this.existingChallengeSession = existingChallengeSession;
                this.paramCollection = parameters;
            }
#endif
        }

        [SerializeField] private CoinModeText createRoundTitle = null;
        [SerializeField] private ScrollRect scrollRect = null;

        [SerializeField] private CoinModeButton nextButton = null;
        [SerializeField] private CoinModeButton cancelButton = null;

        [SerializeField] private BoolParameterInput boolInputTemplate = null;
        [SerializeField] private DecimalParameterInput decimalInputTemplate = null;
        [SerializeField] private IntegerParameterInput integerInputTemplate = null;
        [SerializeField] private StringParameterInput stringInputTemplate = null;
        [SerializeField] private DropdownParameterInput dropdownInputTemplate = null;

        public override bool requiresData { get; } = true;

        protected override CoinModeButton invokingButton { get { return nextButton; } }
        protected override PlayerComponent player { get { return screenData.player; } }
        protected override GameComponent game { get { return screenData.game; } }

        protected override string roundName { get { return screenData.roundName; } }
        protected override double potContribution { get { return screenData.potContribution; } }
        protected override string localPassphrase { get { return screenData.localPassphrase; } }

        protected override JsonObject customJson { get { return _customJson; } }
        private JsonObject _customJson = null;
        protected override string serverArgs { get { return screenData.serverArgs; } }
        protected override bool startSessionImmediately { get { return screenData.startSessionImmediately; } }
        protected override SessionComponent existingChallengeSession { get { return screenData.existingChallengeSession; } }


        private ScreenData screenData = new ScreenData();

        private List<ParameterInput> activeParameters = new List<ParameterInput>();

        protected override void Awake()
        {
            base.Awake();
            if (nextButton != null) nextButton.onClick.AddListener(NextAction);
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelAction);

            if (boolInputTemplate != null) boolInputTemplate.gameObject.SetActive(false);
            if (decimalInputTemplate != null) decimalInputTemplate.gameObject.SetActive(false);
            if (integerInputTemplate != null) integerInputTemplate.gameObject.SetActive(false);
            if (stringInputTemplate != null) stringInputTemplate.gameObject.SetActive(false);
            if (dropdownInputTemplate != null) dropdownInputTemplate.gameObject.SetActive(false);
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);

            if (createRoundTitle != null)
            {
                createRoundTitle.text = screenData.screenTitle;
            }

            _customJson = screenData.json;

            for(int i = 0; i < activeParameters.Count; i++)
            {
                Destroy(activeParameters[i].gameObject);
            }
            activeParameters.Clear();        

            if(screenData.paramCollection != null)
            {
                foreach(KeyValuePair<string, InputParameter> param in screenData.paramCollection.parameters)
                {
                    switch (param.Value.paramType)
                    {
                        case InputParameter.ParamType.Bool:
                            {
                                BoolParameterInput paramInput = Instantiate(boolInputTemplate, scrollRect.content);
                                paramInput.gameObject.SetActive(true);
                                paramInput.SetParameter(param.Value);
                                activeParameters.Add(paramInput);
                            }
                            break;
                        case InputParameter.ParamType.String:
                            {
                                StringParameterInput paramInput = Instantiate(stringInputTemplate, scrollRect.content);
                                paramInput.gameObject.SetActive(true);
                                paramInput.SetParameter(param.Value);
                                activeParameters.Add(paramInput);
                            }
                            break;
                        case InputParameter.ParamType.Decimal:
                            {
                                DecimalParameterInput paramInput = Instantiate(decimalInputTemplate, scrollRect.content);
                                paramInput.gameObject.SetActive(true);
                                paramInput.SetParameter(param.Value);
                                activeParameters.Add(paramInput);
                            }
                            break;
                        case InputParameter.ParamType.Integer:
                            {
                                IntegerParameterInput paramInput = Instantiate(integerInputTemplate, scrollRect.content);
                                paramInput.gameObject.SetActive(true);
                                paramInput.SetParameter(param.Value);
                                activeParameters.Add(paramInput);
                            }
                            break;
                        case InputParameter.ParamType.Dropdown:
                            {
                                DropdownParameterInput paramInput = Instantiate(dropdownInputTemplate, scrollRect.content);
                                paramInput.gameObject.SetActive(true);
                                paramInput.SetParameter(param.Value);
                                activeParameters.Add(paramInput);
                            }
                            break;
                    }
                }
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void NextAction()
        {
            _customJson = BuildJsonFromParamValues(_customJson);

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

        private JsonObject BuildJsonFromParamValues(JsonObject json)
        {
            if(json == null)
            {
                json = new JsonObject();
            }

            foreach(KeyValuePair<string, InputParameter> param in screenData.paramCollection.parameters)
            {
                switch (param.Value.paramType)
                {
                    case InputParameter.ParamType.Bool:
                        {
                            BoolParam boolParam = param.Value as BoolParam;
                            json[param.Key] =  boolParam.value;
                        }                        
                        break;
                    case InputParameter.ParamType.String:
                        {
                            StringParam stringParam = param.Value as StringParam;
                            json[param.Key] = stringParam.value;
                        }
                        break;
                    case InputParameter.ParamType.Decimal:
                        {
                            DecimalParam decimalParam = param.Value as DecimalParam;
                            json[param.Key] = decimalParam.value;
                        }
                        break;
                    case InputParameter.ParamType.Integer:
                        {
                            IntegerParam integerParam = param.Value as IntegerParam;
                            json[param.Key] = integerParam.value;
                        }
                        break;
                    case InputParameter.ParamType.Dropdown:
                        {
                            DropdownParam dropdownParam = param.Value as DropdownParam;
                            json[param.Key] = dropdownParam.value;
                        }
                        break;
                }

            }
            return json;
        }
    }
}
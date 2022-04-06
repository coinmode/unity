using System;
using UnityEngine;
using UnityEngine.UI;
using CoinMode.NetApi;
using static CoinMode.WalletComponent;
using System.Collections;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM SetupTransferScreen")]
    public class SetupTransferScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public UserComponent user { get; private set; }
            public string walletId { get; private set; }

            public ScreenData(PlayerComponent player, UserComponent user, string walletId)
            {
                this.player = player;
                this.user = user;
                this.walletId = walletId;
            }
        }

        [SerializeField]
        private Text titleText = null;

        [SerializeField]
        private CurrencyDropDown currencySelector = null;

        [SerializeField]
        private CurrencyDisplayComponent currentBalance = null;

        [SerializeField]
        private CurrencyDisplayComponent newBalance = null;

        [SerializeField]
        private CurrencyDisplayComponent feeDisplay = null;

        [SerializeField]
        private CoinModeCurrencyInputField currencyInput = null;

        [SerializeField]
        private CoinModeButton cancelButton = null;

        [SerializeField]
        private CoinModeButton nextButton = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        private Wallet sourceWallet = null;
        private PlayerWalletComponent.Pocket currentPocket = null;

        private double fee = 0.0D;

        protected override void Start()
        {
            base.Start();

            if (cancelButton != null) cancelButton.onClick.AddListener(CancelAction);
            if (nextButton != null) nextButton.onClick.AddListener(NextAction);
            if (currencySelector != null) currencySelector.onCurrencySelected += OnCurrencySelected;
            if (currencyInput != null)
            {
                currencyInput.onCurrencyValueUpdated += OnCurrencyValueUpdated;
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);

            titleText.text = string.Format("Send funds to {0}", screenData.user.displayName);
            CoinModeManager.walletComponent.TryGetWallet(screenData.walletId, out sourceWallet);

            if (currencySelector != null)
            {
                currencySelector.Init(CoinModeManager.walletComponent);
                currencySelector.SetCurrency(screenData.walletId);
            }

            if (currencyInput != null)
            {
                currencyInput.SetSourceWallet(sourceWallet);
                currencyInput.SetLocalCurrency(screenData.player.displayCurrencyKey);
            }

            GetBalances();            
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void GetBalances()
        {
            SetIsLoading(true);
            screenData.player.walletComponent.GetBalances(screenData.player.playToken,
                OnGetBalanceSuccess, OnGetBalanceFailure);
        }

        private void OnGetBalanceSuccess(PlayerWalletComponent walletComp)
        {
            GetFees();
        }

        private void OnGetBalanceFailure(PlayerWalletComponent walletComp, CoinModeError error)
        {
            //CoinModeMenu.DisplayMenuMessage(error.userMessage, CoinModeMenu.MenuMessageType.Error);
            StartCoroutine(RetryGetBalances());
        }

        // This needs to re run when we switch pockets
        private void GetFees()
        {
            SetIsLoading(true);
            CoinModeManager.SendRequest(PlayersWalletTransferFunds.GetFeesToPlayerId(screenData.player.playToken,
                0.0D, sourceWallet.walletId, OnGetFeesSuccess, OnGetFeesFailure));
        }

        private void OnGetFeesSuccess(PlayersWalletTransferFunds.GetTransferFeesResponse response)
        {
            fee = response.fee != null ? response.fee.Value : 0.0D;
            SetIsLoading(false);
            screenData.player.walletComponent.TryGetWalletPocket(sourceWallet.walletId, out currentPocket);
            SetDisplayBalancesToCurrent();
        }

        private void OnGetFeesFailure(CoinModeErrorResponse response)
        {
            //CoinModeMenu.DisplayMenuMessage(error.userMessage, CoinModeMenu.MenuMessageType.Error);
            StartCoroutine(RetryGetFees());
        }

        private void OnCurrencySelected(Wallet currency)
        {
            sourceWallet = currency;
            screenData.player.walletComponent.TryGetWalletPocket(sourceWallet.walletId, out currentPocket);
            SetDisplayBalancesToCurrent();
            currencyInput.SetFromBaseUnitValue(0.0D, true);
        }

        private void OnCurrencyValueUpdated(double value, CryptoWalletUnit valueUnit)
        {
            if(valueUnit == CryptoWalletUnit.Default)
            {
                value = CoinModeManager.walletComponent.ConvertDefaultUnitToBaseUnit(sourceWallet, value);
            }

            if (value + fee > currentPocket.confirmedBalance)
            {                
                currencyInput.SetFromBaseUnitValue(currentPocket.confirmedBalance - fee, false);
                value = currencyInput.valueAsWalletBaseUnit;
            }

            if (newBalance != null)
            {
                double newValue = currentPocket.confirmedBalance - (value + fee);
                newBalance.SetConversionValues(newValue, screenData.player.displayCurrencyKey);
            }

            ValidateButtonStates();
        }

        private void NextAction()
        {
            ConfirmTransferScreen.ScreenData data = new ConfirmTransferScreen.ScreenData(screenData.player, screenData.user, currentPocket, currencyInput.valueAsWalletBaseUnit, fee);
            controller.SwitchScreen<ConfirmTransferScreen>(data);
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void SetDisplayBalancesToCurrent()
        {
            if (currentBalance != null)
            {
                currentBalance.SetSourceWallet(currentPocket.pocketId, false);
                currentBalance.SetConversionValues(currentPocket.confirmedBalance, screenData.player.displayCurrencyKey);
            }

            if (newBalance != null)
            {
                newBalance.SetSourceWallet(currentPocket.pocketId, false);
                newBalance.SetConversionValues(currentPocket.confirmedBalance - (currencyInput.valueAsWalletBaseUnit + fee), screenData.player.displayCurrencyKey);
            }

            if (feeDisplay != null)
            {
                if(fee > 0 )
                {
                    feeDisplay.gameObject.SetActive(true);
                    feeDisplay.SetSourceWallet(currentPocket.pocketId, false);
                    feeDisplay.SetConversionValues(fee, screenData.player.displayCurrencyKey);
                }
                else
                {
                    feeDisplay.gameObject.SetActive(false);
                }
            }
        }

        private void SetIsLoading(bool loading)
        {
            if (currentBalance != null) currentBalance.SetLoading(loading);
            if (newBalance != null) newBalance.SetLoading(loading);
            if (feeDisplay != null) feeDisplay.SetLoading(loading);
            if (currencySelector != null) currencySelector.interactable = !loading;
            if (currencyInput != null) currencyInput.interactable = !loading;
            if (loading)
            {
                if (nextButton) nextButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
            }
            else
            {
                ValidateButtonStates();
            }
        }

        private void ValidateButtonStates()
        {
            if (IsValueValid(currencyInput.valueAsWalletBaseUnit + fee))
            {
                if (nextButton) nextButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            }
            else
            {
                if (nextButton) nextButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
            }
        }

        private bool IsValueValid(double value)
        {
            if(currentPocket != null)
            {
                return value <= currentPocket.confirmedBalance && value > 0.0D;
            }
            return false;
        }

        private IEnumerator RetryGetFees()
        {
            yield return new WaitForSeconds(2.5f);
            GetFees();
        }

        private IEnumerator RetryGetBalances()
        {
            yield return new WaitForSeconds(2.5f);
            GetBalances();
        }
    }
}
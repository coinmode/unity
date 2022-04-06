using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoinMode.NetApi;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM ConfirmTransferScreen")]
    public class ConfirmTransferScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public UserComponent user { get; private set; }
            public PlayerWalletComponent.Pocket pocket { get; private set; }
            public double amount { get; private set; }
            public double fee { get; private set; }

            public ScreenData(PlayerComponent player, UserComponent user, PlayerWalletComponent.Pocket pocket, double amount, double fee)
            {
                this.player = player;
                this.user = user;
                this.pocket = pocket;
                this.amount = amount;
                this.fee = fee;
            }
        }

        [SerializeField]
        private Text titleText = null;

        [SerializeField]
        private CurrencyDisplayComponent currentBalance = null;

        [SerializeField]
        private CurrencyDisplayComponent transferAmount = null;

        [SerializeField]
        private CurrencyDisplayComponent feeAmount = null;

        [SerializeField]
        private Image totalDivider = null;

        [SerializeField]
        private CurrencyDisplayComponent totalAmount = null;

        [SerializeField]
        private Text totalAmountTitle = null;

        [SerializeField]
        private CurrencyDisplayComponent newBalance = null;

        [SerializeField]
        private CoinModeButton backButton = null;

        [SerializeField]
        private CoinModeButton confirmButton = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        protected override void Start()
        {
            base.Start();
            if (backButton != null) backButton.onClick.AddListener(BackAction);
            if (confirmButton != null) confirmButton.onClick.AddListener(ConfirmAction);
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            titleText.text = string.Format("Send funds to {0}", screenData.user.displayName);
            if (screenData.fee > 0)
            {
                if (transferAmount != null)
                {
                    transferAmount.gameObject.SetActive(true);
                    transferAmount.SetSourceWallet(screenData.pocket.pocketId, false);
                    transferAmount.SetConversionValues(screenData.amount, screenData.player.displayCurrencyKey);
                }
                if (feeAmount != null)
                {
                    feeAmount.gameObject.SetActive(true);
                    feeAmount.SetSourceWallet(screenData.pocket.pocketId, false);
                    feeAmount.SetConversionValues(screenData.fee, screenData.player.displayCurrencyKey);
                }
                if (totalDivider != null) totalDivider.gameObject.SetActive(true);
                if (totalAmountTitle != null) totalAmountTitle.text = "Total";
            }
            else
            {
                if (transferAmount != null) transferAmount.gameObject.SetActive(false);
                if (feeAmount != null) feeAmount.gameObject.SetActive(false);
                if (totalDivider != null) totalDivider.gameObject.SetActive(false);
                if (totalAmountTitle != null) totalAmountTitle.text = "Transfer Amount";
            }

            if (currentBalance != null)
            {
                currentBalance.SetSourceWallet(screenData.pocket.pocketId, false);
                currentBalance.SetConversionValues(screenData.pocket.confirmedBalance, screenData.player.displayCurrencyKey);
            }

            if (totalAmount != null)
            {
                totalAmount.SetSourceWallet(screenData.pocket.pocketId, false);
                totalAmount.SetConversionValues(screenData.amount + screenData.fee, screenData.player.displayCurrencyKey);
            }

            if (newBalance != null)
            {
                newBalance.SetSourceWallet(screenData.pocket.pocketId, false);
                newBalance.SetConversionValues(screenData.pocket.confirmedBalance - (screenData.amount + screenData.fee), screenData.player.displayCurrencyKey);
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void ConfirmAction()
        {
            controller.Disable();
            confirmButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            CoinModeManager.SendRequest(PlayersWalletTransferFunds.Request(screenData.player.playToken, screenData.user.publicId, 
                null, null, null, screenData.amount, screenData.pocket.pocketId, OnRequestSendFundsSuccess, OnRequestSendFundsFailure));
        }

        private void OnRequestSendFundsSuccess(PlayersWalletTransferFunds.RequestTransferResponse response)
        {
            controller.Enable();
            confirmButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            if (response.requiredVerification != null && response.requiredVerification.Length > 0)
            {
                VerificationComponent vC = new VerificationComponent();
                VerifySendFundsScreen.ScreenData data = new VerifySendFundsScreen.ScreenData(screenData.player, screenData.user, screenData.pocket, vC, response.pendingPaymentId);
                controller.SwitchScreen<VerifySendFundsScreen>(data);
            }
            else
            {
                CoinModeManager.SendRequest(PlayersWalletTransferFunds.Verify(screenData.player.playToken, response.pendingPaymentId,
                    null, OnVerifyTransferFundsSuccess, OnVerifyTransferFundsFailure));
            }
        }

        private void OnRequestSendFundsFailure(CoinModeErrorResponse response)
        {
            controller.Enable();
            confirmButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
        }

        private void OnVerifyTransferFundsSuccess(PlayersWalletTransferFunds.VerifyTransferResponse response)
        {
            controller.Enable();
            confirmButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            TransferCompleteScreen.ScreenData data = new TransferCompleteScreen.ScreenData(screenData.player, screenData.user, screenData.pocket);
            controller.SwitchScreen<TransferCompleteScreen>(data);
        }

        private void OnVerifyTransferFundsFailure(CoinModeErrorResponse response)
        {
            controller.Enable();
            confirmButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
        }                

        private void BackAction()
        {
            controller.ReturnToPreviousScreen();
        }
    }
}
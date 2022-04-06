using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoinMode.NetApi;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM TransferCompleteScreen")]
    public class TransferCompleteScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public UserComponent user { get; private set; }
            public PlayerWalletComponent.Pocket pocket { get; private set; }

            public ScreenData(PlayerComponent player, UserComponent user, PlayerWalletComponent.Pocket pocket)
            {
                this.player = player;
                this.user = user;
                this.pocket = pocket;
            }
        }

        [SerializeField]
        private Text titleText = null;

        [SerializeField]
        private CurrencyDisplayComponent newBalance = null;

        [SerializeField]
        private CoinModeButton closeButton = null;

        public override bool supportsHistory { get; } = false;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        protected override void Start()
        {
            base.Start();
            if (closeButton != null) closeButton.onClick.AddListener(CloseAction);
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            titleText.text = string.Format("Transfer to {0} successful!", screenData.user.displayName);
            controller.Disable();
            if (closeButton != null) closeButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            if (newBalance != null) newBalance.SetLoading(true);
            screenData.player.walletComponent.GetBalances(screenData.player.playToken, OnGetBalanceSuccess, OnGetBalanceFailure);

        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void OnGetBalanceSuccess(PlayerWalletComponent walletComp)
        {
            controller.Enable();
            if (closeButton != null) closeButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            if (newBalance != null)
            {
                newBalance.SetLoading(false);
                newBalance.SetSourceWallet(screenData.pocket.pocketId, false);
                newBalance.SetConversionValues(screenData.pocket.confirmedBalance, screenData.player.displayCurrencyKey);
            }
        }

        private void OnGetBalanceFailure(PlayerWalletComponent walletComp, CoinModeError error)
        {
            throw new NotImplementedException();
        }

        private void CloseAction()
        {
            controller.RemoveLatestFromHistory(typeof(ConfirmTransferScreen), typeof(VerifySendFundsScreen), 
                typeof(SetupTransferScreen), typeof(LightningDepositScreen));
            controller.ReturnToPreviousScreen();
        }
    }
}
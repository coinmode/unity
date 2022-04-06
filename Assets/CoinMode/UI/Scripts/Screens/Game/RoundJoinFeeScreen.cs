using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundJoinFeeScreen")]
    public class RoundJoinFeeScreen : SessionFrameworkScreen
    {
        [SerializeField]
        private CoinModeText gameTitleText = null;

        [SerializeField]
        private CurrencyDisplayComponent joinCurrencyDisplay = null;

        [SerializeField]
        private SponsorPanel sponsorPanel = null;

        [SerializeField]
        private CoinModeButton cancelButton = null;

        [SerializeField]
        private CoinModeButton nextButton = null;

        [SerializeField]
        private WalletDisplayComponent walletDisplay = null;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return nextButton; } }

        protected override void Start()
        {
            base.Start();
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextAction);
            }            
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<JoinRoundScreenData>(data);
            if (gameTitleText != null)
            {
                gameTitleText.SetText(screenData.game.name);
            }

            if (joinCurrencyDisplay != null)
            {
                joinCurrencyDisplay.SetSourceWallet(screenData.game.walletId);
                joinCurrencyDisplay.SetConversionValues(screenData.round.playFee, screenData.player.displayCurrencyKey);
            }

            if (walletDisplay != null)
            {
                Wallet currency;
                screenData.game.TryGetGameWallet(out currency);
                walletDisplay.SetUp(screenData.player, currency);
                walletDisplay.UpdateBalance();
            }

            if (CoinModeManager.advertisementComponent.advertDataAvailable)
            {
                StyleToMatchAdvertiser();
                DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                sponsorPanel.Init(ad.logoUrl, ad.promoImageUrlIsSet || ad.roundTextIsSet || ad.urlIsSet);
            }
            else
            {
                sponsorPanel.gameObject.SetActive(false);
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            RevertStyle();
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<JoinRoundScreenData>(data);
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void NextAction()
        {
            if (screenData.round.requiresPassphrase)
            {
                controller.SwitchScreen<RoundPasswordScreen>(screenData);
            }
            else
            {
                RequestSession();
            }
        }
    }
}
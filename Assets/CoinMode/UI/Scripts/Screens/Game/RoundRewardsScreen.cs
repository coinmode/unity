using System.Collections.Generic;
using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundRewardsScreen")]
    public class RoundRewardsScreen : SessionFrameworkScreen
    {
        [SerializeField] private CoinModeText gameTitleText = null;
        [SerializeField] private CoinModeText payoutTypeText = null;

        [SerializeField] private CurrencyDisplayComponent potentialRewardCurrencyDisplay = null;
        [SerializeField] private CurrencyDisplayComponent currentRewardCurrencyDisplay = null;
        [SerializeField] private CurrencyDisplayComponent contributionCurrencyDisplay = null;
        [SerializeField] private RectTransform contributionInfoContainer = null;
        [SerializeField] private SponsorPanel sponsorPanel = null;

        [SerializeField] private CoinModeButton cancelButton = null;
        [SerializeField] private CoinModeButton nextButton = null;        

        [SerializeField] private WalletDisplayComponent walletDisplay = null;        

        [SerializeField] private RectTransform raysImage = null;
        [SerializeField] private float raysRotateSpeed = 0.5F;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return nextButton; } }

        protected override void Start()
        {
            base.Start();
            if(cancelButton != null)
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

            if(payoutTypeText != null)
            {
                if(screenData.round.payoutType != PayoutType.None)
                {
                    payoutTypeText.SetText(CoinModeParamHelpers.PayoutTypeUserString(screenData.round.payoutType));
                    payoutTypeText.gameObject.SetActive(true);
                }
                else
                {
                    payoutTypeText.gameObject.SetActive(false);
                }
            }

            if(potentialRewardCurrencyDisplay != null)
            {
                double potential = screenData.round.winningPot + screenData.round.potContribution;
                potentialRewardCurrencyDisplay.SetSourceWallet(screenData.game.walletId);
                potentialRewardCurrencyDisplay.SetConversionValues(potential, screenData.player.displayCurrencyKey);
            }

            if (currentRewardCurrencyDisplay != null)
            {
                currentRewardCurrencyDisplay.SetSourceWallet(screenData.game.walletId);
                currentRewardCurrencyDisplay.SetConversionValues(screenData.round.winningPot, screenData.player.displayCurrencyKey);
            }

            if (contributionCurrencyDisplay != null)
            {
                contributionCurrencyDisplay.SetSourceWallet(screenData.game.walletId);
                contributionCurrencyDisplay.SetConversionValues(screenData.round.potContribution, screenData.player.displayCurrencyKey);
            }

            if (walletDisplay != null)
            {
                Wallet currency;
                screenData.game.TryGetGameWallet(out currency);
                walletDisplay.SetUp(screenData.player, currency);
                walletDisplay.UpdateBalance();
            }

            if(contributionInfoContainer != null)
            {
                contributionInfoContainer.gameObject.SetActive(screenData.round.potContribution > 0.0D);
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

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if(raysImage != null)
            {
                Vector3 rotation = raysImage.rotation.eulerAngles;
                rotation.z += raysRotateSpeed * deltaTime;
                raysImage.rotation = Quaternion.Euler(rotation);
            }
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void NextAction()
        {
            if (screenData.round.playFee > 0.0D)
            {
                controller.SwitchScreen<RoundJoinFeeScreen>(screenData);
            }
            else if (screenData.round.requiresPassphrase)
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
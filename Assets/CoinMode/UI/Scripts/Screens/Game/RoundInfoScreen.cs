using UnityEngine;
using UnityEngine.UI;
using System;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundInfoScreen")]
    public class RoundInfoScreen : SessionFrameworkScreen
    {
        [SerializeField]
        private CoinModeText challengeText = null;

        [SerializeField]
        private DownloadableImage roundImage = null;

        [SerializeField]
        private Text joinFeeText = null;

        [SerializeField]
        private Text rewardText = null;

        [SerializeField]
        private Text roundFinishText = null;

        [SerializeField]
        private Text requiresPassphraseText = null;

        [SerializeField]
        private SponsorPanel sponsorPanel = null;

        [SerializeField]
        private CoinModeButton nextButton = null;

        [SerializeField]
        private CoinModeButton cancelButton = null;

        public override bool requiresData { get; } = true;

        protected override CoinModeButton invokingButton { get { return nextButton; } }

        private Wallet currentGameWallet = null;
        private CurrencyConversion conversion = new CurrencyConversion();

        protected override void Start()
        {
            base.Start();

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextAction);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<JoinRoundScreenData>(data);
            if (sponsorPanel != null) sponsorPanel.SetUp(controller, screenData.round);

            if (screenData.game != null)
            {
                screenData.game.TryGetGameWallet(out currentGameWallet);
            }

            if (screenData.round != null)
            {
                if (roundImage != null && screenData.game != null)
                {
                    // Should swap for game image eventually
                    if (!string.IsNullOrEmpty(CoinModeManager.titleComponent.imageUrl))
                    {
                        roundImage.SetImageFromUrl(CoinModeManager.titleComponent.imageUrl);
                    }
                    else
                    {
                        roundImage.gameObject.SetActive(false);
                    }
                }

                if(challengeText != null)
                {
                    if(screenData.isChallenge)
                    {
                        challengeText.gameObject.SetActive(true);
                        if (!string.IsNullOrWhiteSpace(screenData.challengingPlayer))
                        {
                            if(!string.IsNullOrWhiteSpace(screenData.challengingScore))
                            {
                                challengeText.text = string.Format("You have been challenged by {0} with a score of {1}", screenData.challengingPlayer, 
                                    screenData.challengingScore);
                            }
                            else
                            {
                                challengeText.text = string.Format("You have been challenged by {0}", screenData.challengingPlayer);
                            }
                        }
                        else
                        {
                            challengeText.text = "You have been challenged!";
                        }                        
                    }
                    else
                    {
                        challengeText.gameObject.SetActive(false);
                    }
                }

                if (joinFeeText != null)
                {
                    if (screenData.round.playFee > 0.0D || screenData.round.potContribution > 0.0D)
                    {
                        double cost = screenData.round.playFee + screenData.round.potContribution;
                        CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(currentGameWallet, cost, screenData.player.displayCurrencyKey, out conversion);
                        joinFeeText.text = string.Format("The total cost of entering this round is {0}", conversion.targetCurrencyString);
                    }
                    else
                    {
                        joinFeeText.text = "Entering this round is free!";
                    }
                }

                if (rewardText != null)
                {
                    if (screenData.round.winningPot > 0.0D)
                    {
                        CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(currentGameWallet, screenData.round.winningPot,
                            screenData.player.displayCurrencyKey, out conversion);
                        rewardText.text = string.Format("The current winning pot for this round is {0}", conversion.targetCurrencyString);
                    }
                    else
                    {
                        rewardText.text = "This round has no reward";
                    }
                }

                if (roundFinishText != null)
                {
                    DateTimeOffset epochNow = new DateTimeOffset(DateTime.UtcNow);
                    TimeSpan seconds = TimeSpan.FromSeconds(Mathf.Max(0.0F, screenData.round.epochToFinish - epochNow.ToUnixTimeSeconds()));
                    if (seconds.TotalDays >= 1.0D)
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:dd\\.hh\\:mm\\:ss} days.", seconds);
                    }
                    else if (seconds.TotalHours >= 1.0D)
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:hh\\:mm\\:ss} hours.", seconds);
                    }
                    else if (seconds.TotalMinutes >= 1.0D)
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:mm\\:ss} minutes.", seconds);
                    }
                    else
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:ss} seconds.", seconds);
                    }
                }

                if (requiresPassphraseText != null)
                {
                    requiresPassphraseText.gameObject.SetActive(screenData.round.requiresPassphrase);
                }
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

            if (isOpen && screenData.round != null)
            {
                if (roundFinishText != null)
                {
                    DateTimeOffset epochNow = new DateTimeOffset(DateTime.UtcNow);
                    TimeSpan seconds = TimeSpan.FromSeconds(Mathf.Max(0.0F, screenData.round.epochToFinish - epochNow.ToUnixTimeSeconds()));
                    if (seconds.TotalDays >= 1.0D)
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:dd\\.hh\\:mm\\:ss} days.", seconds);
                    }
                    else if (seconds.TotalHours >= 1.0D)
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:hh\\:mm\\:ss} hours.", seconds);
                    }
                    else if (seconds.TotalMinutes >= 1.0D)
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:mm\\:ss} minutes.", seconds);
                    }
                    else
                    {
                        roundFinishText.text = string.Format("Round will finish in {0:ss} seconds.", seconds);
                    }
                }
            }
        }

        private void NextAction()
        {
            if (screenData.round.hasReward)
            {
                controller.SwitchScreen<RoundRewardsScreen>(screenData);
            }
            else if (screenData.round.playFee > 0.0D)
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

        private void CancelAction()
        {
            if(controller.ReturnToPreviousScreen().ControllerClosed())
            {
                controller.OnPlayGameFailure(CoinModeErrorBase.ErrorType.Generic, "USER_EXIT", "User chose to close play dialog");
                CoinModeManager.advertisementComponent.ClearCurrentAdvertData();
            }            
        }
    }
}
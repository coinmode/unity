using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM ResultsScreen")]
    public class ResultsScreen : ResultsScreenBase
    {
        [SerializeField] private CoinModeText scoreText = null;
        [SerializeField] private CoinModeText positionText = null;
        [SerializeField] private CoinModeText ordinalText = null;
        [SerializeField] private CurrencyDisplayComponent rewardDisplay = null;
        [SerializeField] private RectTransform winningsContainer = null;
        [SerializeField] private RectTransform resultsContainer = null;

        [SerializeField] private CoinModeText statusText = null;
        [SerializeField] private RectTransform waitingContainer = null;

        [SerializeField] private CoinModeButton highScoresButton = null;

        [SerializeField] private RectTransform raysImage = null;
        [SerializeField] private float raysRotateSpeed = -10.0F;

        private bool getScores = false;
        private float currentWaitTime = 0.0F;
        private float retryInterval = 10.0F;

        protected override void Awake()
        {
            base.Awake();
            if (highScoresButton != null)
            {
                highScoresButton.onClick.AddListener(ViewScores);
            }
        }

        protected override void OnOpen(object data)
        {
            base.OnOpen(data);

            if(screenData.session != null)
            {
                Setup();
            }
            else
            {
                switch (screenData.round.status)
                {
                    case PlayStatus.WaitingToPlay:
                    case PlayStatus.InProgress:
                        GetHighScores(screenData.round, screenData.session);
                        break;
                    case PlayStatus.Completed:
                        Setup();
                        break;
                    default:
                        HandleRoundError(screenData.round.status);
                        break;
                }
            }
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);
            if(getScores)
            {
                currentWaitTime += deltaTime;
                if(currentWaitTime >= retryInterval)
                {
                    getScores = false;
                    currentWaitTime = 0.0F;
                    GetHighScores(screenData.round, screenData.session);
                }
            }

            if (raysImage != null)
            {
                Vector3 rotation = raysImage.rotation.eulerAngles;
                rotation.z += raysRotateSpeed * deltaTime;
                raysImage.rotation = Quaternion.Euler(rotation);
            }
        }

        private void Setup()
        {
            if (!screenData.screenConfig.useExistingScores || screenData.round.GetHighScoresCount() == 0)
            {
                GetHighScores(screenData.round, screenData.session);
            }
            else
            {
                HighScore highlightedScore = null;
                foreach (HighScore highScore in screenData.round.highScores)
                {
                    if (highScore.highlighted)
                    {
                        highlightedScore = highScore;
                        break;
                    }
                }

                if (highlightedScore != null)
                {
                    Populate(highlightedScore);
                }
                else
                {
                    GetHighScores(screenData.round, screenData.session);
                }
            }
        }

        private void GetHighScores(RoundComponent round, SessionComponent session)
        {
            controller.ShowLoading();

            if (session != null)
            {
                if (statusText != null) statusText.gameObject.SetActive(false);
                round.GetHighscores(session, OnGetHighScoresSuccess, OnGetHighScoresFailure);
            }
            else
            {
                if (statusText != null)
                {
                    statusText.gameObject.SetActive(false);
                    statusText.SetText("Awaiting Result!");
                }
                round.GetHighscores(OnGetHighScoresSuccess, OnGetHighScoresFailure);
            }

            if (waitingContainer != null) waitingContainer.gameObject.SetActive(true);
            if (resultsContainer != null) resultsContainer.gameObject.SetActive(false);
        }

        protected override void OnGetHighScoresSuccess(RoundComponent round)
        {
            if (screenData.session != null)
            {                
                HighScore highlightedScore = round.GetHighlightedScore();

                if (highlightedScore != null)
                {
                    // This prevents scores be re-retrieved when returning from high score screen and spamming cmapi
                    if (!screenData.screenConfig.useExistingScores)
                    {
                        screenData.screenConfig.useExistingScores = true;
                    }
                    controller.HideLoading();
                    Populate(highlightedScore);
                }
                else
                {
                    getScores = true;
                }
            }
            else
            {
                switch (screenData.round.status)
                {
                    case PlayStatus.WaitingToPlay:
                    case PlayStatus.InProgress:
                        getScores = true;
                        break;
                    case PlayStatus.Completed:
                        HighScore highScore = round.GetHighestScoreForPlayer(screenData.player);
                        if (highScore != null)
                        {
                            // This prevents scores be re-retrieved when returning from high score screen and spamming cmapi
                            if (!screenData.screenConfig.useExistingScores)
                            {
                                screenData.screenConfig.useExistingScores = true;
                            }

                            controller.HideLoading();
                            Populate(highScore);
                        }                        
                        break;
                    default:
                        HandleRoundError(round.status);
                        break;
                }
            }
        }

        private void Populate(HighScore highScore)
        {
            if (waitingContainer != null) waitingContainer.gameObject.SetActive(false);
            if (resultsContainer != null) resultsContainer.gameObject.SetActive(true);

            if (scoreText != null) scoreText.SetText(highScore.formattedScore);

            if (positionText != null) positionText.SetText(highScore.position.ToString());
            if (ordinalText != null) ordinalText.SetText(HighScore.GetOrdinal(highScore.groupedPosition));

            bool payOut = highScore.paidOut > 0.0D;
            winningsContainer.gameObject.SetActive(payOut);
            if (payOut)
            {
                if (rewardDisplay != null)
                {
                    rewardDisplay.SetSourceWallet(screenData.game.walletId);
                    rewardDisplay.SetConversionValues(highScore.paidOut, screenData.player.displayCurrencyKey);
                }
            }

#if UNITY_2019_3_OR_NEWER
            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(screenData.supportsChallenge);
            }
#endif
        }

        private void HandleRoundError(PlayStatus status)
        {
            if (statusText != null)
            {
                statusText.SetText("No result! " + screenData.round.status.ToString());
            }
        }

        private void ViewScores()
        {
            controller.SwitchScreen<HighScoreScreen>(screenData);
        }
    }
}
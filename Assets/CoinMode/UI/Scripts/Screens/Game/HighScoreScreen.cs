using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM HighScoreScreen")]
    public class HighScoreScreen : ResultsScreenBase
    {
        [SerializeField] private HighScoreDisplay highScoreDisplayTemplate = null;
        [SerializeField] private ScrollRect highScoreScrollRect = null;
        [SerializeField] private RectTransform highScoreContainer = null;
        [SerializeField] private CoinModeText noneText = null;

        private List<HighScoreDisplay> highScoreDisplays = new List<HighScoreDisplay>();        

        protected override void Awake()
        {
            base.Awake();
            if(highScoreDisplayTemplate != null)
            {
                highScoreDisplayTemplate.gameObject.SetActive(false);
            }
        }

        protected override void OnOpen(object data)
        {
            base.OnOpen(data);
            if(screenData.round != null)
            {
                ClearHighScores();                
                if(!screenData.screenConfig.useExistingScores || screenData.round.GetHighScoresCount() == 0)
                {
                    controller.ShowLoading();
                    if (noneText != null)
                    {
                        noneText.gameObject.SetActive(false);
                    }

                    if (screenData.session != null)
                    {
                        screenData.round.GetHighscores(screenData.session, OnGetHighScoresSuccess, OnGetHighScoresFailure);
                    }
                    else
                    {
                        screenData.round.GetHighscores(OnGetHighScoresSuccess, OnGetHighScoresFailure);
                    }
                }
                else
                {
                    PopulateHighScores(screenData.round);
                }
            } 
        }

        protected override void OnGetHighScoresSuccess(RoundComponent round)
        {
            controller.HideLoading();
            PopulateHighScores(round);
        }

        private void PopulateHighScores(RoundComponent round)
        {
            HighScoreDisplay focusedDisplay = null;
            foreach (HighScore highScore in round.highScores)
            {
                HighScoreDisplay display = Instantiate(highScoreDisplayTemplate, highScoreContainer);
                display.gameObject.SetActive(true);
                display.SetInfo(highScore, screenData.player, highScore.highlighted);
                highScoreDisplays.Add(display);
                if (highScore.highlighted)
                {
                    focusedDisplay = display;
                }
            }

            if (focusedDisplay != null)
            {
                RebuildLayout(highScoreScrollRect.transform as RectTransform);
                highScoreScrollRect.FocusVertically(focusedDisplay.transform as RectTransform);
            }

            if (noneText != null)
            {
                noneText.gameObject.SetActive(highScoreDisplays.Count == 0);
            }

#if UNITY_2019_3_OR_NEWER
            if (challengeButton != null)
            {
                challengeButton.gameObject.SetActive(screenData.supportsChallenge);
            }
#endif
        }

        private void ClearHighScores()
        {
            for (int i = 0; i < highScoreDisplays.Count; i++)
            {
                highScoreDisplays[i].transform.SetParent(null);
                Destroy(highScoreDisplays[i].gameObject);
            }
            highScoreDisplays.Clear();
        }
    }
}
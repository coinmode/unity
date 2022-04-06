using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundListScreen")]
    public class RoundListScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; }
            public GameComponent game { get; private set; }

            public ScreenData(PlayerComponent player, GameComponent game)
            {
                this.player = player;
                this.game = game;
            }
        }

        [SerializeField]
        private CoinModeText gameTitleText = null;

        [SerializeField]
        private RoundEntryDisplay roundDisplayTemplate = null;

        [SerializeField]
        private ScrollRect scrollRect = null;

        [SerializeField]
        private CoinModeText noneText = null;

        [SerializeField]
        private CoinModeButton closeButton = null;

        [SerializeField]
        private CoinModeButton retryButton = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        private List<RoundEntryDisplay> roundEntries = new List<RoundEntryDisplay>();

        protected override void Start()
        {
            base.Start();
            if(roundDisplayTemplate != null)
            {
                roundDisplayTemplate.gameObject.SetActive(false);
            }
            if(closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAction);
            }
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryAction);
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            if(screenData.game != null && screenData.player != null)
            {
                ListRounds();   
            }    
        }        

        private void ListRounds()
        {
            ClearRounds();
            controller.ShowLoading();
            if (noneText != null)
            {
                noneText.gameObject.SetActive(false);
            }

            screenData.game.ListRounds(screenData.player, true, true, false, OnListRoundsSucccess, OnListRoundsFailure);

            if (gameTitleText != null)
            {
                gameTitleText.SetText(screenData.game.name);
            }
        }

        private void OnListRoundsSucccess(GameComponent game, MinimalRoundInfo[] rounds)
        {
            controller.HideLoading();
            
            for(int i = 0; i < rounds.Length; i++)
            {
                RoundEntryDisplay roundEntry = Instantiate(roundDisplayTemplate, scrollRect.content);
                roundEntry.SetInfo(this, rounds[i]);
                roundEntry.gameObject.SetActive(true);
                roundEntries.Add(roundEntry);
            }

            if (roundEntries.Count == 0)
            {
                if (noneText != null)
                {
                    noneText.gameObject.SetActive(true);
                }
            }
        }

        private void OnListRoundsFailure(GameComponent game, CoinModeError error)
        {
            controller.HideLoading();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void ClearRounds()
        {
            for (int i = 0; i < roundEntries.Count; i++)
            {
                roundEntries[i].transform.SetParent(null);
                Destroy(roundEntries[i].gameObject);
            }
            roundEntries.Clear();
        }

        private void CloseAction()
        {
            if (controller.ReturnToPreviousScreen().ControllerClosed())
            {
                controller.OnCreateRoundFailure(CoinModeErrorBase.ErrorType.Generic, "USER_EXIT", "User chose to close dialog");
            }
        }

        private void RetryAction()
        {
            if (screenData.game != null && screenData.player != null)
            {
                ListRounds();
            }
        }

        internal void PlayRound(string roundId)
        {
            RoundComponent round = screenData.game.FindOrConstructRound(roundId);
            controller.PlayGameFromRoundList(round);
        }

        internal void ViewRoundHighScores(string roundId)
        {
            RoundComponent round = screenData.game.FindOrConstructRound(roundId);
            controller.HighScoresFromRoundList(round);
        }
    }
}
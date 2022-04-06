using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    public abstract class ResultsScreenBase : CoinModeMenuScreen
    {
        [SerializeField] private CoinModeText gameTitleText = null;

        [SerializeField] protected CoinModeButton challengeButton = null;
        [SerializeField] private CoinModeButton closeButton = null;

        public override bool requiresData { get; } = true;
        protected HighScoreScreenData screenData = new HighScoreScreenData();
        protected ChallengeType supportedChallengeTypes { get; private set; } = ChallengeType.None;

        List<CoinModeModalWindow.ModalWindowAction> challengeActions = new List<CoinModeModalWindow.ModalWindowAction>();

        protected override void Awake()
        {
            base.Awake();
            challengeActions.Add(new CoinModeModalWindow.ModalWindowAction("Create", PvpChallengeAction));
            challengeActions.Add(new CoinModeModalWindow.ModalWindowAction("Invite", RoundInviteAction));
            challengeActions.Add(new CoinModeModalWindow.ModalWindowAction("Close", null));

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAction);
            }

            if (challengeButton != null)
            {
#if UNITY_2019_3_OR_NEWER
                challengeButton.onClick.AddListener(ChallengeAction);
#endif
                challengeButton.gameObject.SetActive(false);
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<HighScoreScreenData>(data);
            if (gameTitleText != null) 
            {
                string gameTitle = screenData.game != null ? screenData.game.name : "Round";
                gameTitleText.SetText(gameTitle);
            }
            supportedChallengeTypes = GetSupportedChallengeTypes(screenData);
        }

        protected override void OnClose()
        {
            controller.HideLoading();
            base.OnClose();
        }

        protected abstract void OnGetHighScoresSuccess(RoundComponent round);

        protected void OnGetHighScoresFailure(RoundComponent round, CoinModeError error)
        {
            controller.HideLoading();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<HighScoreScreenData>(data);
        }               

        private void CloseAction()
        {
            if (controller.ReturnToPreviousScreen().ControllerClosed())
            {
                if (screenData.screenConfig.clearAdvertDataOnClose)
                {
                    CoinModeManager.advertisementComponent.ClearCurrentAdvertData();
                }
            }
        }

#if UNITY_2019_3_OR_NEWER
        private void ChallengeAction()
        {
            if (supportedChallengeTypes.HasFlag(ChallengeType.PvP) && supportedChallengeTypes.HasFlag(ChallengeType.RoundInvite))
            {
                controller.OpenModalWindow("Would you like to create a new PvP challenge with your score, or invite players to this round?",
                    challengeActions);
            }
            else
            {
                switch (supportedChallengeTypes)
                {
                    case ChallengeType.PvP:
                        PvpChallengeAction();
                        break;                    
                    case ChallengeType.RoundInvite:
                        RoundInviteAction();
                        break;
                    default:
                        CoinModeLogging.LogError("ResultsScreenBase", "ChallengeAction", "Unable to display challenge dialog, error in game config!");
                        break;
                }
            }            
        }
#endif

        private void PvpChallengeAction()
        {
            controller.OpenCreatePvpChallengeFromHighScores(CoinModeSettings.GetPvpChallengeGameAlias(screenData.game.localAlias), screenData.session, 
                screenData.challengeCustomJson, OnChallengeCreatedFailure);
        }

        private void RoundInviteAction()
        {
            ShareScreen.ScreenData data = new ShareScreen.ScreenData("Invite To Round", "Invite To Round", DeepLinkUtilities.CreateRoundInviteLink(screenData.session));
            controller.SwitchScreen<ShareScreen>(data);
        }

        private void OnChallengeCreatedFailure(SessionComponent session, CoinModeError error)
        {
            controller.HideLoading();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        private ChallengeType GetSupportedChallengeTypes(HighScoreScreenData screenData)
        {
            return screenData.screenConfig.supportedChallengeType & CoinModeSettings.GameSupportedChallengeTypes(screenData.game.localAlias);
        }
    }
}
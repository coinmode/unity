using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundEntryDisplay")]
    public class RoundEntryDisplay : CoinModeListSelectable
    {
        [SerializeField] private CoinModeText roundNameText = null;
        [SerializeField] private Image passwordLockedImage = null;
        [SerializeField] private CoinModeText hasRewardText = null;
        [SerializeField] private CoinModeText playerCountText = null;
        [SerializeField] private Button highscoresButton = null;

        private RoundListScreen owningScreen = null;
        private string roundId = "";

        protected override void Start()
        {
            base.Start();
            if(highscoresButton != null)
            {
                highscoresButton.onClick.AddListener(HighScoresAction);
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if(owningScreen != null)
            {
                owningScreen.PlayRound(roundId);
            }
        }

        public void SetInfo(RoundListScreen owningScreen, MinimalRoundInfo roundInfo)
        {
            this.owningScreen = owningScreen;
            roundId = roundInfo.roundId;
            if (roundNameText != null)
            {
                roundNameText.text = !string.IsNullOrWhiteSpace(roundInfo.name) ? roundInfo.name : roundInfo.roundId;
            }

            if (passwordLockedImage != null)
            {
                passwordLockedImage.gameObject.SetActive(roundInfo.requirePassphrase.Value);
            }

            if (hasRewardText != null)
            {
                hasRewardText.gameObject.SetActive(roundInfo.potContribution > 0 || roundInfo.winningPot > 0);
            }

            if (playerCountText != null)
            {
                playerCountText.gameObject.SetActive(false);
            }
        }

        private void HighScoresAction()
        {
            if (owningScreen != null)
            {
                owningScreen.ViewRoundHighScores(roundId);
            }
        }
    }
}
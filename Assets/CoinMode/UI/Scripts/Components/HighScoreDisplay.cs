using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM HighScoreDisplay")]
    public class HighScoreDisplay : CoinModeListSelectable
    {
        [SerializeField] private CoinModeText positionText = null;
        [SerializeField] private CoinModeText nameText = null;
        [SerializeField] private CoinModeText scoreText = null;

        [SerializeField] private List<Image> higlightAccentImages = new List<Image>();

        private PlayerComponent player = null;
        private string userPublicId = "";

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (userPublicId == player.publicId)
            {
                CoinModeMenu.OpenPlayerMenu();
            }
            else
            {
                CoinModeMenu.OpenUserInfo(userPublicId);
            }
        }

        public void SetInfo(HighScore highScore, PlayerComponent player, bool highlight)
        {
            this.player = player;
            SetSelected(highlight);
            if (positionText != null)
            {
                positionText.SetText("#" + highScore.position.ToString());
            }
            if (nameText != null)
            {
                nameText.SetText(highScore.displayName);
            }
            if (scoreText != null)
            {
                if(!string.IsNullOrEmpty(highScore.formattedScore))
                {
                    scoreText.SetText(highScore.formattedScore);
                }                
                else
                {
                    scoreText.SetText(highScore.score.ToString());
                }
            }
            userPublicId = highScore.playerId;
        }

        protected override void OnBackgroundColorSet(Color color)
        {
            for (int i = 0; i < higlightAccentImages.Count; i++)
            {
                higlightAccentImages[i].color = color;
            }
        }

        protected override void OnSelectedSet(bool selected)
        {
            for (int i = 0; i < higlightAccentImages.Count; i++)
            {
                higlightAccentImages[i].gameObject.SetActive(selected);
            }
        }
    }
}
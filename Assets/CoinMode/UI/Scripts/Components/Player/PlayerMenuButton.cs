using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM PlayerMenuButton")]
    public class PlayerMenuButton : Button
    {
        [SerializeField]
        private DownloadableImage avatarDownloadableImage = null;

        [SerializeField]
        private RectTransform defaultIcon = null;

        [SerializeField]
        private RectTransform userIcon = null;

        protected override void Start()
        {
            base.Start();
            if (defaultIcon != null) defaultIcon.gameObject.SetActive(true);
            if (userIcon != null) userIcon.gameObject.SetActive(false);
        }

        internal void AssignPlayer(PlayerComponent player)
        {
            if(!string.IsNullOrEmpty(player.avatarSmallUrl))
            {
                if (defaultIcon != null) defaultIcon.gameObject.SetActive(false);
                if (userIcon != null) userIcon.gameObject.SetActive(true);
                if (avatarDownloadableImage != null) avatarDownloadableImage.SetImageFromUrl(player.avatarSmallUrl);
            }
            else
            {
                if (defaultIcon != null) defaultIcon.gameObject.SetActive(true);
                if (userIcon != null) userIcon.gameObject.SetActive(false);
            }
        }
    }
}

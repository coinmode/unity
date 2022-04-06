using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM PlayerInfoScreen")]
    public class PlayerInfoScreen : CoinModeMenuScreen
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
        private Button closeButton = null;

        [SerializeField]
        private DownloadableImage userAvatar = null;

        [SerializeField]
        private RectTransform userAvatarContainer = null;

        [SerializeField]
        private RectTransform defaultAvatar = null;

        [SerializeField]
        private CoinModeText displayName = null;

        [SerializeField]
        private CoinModeButton logoutButton = null;

        [SerializeField]
        private CoinModeButton profileButton = null;

        [SerializeField]
        private WalletDisplayComponent walletComponent = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        protected override void Start()
        {
            base.Start();
            if (closeButton != null) closeButton.onClick.AddListener(CloseAction);
            if (profileButton != null) profileButton.onClick.AddListener(ProfileAction);
            if (logoutButton != null) logoutButton.onClick.AddListener(LogoutAction);
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            controller.HidePlayerMenuButton();
            if(screenData.player != null)
            {
                if (displayName != null) displayName.text = screenData.player.displayName;
                if(!string.IsNullOrEmpty(screenData.player.avatarLargeUrl))
                {
                    if (userAvatar != null) userAvatar.SetImageFromUrl(screenData.player.avatarLargeUrl);
                    if (defaultAvatar != null) defaultAvatar.gameObject.SetActive(false);
                    if (userAvatarContainer != null) userAvatarContainer.gameObject.SetActive(true);
                }
                else
                {
                    if (defaultAvatar != null) defaultAvatar.gameObject.SetActive(true);
                    if (userAvatarContainer != null) userAvatarContainer.gameObject.SetActive(false);
                }
                if (walletComponent != null)
                {
                    Wallet wallet = null;
                    // TODO this needs to be fueled by some drop down because you might not have a current game..
                    if(screenData.game != null)
                    {
                        CoinModeManager.walletComponent.TryGetWallet(screenData.game.walletId, out wallet);
                    }
                    else
                    {
                        wallet = screenData.player.defaultWallet;
                    }
                    walletComponent.SetUp(screenData.player, wallet);
                    walletComponent.UpdateBalance();
                }
            }
        }

        protected override void OnClose()
        {
            base.OnClose();
            controller.ShowPlayerMenuButton();
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<ScreenData>(data);
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void ProfileAction()
        {
            if (controller != null) controller.OpenWebsiteProfile();
        }

        private void LogoutAction()
        {            
            if (controller != null) controller.Logout(LogoutConfirmed);            
        }

        private void LogoutConfirmed()
        {
            controller.Close();
        }
    }
}

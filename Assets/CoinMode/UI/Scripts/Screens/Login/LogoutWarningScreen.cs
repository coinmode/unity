using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LogoutWarningScreen")]
    public class LogoutWarningScreen : CoinModeMenuScreen
    {
        public Text warningText = null;

        public CoinModeButton logoutButton = null;
        public CoinModeButton cancelButton = null;

        public override bool requiresData { get; } = true;
        private PlayerComponent player = null;

        protected override void Start()
        {
            base.Start();

            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(LogoutAction);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);
            if (warningText != null)
            {
                warningText.text = string.Format(warningText.text, CoinModeManager.recentPlayerCache.displayName);
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        private void LogoutAction()
        {          
            CoinModeManager.ClearPlayerCache();
            PlayerComponent newPlayer = controller.ResetPlayer();
            controller.SwitchScreen<LoginScreen>(newPlayer);
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }        
    }
}
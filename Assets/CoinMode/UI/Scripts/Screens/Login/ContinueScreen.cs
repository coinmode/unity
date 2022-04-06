using System;
using CoinMode.NetApi;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM ContinueScreen")]
    public class ContinueScreen : LoginFrameworkScreen
    {
        public Text continueText = null;

        public CoinModeButton continueButton = null;
        public CoinModeButton logoutButton = null;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return continueButton; } }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if(continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueAction);
            }

            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(LogoutAction);
            }            
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);

            if (continueText != null)
            {
                continueText.text = string.Format(continueText.text, CoinModeManager.recentPlayerCache.displayName);
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        private void ContinueAction()
        {
            PlayerAuthMode authMode = (PlayerAuthMode)System.Enum.Parse(typeof(PlayerAuthMode), CoinModeManager.recentPlayerCache.authMode);
            switch (authMode)
            {
                case PlayerAuthMode.UuidOrEmail:
                case PlayerAuthMode.Google:
                case PlayerAuthMode.Discord:
                case PlayerAuthMode.Apple:
                    loginComponent.LoginWithExistingPlayToken(authMode, CoinModeManager.recentPlayerCache.uuid, CoinModeManager.recentPlayerCache.playToken);
                    break;
                default:
                case PlayerAuthMode.None:
                    controller.DisplayMessage("Unable to login for stored user, please logout!", CoinModeMenu.MessageType.Error);
                    break;
            }            
        }

        private void LogoutAction()
        {
            logoutButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
            CoinModeManager.SendRequest(Players.GetProperties(null, CoinModeManager.recentPlayerCache.publicId, OnGetPlayerPropertiesSuccess, OnGetPlayerPropertiesFailure));
        }

        private void OnGetPlayerPropertiesSuccess(Players.GetPropertiesResponse response)
        {
            logoutButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();

            if(response.emailVerified == null || !response.emailVerified.Value)
            {
                controller.SwitchScreen<LogoutWarningScreen>(player);
            }
            else
            {
                CoinModeManager.ClearPlayerCache();
                PlayerComponent newPlayer = controller.ResetPlayer();
                controller.SwitchScreen<LoginScreen>(newPlayer);
            }            
        }

        private void OnGetPlayerPropertiesFailure(CoinModeErrorResponse response)
        {
            logoutButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(response.userMessage, CoinModeMenu.MessageType.Error);
        }
    }
}
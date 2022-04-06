using CoinMode.NetApi;
using System;

namespace CoinMode.UI
{
    internal class LoginFrameworkComponent
    {
        public delegate CoinModeButton GetButtonEvent();

        private CoinModeMenu controller = null;
        private CoinModeButton invokingButton
        {
            get
            {
                return getInvokingButton.Invoke();
            }
        }

        private GetButtonEvent getInvokingButton;

        internal PlayerComponent player = null;

        internal LoginFrameworkComponent(CoinModeMenu controller, GetButtonEvent getInvokingButton)
        {
            this.controller = controller;
            this.getInvokingButton = getInvokingButton;
        }

        internal void RegisterNewPlayer(string displayName, string email, string mobile, DateTime dateOfBirth)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();            
            controller.DisplayMessage("Creating account!");
            string e = !string.IsNullOrWhiteSpace(email) ? email : null;
            string m = !string.IsNullOrWhiteSpace(mobile) ? mobile : null;
            player.RegisterNewPlayer(displayName, e, m, dateOfBirth, OnRegisterSuccess, OnRegisterFailure);
        }

        private void OnRegisterSuccess(PlayerComponent player)
        {
            controller.DisplayMessage("Requesting play token!");
            player.RequestNewPlayToken(OnRequestPlayTokenSuccess, OnRequestPlayTokenFailure);
        }

        private void OnRegisterFailure(PlayerComponent player, CoinModeError error)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        internal void LoginWithExistingPlayToken(PlayerAuthMode authMode, string loginId, string playToken)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
            controller.DisplayMessage("Getting play token info!");
            switch(authMode)
            {
                case PlayerAuthMode.UuidOrEmail:
                    player.AssignUuidOrEmail(loginId);
                    break;
                case PlayerAuthMode.Discord:
                    player.AssignDiscordId(loginId, "");
                    break;
                case PlayerAuthMode.Google:
                    player.AssignGoogleId(loginId, "");
                    break;
                case PlayerAuthMode.Apple:
                    player.AssignAppleId(loginId, "");
                    break;
            }            
            player.RequestExistingPlayToken(playToken, OnRequestExistingSuccess, OnRequestExistingFailure);
        }

        private void OnRequestExistingSuccess(PlayerComponent player)
        {
            if (player.state == PlayerComponent.PlayerState.PlayTokenVerified)
            {
                OnVerifyPlayTokenSuccess(player);
            }
            else if (player.authMode == PlayerAuthMode.UuidOrEmail)
            {
                controller.DisplayMessage("Requesting new play token!");
                player.RequestNewPlayToken(OnRequestPlayTokenSuccess, OnRequestPlayTokenFailure);
            }
            else
            {
                controller.DisplayMessage("Please login again to verify account!", CoinModeMenu.MessageType.Error);
                controller.PromptReLogin();
            }
        }

        private void OnRequestExistingFailure(PlayerComponent player, CoinModeError error)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        internal void LoginWithUuidOrEmail(string uuidOrEmail)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();            
            controller.DisplayMessage("Requesting play token!");
            player.AssignUuidOrEmail(uuidOrEmail);
            player.RequestNewPlayToken(OnRequestPlayTokenSuccess, OnRequestPlayTokenFailure);
        }

        internal void LoginWithDiscord(string discordId, string discordSignInId)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
            controller.DisplayMessage("Requesting play token!");
            player.AssignDiscordId(discordId, discordSignInId);
            player.RequestNewPlayToken(OnRequestPlayTokenSuccess, OnRequestPlayTokenFailure);
        }

        internal void LoginWithGoogle(string googleId, string googleSignInId)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
            controller.DisplayMessage("Requesting play token!");
            player.AssignGoogleId(googleId, googleSignInId);
            player.RequestNewPlayToken(OnRequestPlayTokenSuccess, OnRequestPlayTokenFailure);
        }

        internal void LoginWithApple(string appleId, string appleSignInId)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
            controller.DisplayMessage("Requesting play token!");
            player.AssignAppleId(appleId, appleSignInId);
            player.RequestNewPlayToken(OnRequestPlayTokenSuccess, OnRequestPlayTokenFailure);
        }

        private void OnRequestPlayTokenSuccess(PlayerComponent player)
        {
            if(player.licenseRequiresSigning)
            {
                invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                controller.Enable();
                controller.DisplayMessage("License requires signing!!");
                controller.SwitchScreen<SignLicensesScreen>(player);                
            }
            else
            {
                controller.DisplayMessage("Verifying play token!");
                player.VerifyPlayToken(OnVerifyPlayTokenSuccess, OnVerifyPlayTokenUpdate, OnVerifyPlayTokenFailure);
            }            
        }

        private void OnRequestPlayTokenFailure(PlayerComponent playToken, CoinModeError error)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        internal void LoginWithRequestedPlayToken()
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            controller.Disable();
            controller.DisplayMessage("Verifying play token!");
            player.VerifyPlayToken(OnVerifyPlayTokenSuccess, OnVerifyPlayTokenUpdate, OnVerifyPlayTokenFailure);
        }

        private void OnVerifyPlayTokenSuccess(PlayerComponent player)
        {
            controller.DisplayMessage("Getting player properties!");
            player.GetProperties(OnGetPlayerPropertiesSuccess, OnGetPlayerPropertiesFailure);            
        }

        private void OnVerifyPlayTokenUpdate(PlayerComponent player, VerificationComponent verificationComponent)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage("Play token requires user verification!");
            controller.SwitchScreen<VerifyLoginScreen>(player);
        }

        private void OnVerifyPlayTokenFailure(PlayerComponent player, CoinModeError error)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        internal void LoginWithVerifiedPlayToken()
        {
            controller.DisplayMessage("Getting player properties!");
            player.GetProperties(OnGetPlayerPropertiesSuccess, OnGetPlayerPropertiesFailure);
        }

        private void OnGetPlayerPropertiesSuccess(PlayerComponent player)
        {
            JoinTitleLobby();
        }

        private void OnGetPlayerPropertiesFailure(PlayerComponent player, CoinModeError error)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        internal void JoinTitleLobby()
        {
            controller.DisplayMessage("Joining title lobby!");
            CoinModeManager.SendLocationRequest(Location.AddPlayerLocation(player.publicId, CoinModeManager.titleComponent.titleId,
                OnAddPlayerLocationSuccess, OnAddPlayerLocationFailure));
        }

        private void OnAddPlayerLocationSuccess(Location.AddPlayerLocationResponse response)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage("Logged in successfully", CoinModeMenu.MessageType.Success);
            controller.OnLoginSuccess();
        }

        private void OnAddPlayerLocationFailure(CoinModeErrorResponse error)
        {
            invokingButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.Enable();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }
    }
}

using CoinMode.NetApi;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LoginScreen")]
    public class LoginScreen : LoginFrameworkScreen
    {
        [SerializeField]
        private CoinModeInputField userNameInputField = null;
        [SerializeField]
        private CoinModeButton quickPlayButton = null;
        [SerializeField]
        private CoinModeButton discordSignInButton = null;
        [SerializeField]
        private CoinModeButton googleSignInButton = null;
        [SerializeField]
        private CoinModeButton appleSignInButton = null;
        [SerializeField]
        private CoinModeButton loginButton = null;
        [SerializeField]
        private Button createUserButton = null;
        [SerializeField]
        private CoinModeButton closeButton = null;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton 
        {
            get
            {
                switch (authMode)
                {
                    default:
                        return loginButton;
                    case PlayerAuthMode.Discord:
                        return discordSignInButton;
                    case PlayerAuthMode.Google:
                        return googleSignInButton;
                    case PlayerAuthMode.Apple:
                        return appleSignInButton;
                }
            } 
        }

        private PlayerAuthMode authMode = PlayerAuthMode.None;
        private string currentExternalSignInId = "";

        private bool waitingForDiscordSignIn = false;
        private bool waitingForGoogleSignIn = false;
        private bool waitingForAppleSignIn = false;

        private int maxOauthPings = 2;
        private int currentOauthAttempts = 0;

        private float oauthPingInterval = 1.0F;
        private float currentPingTimer = 0.0F;

        private bool pingOauth = false;
        private bool waitingOnSignInResponse = false;

        private bool timeOutOauthAfterMaxAttempts = false;

        protected override void Start()
        {
            base.Start();
            if (userNameInputField != null)
            {
                userNameInputField.onEndEdit.AddListener(EditUsernameDone);
            }

            if (quickPlayButton != null)
            {
                quickPlayButton.onClick.AddListener(QuickPlayAction);
            }

            if (discordSignInButton != null)
            {
                discordSignInButton.onClick.AddListener(DiscordSignInAction);
            }

            if (googleSignInButton != null)
            {
                googleSignInButton.onClick.AddListener(GoogleSignInAction);
            }

            if (appleSignInButton != null)
            {
                appleSignInButton.onClick.AddListener(AppleSignInAction);
            }

            if (loginButton != null)
            {
                loginButton.onClick.AddListener(LoginAction);
            }

            if (createUserButton != null)
            {
                createUserButton.onClick.AddListener(CreateUserAction);
            }

            if (closeButton != null)
            {
                createUserButton.onClick.AddListener(CloseAction);
            }            
        }

        protected override void UpdateUI(float deltaTime)
        {
            base.UpdateUI(deltaTime);

            if(pingOauth && !waitingOnSignInResponse)
            {
                currentPingTimer += deltaTime;
                if(currentPingTimer >= oauthPingInterval)
                {
                    currentPingTimer = 0.0F;
                    waitingOnSignInResponse = true;

                    if(timeOutOauthAfterMaxAttempts)
                    {
                        currentOauthAttempts++;
                    }

                    if (waitingForDiscordSignIn)
                    {
                        CoinModeManager.SendOauthRequest(Oauth.RequestDiscordUserInformation(currentExternalSignInId, OnRequestDiscordUserInfoSuccess, OnRequestDiscordUserInfoFailure));
                    }

                    if (waitingForGoogleSignIn)
                    {
                        CoinModeManager.SendOauthRequest(Oauth.RequestGoogleUserInformation(currentExternalSignInId, OnRequestGoogleUserInfoSuccess, OnRequestGoogleUserInfoFailure));
                    }

                    if (waitingForAppleSignIn)
                    {
                        CoinModeManager.SendOauthRequest(Oauth.RequestAppleUserInformation(currentExternalSignInId, OnRequestAppleUserInfoSuccess, OnRequestAppleUserInfoFailure));
                    }
                }                
            }
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);

            if (loginButton != null)
            {
                if (userNameInputField != null)
                {
                    if (userNameInputField.text != "")
                    {
                        loginButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                    }
                    else
                    {
                        loginButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                    }
                }
                else
                {
                    loginButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }            
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        private void LoginAction()
        {
            if (userNameInputField != null)
            {
                if(userNameInputField.text != "")
                {
                    authMode = PlayerAuthMode.UuidOrEmail;
                    loginComponent.LoginWithUuidOrEmail(userNameInputField.text);
                }
                else
                {
                    controller.DisplayMessage("Please enter your email / UUID");
                }
            }
        }

        private void QuickPlayAction()
        {
            controller.SwitchScreen<CreateQuickPlayAccountScreen>(player);
        }

        private void DiscordSignInAction()
        {
            controller.Disable();
            discordSignInButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            CoinModeManager.SendOauthRequest(Oauth.RequestDiscordSignInUrl(OnRequestDiscordUrlSuccess, OnRequestDiscordUrlFailure));
        }

        private void GoogleSignInAction()
        {
            controller.Disable();
            googleSignInButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            CoinModeManager.SendOauthRequest(Oauth.RequestGoogleSignInUrl(OnRequestGoogleUrlSuccess, OnRequestGoogleUrlFailure));
        }

        private void AppleSignInAction()
        {
            controller.Disable();
            appleSignInButton.SetButtonState(CoinModeButton.ButtonState.Waiting);
            CoinModeManager.SendOauthRequest(Oauth.RequestAppleSignInUrl(OnRequestAppleUrlSuccess, OnRequestAppleUrlFailure));
        }

        private void CreateUserAction()
        {
            controller.SwitchScreen<CreateUserScreen>(player);
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void EditUsernameDone(string text)
        {
            if (loginButton != null)
            {
                if (text != "")
                {
                    loginButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    loginButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }

        private void OpenExternalSignInUrl(string externalSignInId, string url, string message, ref bool waitingFlag)
        {
            this.currentExternalSignInId = externalSignInId;
            Application.OpenURL(url);
            timeOutOauthAfterMaxAttempts = false;
            currentOauthAttempts = 0;
            controller.DisplayMessage(message, CoinModeMenu.MessageType.Message);
            waitingFlag = true;
            pingOauth = true;
        }

        private void OnRequestSignInUrlError(string message, CoinModeButton button)
        {
            controller.Enable();
            controller.DisplayMessage(message, CoinModeMenu.MessageType.Error);
            button.SetButtonState(CoinModeButton.ButtonState.Interatable);
        }

        private void OnRequestExternalSignInUserInfoSuccess(PlayerAuthMode authMode, Oauth.UserInformationResponse response, CoinModeButton button, ref bool waitingFlag, string providerName)
        {
            if (response.signedIn.HasValue && response.signedIn.Value)
            {
                this.authMode = authMode;
                CompleteOauthProcedure(button, ref waitingFlag, "Successfully signed in to " + providerName + "!", CoinModeMenu.MessageType.Success);
                switch(authMode)
                {
                    case PlayerAuthMode.Discord:
                        loginComponent.LoginWithDiscord(response.id, currentExternalSignInId);
                        break;
                    case PlayerAuthMode.Google:
                        loginComponent.LoginWithGoogle(response.id, currentExternalSignInId);
                        break;
                    case PlayerAuthMode.Apple:
                        loginComponent.LoginWithApple(response.id, currentExternalSignInId);
                        break;
                }                
            }
            else
            {
                if (timeOutOauthAfterMaxAttempts)
                {
                    if (currentOauthAttempts >= maxOauthPings)
                    {
                        CompleteOauthProcedure(button, ref waitingFlag, "Unable to authorize " + providerName + " sign in!", CoinModeMenu.MessageType.Error);
                    }
                }
            }
            waitingOnSignInResponse = false;
        }

        private void CompleteOauthProcedure(CoinModeButton button, ref bool waitingFlag, string message, CoinModeMenu.MessageType messageType)
        {
            pingOauth = false;
            waitingFlag = false;
            controller.Enable();
            button.SetButtonState(CoinModeButton.ButtonState.Interatable);
            controller.DisplayMessage(message, messageType);
        }

        private void OnRequestExternalSignInUserInfoFailure(string message, CoinModeButton button, ref bool waitingForFlag)
        {
            if (timeOutOauthAfterMaxAttempts)
            {
                if (currentOauthAttempts >= maxOauthPings)
                {
                    CompleteOauthProcedure(button, ref waitingForFlag, message, CoinModeMenu.MessageType.Error);
                }
            }
            waitingOnSignInResponse = false;
        }

        protected override void OnApplicationFocus(bool hasFocus)
        {
            if(hasFocus)
            {
                if(waitingForDiscordSignIn || waitingForGoogleSignIn || waitingForAppleSignIn)
                {
                    timeOutOauthAfterMaxAttempts = true;
                }
            }
        }

        // Discord Url
        private void OnRequestDiscordUrlSuccess(Oauth.DiscordSignInUrlResponse response)
        {
            OpenExternalSignInUrl(response.externalSignInId, response.url, "Redirecting to Discord authentication!", ref waitingForDiscordSignIn);
        }

        private void OnRequestDiscordUrlFailure(CoinModeErrorResponse response)
        {
            OnRequestSignInUrlError(response.userMessage, discordSignInButton);
        }

        // Google Url
        private void OnRequestGoogleUrlSuccess(Oauth.GoogleSignInUrlResponse response)
        {
            OpenExternalSignInUrl(response.externalSignInId, response.url, "Redirecting to Google authentication!", ref waitingForGoogleSignIn);
        }

        private void OnRequestGoogleUrlFailure(CoinModeErrorResponse response)
        {
            OnRequestSignInUrlError(response.userMessage, googleSignInButton);
        }

        // Apple Url
        private void OnRequestAppleUrlSuccess(Oauth.AppleSignInUrlResponse response)
        {
            OpenExternalSignInUrl(response.externalSignInId, response.url, "Redirecting to Apple authentication!", ref waitingForAppleSignIn);
        }

        private void OnRequestAppleUrlFailure(CoinModeErrorResponse response)
        {
            OnRequestSignInUrlError(response.userMessage, appleSignInButton);
        }

        // Discord User Info
        private void OnRequestDiscordUserInfoSuccess(Oauth.DiscordUserInformationResponse response)
        {
            OnRequestExternalSignInUserInfoSuccess(PlayerAuthMode.Discord, response, discordSignInButton, ref waitingForDiscordSignIn, "Discord");
        }

        private void OnRequestDiscordUserInfoFailure(CoinModeErrorResponse response)
        {
            OnRequestExternalSignInUserInfoFailure(response.userMessage, discordSignInButton, ref waitingForDiscordSignIn);
        }

        // Google User Info
        private void OnRequestGoogleUserInfoSuccess(Oauth.GoogleUserInformationResponse response)
        {
            OnRequestExternalSignInUserInfoSuccess(PlayerAuthMode.Google,response, googleSignInButton, ref waitingForGoogleSignIn, "Google");
        }

        private void OnRequestGoogleUserInfoFailure(CoinModeErrorResponse response)
        {
            OnRequestExternalSignInUserInfoFailure(response.userMessage, googleSignInButton, ref waitingForGoogleSignIn);
        }

        // Apple User Info
        private void OnRequestAppleUserInfoSuccess(Oauth.AppleUserInformationResponse response)
        {
            OnRequestExternalSignInUserInfoSuccess(PlayerAuthMode.Apple, response, appleSignInButton, ref waitingForAppleSignIn, "Apple");
        }

        private void OnRequestAppleUserInfoFailure(CoinModeErrorResponse response)
        {
            OnRequestExternalSignInUserInfoFailure(response.userMessage, appleSignInButton, ref waitingForAppleSignIn);
        }
    }
}
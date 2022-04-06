using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM UserInfoScreen")]
    public class UserInfoScreen : CoinModeMenuScreen
    {
        public struct ScreenData
        {
            public PlayerComponent player { get; private set; } 
            public string userId { get; private set; }

            public ScreenData(PlayerComponent player, string userId)
            {
                this.player = player;
                this.userId = userId;
            }
        }

        public UserComponent.UserState infoState 
        {
            get
            {
                if(user != null)
                {
                    return user.state;
                }
                else
                {
                    return UserComponent.UserState.Clean;
                }
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
        private CoinModeButton sendFundsButton = null;

        public override bool requiresData { get; } = true;
        private ScreenData screenData = new ScreenData();

        private UserComponent user = null;

        protected override void Start()
        {
            base.Start();
            if (closeButton != null) closeButton.onClick.AddListener(CloseAction);
            if (sendFundsButton != null) sendFundsButton.onClick.AddListener(SendFundsAction);
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<ScreenData>(data);
            controller.HidePlayerMenuButton();
            if (!string.IsNullOrWhiteSpace(screenData.userId) && (user == null || user.publicId != screenData.userId))
            {
                CoinModeManager.ConstructUser(out user);
                user.AssignPublicId(screenData.userId);
            }

            if(screenData.player != null && screenData.player.playTokenPermissions.HasFlag(PlaytokenPermissions.AllowDirectPayments))
            {
                sendFundsButton.gameObject.SetActive(true);
            }
            else
            {
                sendFundsButton.gameObject.SetActive(false);
            }

            if (controller != null)
            {
                if(user != null)
                {
                    if(user.state == UserComponent.UserState.PublicIdAssigned)
                    {
                        SetLoading(true);
                        user.GetProperties(OnGetUserPropertiesSuccess, OnGetUserPropertiesFailure);
                    }
                    else if(user.state == UserComponent.UserState.Ready)
                    {
                        SetLoading(false);
                        SetInfo(user.displayName, user.avatarLargeUrl);
                    }
                }
                else
                {
                    CoinModeLogging.LogError("UserInfoWindow", "OnOpen", "Cannot open user info window when user component is null");
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

        private void OnGetUserPropertiesSuccess(UserComponent user)
        {
            SetLoading(false);
            SetInfo(user.displayName, user.avatarLargeUrl);
        }

        private void OnGetUserPropertiesFailure(UserComponent user, CoinModeError error)
        {
            CoinModeLogging.LogWarning("UserInfoWindow", "OnGetUserPropertiesFailure", "Failed to get user properties!");
        }

        private void SetInfo(string name, string avatarURL)
        {
            if (displayName != null) displayName.text = name;
            if (!string.IsNullOrEmpty(avatarURL))
            {
                if (userAvatar != null) userAvatar.SetImageFromUrl(avatarURL);
                if (defaultAvatar != null) defaultAvatar.gameObject.SetActive(false);
                if (userAvatarContainer != null) userAvatarContainer.gameObject.SetActive(true);
            }
            else
            {
                if (defaultAvatar != null) defaultAvatar.gameObject.SetActive(true);
                if (userAvatarContainer != null) userAvatarContainer.gameObject.SetActive(false);
            }
        }

        private void SetLoading(bool loading)
        {
            if (displayName != null) displayName.text = loading ? "..." : "";
            if (userAvatarContainer != null) userAvatarContainer.gameObject.SetActive(!loading);
            if (sendFundsButton != null) sendFundsButton.SetButtonState(loading ? CoinModeButton.ButtonState.Waiting : CoinModeButton.ButtonState.Interatable);
            if(loading)
            {
                controller.ShowLoading();
            }
            else
            {
                controller.HideLoading();
            }
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void SendFundsAction()
        {
            CoinModeMenu.OpenSendFunds(user);
        }
    }
}

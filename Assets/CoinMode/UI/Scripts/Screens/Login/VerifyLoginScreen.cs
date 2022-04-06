using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM VerifyLoginScreen")]
    public class VerifyLoginScreen : VerifyScreen
    {
        [SerializeField]
        private TitlePermissionText permissionTextTemplate = null;

        [SerializeField]
        private Text permissionText = null;

        private PlayerComponent player
        {
            get { return _player; }
            set
            {
                _player = value;
                if (loginComponent != null)
                {
                    loginComponent.player = _player;
                }
            }
        }
        protected PlayerComponent _player = null;        
        internal LoginFrameworkComponent loginComponent { get; private set; } = null;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            LoginFrameworkComponent.GetButtonEvent getButtonEvent = delegate ()
            {
                return verifyButton;
            };
            loginComponent = new LoginFrameworkComponent(controller, getButtonEvent);
            loginComponent.player = _player;
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);            
            base.OnOpen(player.playTokenVerification);                        
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        protected override void OnPrePopulateVerificationFields(RectTransform contentContainer, List<GameObject> verifyChildren)
        {
            int permissionsCount = player.GetTitlePermissionsCount();
            if (permissionsCount > 0)
            {
                if (permissionText != null)
                {
                    permissionText.text = string.Format(permissionText.text, CoinModeManager.titleComponent.name);

                    for (int i = 0; i < permissionsCount; i++)
                    {
                        TitlePermissionText text = Instantiate(permissionTextTemplate, contentContainer);
                        text.gameObject.SetActive(true);
                        text.SetPermissionDetails(player.GetTitlePermission(i).permissionId, player.GetTitlePermission(i).title, player.GetTitlePermission(i).description);
                        verifyChildren.Add(text.gameObject);
                    }
                }
            }
            else
            {
                if (permissionText != null)
                {
                    permissionText.text = "Please verify this login request.";
                }
            }
        }

        protected override void VerifyAction()
        {
            base.VerifyAction();
            controller.DisplayMessage("Verifying play token!");
            player.VerifyPlayToken(OnVerifyPlayTokenSuccess, OnVerifyPlayTokenUpdate, OnVerifyPlayTokenFailure);
        }

        private void OnVerifyPlayTokenSuccess(PlayerComponent player)
        {
            controller.DisplayMessage("Getting player properties!");
            loginComponent.LoginWithVerifiedPlayToken();
        }

        private void OnVerifyPlayTokenUpdate(PlayerComponent player, VerificationComponent verification)
        {
            OnVerifyUpdate();
        }

        private void OnVerifyPlayTokenFailure(PlayerComponent player, CoinModeError error)
        {
            OnVerifyFailure();
            controller.DisplayMessage(error.userMessage, CoinModeMenu.MessageType.Error);
        }

        protected override void RejectAction()
        {
            CoinModeManager.ClearPlayerCache();
            PlayerComponent newPlayer = controller.ResetPlayer();
            controller.SwitchScreen<LoginScreen>(newPlayer);
        }
    }
}
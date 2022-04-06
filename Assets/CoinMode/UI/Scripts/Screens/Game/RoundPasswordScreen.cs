using UnityEngine;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM RoundPasswordScreen")]
    public class RoundPasswordScreen : SessionFrameworkScreen
    {

        [SerializeField]
        private CoinModeInputField passwordInputField = null;

        [SerializeField]
        private SponsorPanel sponsorPanel = null;

        [SerializeField]
        private CoinModeButton cancelButton = null;

        [SerializeField]
        private CoinModeButton nextButton = null;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return nextButton; } }

        protected override void Start()
        {
            base.Start();
            if(passwordInputField != null)
            {
                passwordInputField.onEndEdit.AddListener(EditPasswordDone);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }

            if (nextButton != null)
            {
                nextButton.onClick.AddListener(NextAction);
            }
        }

        protected override void OnOpen(object data)
        {
            screenData = ValidateObject<JoinRoundScreenData>(data);
            if (CoinModeManager.advertisementComponent.advertDataAvailable)
            {
                StyleToMatchAdvertiser();
                DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                sponsorPanel.Init(ad.logoUrl, ad.promoImageUrlIsSet || ad.roundTextIsSet || ad.urlIsSet);
            }
            else
            {
                sponsorPanel.gameObject.SetActive(false);
            }

            if(!string.IsNullOrWhiteSpace(screenData.passphrase))
            {
                passwordInputField.text = screenData.passphrase;
                passwordInputField.interactable = false;
                UpdatePassphrase(screenData.passphrase);
            }
            else
            {
                passwordInputField.interactable = true;
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            RevertStyle();
            if (passwordInputField != null)
            {
                passwordInputField.SetInputText("");
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<JoinRoundScreenData>(data);
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void EditPasswordDone(string password)
        {
            UpdatePassphrase(password);
        }

        private void NextAction()
        {
            if(IsPasswordValid())
            {
                RequestSession();
            }
            else
            {
                controller.DisplayMessage("Enter password!", CoinModeMenu.MessageType.Error);
            }
        }
    }
}
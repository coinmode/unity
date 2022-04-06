using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM CreateQuickPlayAccountScreen")]
    public class CreateQuickPlayAccountScreen : LoginFrameworkScreen
    {
        [SerializeField]
        private CoinModeInputField userNameInputField = null;

        [SerializeField]
        private CoinModeDateInputField dateOfBirthInputField = null;

        [SerializeField]
        private Button termsAndCoditionsButton = null;
        [SerializeField]
        private CoinModeToggle acceptTermsToggle = null;
        [SerializeField]
        private CoinModeButton createAccountButton = null;
        [SerializeField]
        private CoinModeButton cancelButton = null;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return createAccountButton; } }

        protected override void Start()
        {
            base.Start();
            if (userNameInputField != null)
            {
                userNameInputField.onEndEdit.AddListener(EditUsernameDone);
            }

            if (termsAndCoditionsButton != null)
            {
                termsAndCoditionsButton.onClick.AddListener(ViewTermsAction);
            }

            if (acceptTermsToggle != null)
            {
                acceptTermsToggle.onValueChanged.AddListener(AcceptTermsChanged);
            }

            if (createAccountButton != null)
            {
                createAccountButton.onClick.AddListener(CreateAccountAction);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }            
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);

            if (createAccountButton != null)
            {
                if (userNameInputField != null && acceptTermsToggle != null)
                {
                    if (userNameInputField.text != "" && acceptTermsToggle.isOn)
                    {
                        createAccountButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                    }
                    else
                    {
                        createAccountButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                    }
                }
                else
                {
                    createAccountButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        private void ViewTermsAction()
        {
            controller.SwitchScreen<TermsAndConditionsScreen>(null);
        }

        private void CreateAccountAction()
        {
            if (userNameInputField != null && acceptTermsToggle != null)
            {
                if (userNameInputField.text != "" && acceptTermsToggle.isOn)
                {
                    loginComponent.RegisterNewPlayer(userNameInputField.text, "", "", dateOfBirthInputField.GetDate());
                }
                else
                {
                    controller.DisplayMessage("Enter name and accept terms!");
                }
            }
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void EditUsernameDone(string text)
        {
            if (createAccountButton != null && acceptTermsToggle != null)
            {
                if (text != "" && acceptTermsToggle.isOn)
                {
                    createAccountButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createAccountButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }

        private void AcceptTermsChanged(bool value)
        {
            if (createAccountButton != null && userNameInputField != null)
            {
                if (userNameInputField.text != "" && value)
                {
                    createAccountButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createAccountButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }
    }
}
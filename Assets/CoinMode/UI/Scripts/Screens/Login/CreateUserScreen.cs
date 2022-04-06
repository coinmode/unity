using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM CreateUserScreen")]
    public class CreateUserScreen : LoginFrameworkScreen
    {
        [SerializeField]
        private CoinModeInputField displayNameInputField = null;
        [SerializeField]
        private CoinModeInputField emailAddressInputField = null;
        [SerializeField]
        private CoinModeInputField mobileInputField = null;

        [SerializeField]
        private CoinModeDateInputField dateOfBirthInputField = null;

        [SerializeField]
        private Button termsAndCoditionsButton = null;
        [SerializeField]
        private CoinModeToggle acceptTermsToggle = null;

        [SerializeField]
        private CoinModeButton createUserButton = null;
        [SerializeField]
        private CoinModeButton cancelButton = null;

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return createUserButton; } }

        protected override void Start()
        {
            base.Start();
            if (createUserButton != null)
            {
                createUserButton.onClick.AddListener(CreateAccountAction);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }

            if (displayNameInputField != null)
            {
                displayNameInputField.onEndEdit.AddListener(EditDisplayNameDone);
            }

            if (emailAddressInputField != null)
            {
                emailAddressInputField.onEndEdit.AddListener(EditEmailDone);
            }

            if (mobileInputField != null)
            {
                mobileInputField.onEndEdit.AddListener(EditMobileDone);
            }

            if (termsAndCoditionsButton != null)
            {
                termsAndCoditionsButton.onClick.AddListener(ViewTermsAction);
            }

            if (acceptTermsToggle != null)
            {
                acceptTermsToggle.onValueChanged.AddListener(AcceptTermsChanged);
            }            
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);
            if (createUserButton != null && displayNameInputField != null && emailAddressInputField != null && mobileInputField != null && acceptTermsToggle != null)
            {
                if (acceptTermsToggle.isOn && displayNameInputField.text != "" && 
                    (emailAddressInputField.text != "" || mobileInputField.text != ""))
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
            else if (createUserButton != null)
            {
                createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        private void CreateAccountAction()
        {
            if (displayNameInputField != null && emailAddressInputField != null && mobileInputField != null && acceptTermsToggle != null)
            {
                if (acceptTermsToggle.isOn && displayNameInputField.text != "" && 
                    (emailAddressInputField.text != "" || mobileInputField.text != ""))
                {
                    loginComponent.RegisterNewPlayer(displayNameInputField.text, emailAddressInputField.text, mobileInputField.text, dateOfBirthInputField.GetDate());
                }
                else
                {
                    controller.DisplayMessage("Enter name & (email | mobile) & accept terms!");
                }
            }
            else
            {
                createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
            }
        }

        private void ViewTermsAction()
        {
            controller.SwitchScreen<TermsAndConditionsScreen>(null);
        }

        private void CancelAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void EditDisplayNameDone(string text)
        {
            if (createUserButton != null && emailAddressInputField != null && mobileInputField != null && acceptTermsToggle != null)
            {
                if (text != "" && (emailAddressInputField.text != "" || emailAddressInputField.text != "") && acceptTermsToggle.isOn)
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }

        private void EditEmailDone(string text)
        {
            if (createUserButton != null && displayNameInputField != null && mobileInputField != null && acceptTermsToggle != null)
            {
                if ((text != "" || mobileInputField.text != "") && displayNameInputField.text != "" && acceptTermsToggle.isOn)
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }

        private void EditMobileDone(string text)
        {
            if (createUserButton != null && displayNameInputField != null && emailAddressInputField != null && acceptTermsToggle != null)
            {
                if ((text != "" || emailAddressInputField.text != "") && displayNameInputField.text != "" && acceptTermsToggle.isOn)
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }

        private void AcceptTermsChanged(bool value)
        {
            if (createUserButton != null && displayNameInputField != null && emailAddressInputField != null && mobileInputField != null)
            {
                if (value && displayNameInputField.text != "" && (emailAddressInputField.text != "" || mobileInputField.text != ""))
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
                }
                else
                {
                    createUserButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
                }
            }
        }
    }
}
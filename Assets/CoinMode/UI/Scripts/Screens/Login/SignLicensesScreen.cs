using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM SignLicensesScreen")]
    public class SignLicensesScreen : LoginFrameworkScreen
    {
        [SerializeField]
        private CoinModeButton continueButton = null;
        [SerializeField]
        private CoinModeButton cancelButton = null;

        [SerializeField]
        private RectTransform contentContainer = null;

        [SerializeField]
        private LicenseSigningField signingFieldTemplate = null;

        private List<LicenseSigningField> currentSigningFields = new List<LicenseSigningField>();

        private Stack<LicenseSigningField> tempFields = new Stack<LicenseSigningField>();

        public override bool requiresData { get; } = true;
        protected override CoinModeButton invokingButton { get { return continueButton; } }

        protected override void Start()
        {
            base.Start();

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelAction);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ContinueAction);
            }
        }

        protected override void OnOpen(object data)
        {
            player = ValidateObject<PlayerComponent>(data);

            tempFields = new Stack<LicenseSigningField>(currentSigningFields);
            currentSigningFields.Clear();
            contentContainer.DetachChildren();

            foreach(KeyValuePair<string, LicenseProperties> pair in player.requiredLicenses)
            {
                LicenseSigningField newField = tempFields.Count > 0 ? tempFields.Pop() : Instantiate(signingFieldTemplate);
                newField.transform.SetParent(contentContainer);
                newField.SetLicense(pair.Key, pair.Value, ViewTermsAction, OnLicenseSigned);
                newField.SetSigned(player.IsLicenseSigned(pair.Value.licenseId));
                newField.gameObject.SetActive(true);
                currentSigningFields.Add(newField);
            }

            foreach (LicenseSigningField field in tempFields)
            {
                Destroy(field.gameObject);
            }
            tempFields.Clear();

            if (!player.licenseRequiresSigning)
            {
                continueButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            }
            else
            {
                continueButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
            }
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<PlayerComponent>(data);
        }

        private void ContinueAction()
        {
            loginComponent.LoginWithRequestedPlayToken();
        }

        private void ViewTermsAction(string licenseKey)
        {
            controller.SwitchScreen<TermsAndConditionsScreen>(player.requiredLicenses[licenseKey]);
        }

        private void CancelAction()
        {
            CoinModeManager.ClearPlayerCache();
            PlayerComponent newPlayer = controller.ResetPlayer();
            controller.SwitchScreen<LoginScreen>(newPlayer);
        }

        private void OnLicenseSigned(string licenseId, bool value)
        {
            player.SignLicense(licenseId, value);

            if(!player.licenseRequiresSigning)
            {
                continueButton.SetButtonState(CoinModeButton.ButtonState.Interatable);
            }
            else
            {
                continueButton.SetButtonState(CoinModeButton.ButtonState.Disabled);
            }
        }
    }
}
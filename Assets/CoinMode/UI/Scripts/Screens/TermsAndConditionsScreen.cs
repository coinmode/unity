using CoinMode.NetApi;
using UnityEngine;
using UnityEngine.UI;
using static CoinMode.NetApi.Licenses;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM TermsAndConditionsScreen")]
    public class TermsAndConditionsScreen : CoinModeMenuScreen
    {
        [SerializeField]
        private LoadingSpinnerCircle loadingSpinner = null;

        [SerializeField]
        private CoinModePagedTextBox termsAndConditionsTextBox = null;

        [SerializeField]
        private Button refreshButton = null;

        [SerializeField]
        private Button backButton = null;

        public override bool requiresData { get; } = false;

        protected override void Awake()
        {
            base.Awake();
            if (refreshButton != null) refreshButton.onClick.AddListener(RefreshAction);
            if (backButton != null) backButton.onClick.AddListener(BackAction);
        }

        protected override void OnOpen(object data)
        {
            LicenseProperties properties = ValidateObject<LicenseProperties>(data, true);
            if (properties != null)
            {
                loadingSpinner.gameObject.SetActive(false);
                refreshButton.gameObject.SetActive(false);
                Populate(properties);
            }
            else
            {
                GetTerms();
            }            
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<LicenseProperties>(data, true);
        }

        private void OnTermsAndConditionsResponse(GetLicenseResponse response)
        {
            loadingSpinner.gameObject.SetActive(false);
            if (response.properties.contractText != null)
            {
                Populate(response.properties);             
            }
            else
            {
                refreshButton.gameObject.SetActive(true);
            }
        }

        private void Populate(LicenseProperties properties)
        {
            termsAndConditionsTextBox.gameObject.SetActive(true);
            termsAndConditionsTextBox.SetText(properties.contractText);
        }

        private void OnFailure(CoinModeErrorResponse response)
        {
            loadingSpinner.gameObject.SetActive(false);
            refreshButton.gameObject.SetActive(true);
        }

        private void RefreshAction()
        {
            GetTerms();
        }

        private void BackAction()
        {
            controller.ReturnToPreviousScreen();
        }

        private void GetTerms()
        {
            termsAndConditionsTextBox.gameObject.SetActive(false);
            loadingSpinner.gameObject.SetActive(true);
            refreshButton.gameObject.SetActive(false);
            CoinModeManager.SendRequest(Licenses.GetPlayerTerms(OnTermsAndConditionsResponse, OnFailure));
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM LicenseSigningField")]
    public class LicenseSigningField : CoinModeUIBehaviour
    {
        public delegate void ViewLicense(string licenseKey);
        public delegate void SignLicenseUpdate(string licenseId, bool signed);

        [SerializeField]
        private CoinModeText description = null;

        [SerializeField]
        private CoinModeButton viewButton = null;

        [SerializeField]
        private CoinModeToggle signToggle = null;

        private string licenseId = "";
        private string licenseKey = "";

        private ViewLicense viewLicenseEvent;
        private SignLicenseUpdate signLicenseEvent;

        protected override void Awake()
        {
            base.Awake();
            viewButton.onClick.AddListener(OnViewPressed);
            signToggle.onValueChanged.AddListener(OnSignToggleChanged);
        }

        public void SetLicense(string licenseKey, LicenseProperties license, ViewLicense viewLicenseEvent, SignLicenseUpdate signLicenseEvent)
        {
            licenseId = license.licenseId;
            this.licenseKey = licenseKey;
            description.text = license.description != null ? license.description : "";
            this.viewLicenseEvent = viewLicenseEvent;
            this.signLicenseEvent = signLicenseEvent;
        }

        public void SetSigned(bool signed)
        {
#if UNITY_2019_1_OR_NEWER
            signToggle.SetIsOnWithoutNotify(signed);
#else
            signToggle.SetValue(signed);
#endif
        }

        private void OnSignToggleChanged(bool value)
        {
            signLicenseEvent.Invoke(licenseId, value);
        }

        private void OnViewPressed()
        {
            viewLicenseEvent.Invoke(licenseKey);
        }
    }
}
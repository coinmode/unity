using UnityEngine;
using UnityEngine.UI;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM CurrencyDisplayComponent")]
    public class CurrencyDisplayComponent : CoinModeUIBehaviour
    {
        private enum DisplayMode
        {
            Local,
            Wallet,
            Conversion,
        }

        [SerializeField]
        private LoadingSpinnerCircle loadingSpinner = null;

        [SerializeField]
        private Text sourceCurrencyTypeText = null;

        [SerializeField]
        private Text sourceCurrencyValueText = null;

        [SerializeField]
        private Text targetCurrencyValueText = null;

        [SerializeField]
        private string defaultTargetCurrency = "usd";

        [SerializeField]
        private Image divider = null;

        [SerializeField]
        private LayoutElement valueLayoutElement = null;

        [SerializeField]
        private DisplayMode displayMode = DisplayMode.Conversion;

        [SerializeField]
        private bool loading = false;

        private Wallet sourceCurrency = null;
        private CurrencyConversion toLocal = new CurrencyConversion();
        private CurrencyConversion toWallet = new CurrencyConversion();

        protected override void OnValidate()
        {
            base.OnValidate();
            if (divider != null) divider.gameObject.SetActive(displayMode == DisplayMode.Conversion);
            if (targetCurrencyValueText != null) targetCurrencyValueText.gameObject.SetActive(
                displayMode == DisplayMode.Conversion || displayMode == DisplayMode.Local);
            if (sourceCurrencyValueText != null) sourceCurrencyValueText.gameObject.SetActive(
                displayMode == DisplayMode.Conversion || displayMode == DisplayMode.Wallet);
            UpdateLoading();
        }

        protected override void Awake()
        {
            base.Awake();
            if (divider != null) divider.gameObject.SetActive(displayMode == DisplayMode.Conversion);
            if (targetCurrencyValueText != null) targetCurrencyValueText.gameObject.SetActive(
                displayMode == DisplayMode.Conversion || displayMode == DisplayMode.Local);
            if (sourceCurrencyValueText != null) sourceCurrencyValueText.gameObject.SetActive(
                displayMode == DisplayMode.Conversion || displayMode == DisplayMode.Wallet);
            if (loadingSpinner != null) loadingSpinner.gameObject.SetActive(false);
        }

        public void SetLoading(bool loading)
        {
            if(this.loading != loading)
            {
                this.loading = loading;
                UpdateLoading();
            }            
        }

        private void UpdateLoading()
        {
            if (valueLayoutElement != null)
            {
                if (loading)
                {
                    float height = (valueLayoutElement.transform as RectTransform).rect.height;
                    valueLayoutElement.minHeight = height;
                }
                else
                {
                    valueLayoutElement.minHeight = 0;
                }
            }
            if (loadingSpinner != null) loadingSpinner.gameObject.SetActive(loading);

            if (divider != null) divider.gameObject.SetActive(displayMode == DisplayMode.Conversion && !loading);
            if (targetCurrencyValueText != null) targetCurrencyValueText.gameObject.SetActive(
                (displayMode == DisplayMode.Conversion || displayMode == DisplayMode.Local) && !loading);
            if (sourceCurrencyValueText != null) sourceCurrencyValueText.gameObject.SetActive(
                (displayMode == DisplayMode.Conversion || displayMode == DisplayMode.Wallet) && !loading);       
        }

        public bool SetSourceWallet(string walletId, bool updateTitle = true)
        {
            if (!CoinModeManager.walletComponent.TryGetWallet(walletId, out sourceCurrency))
            {
                CoinModeLogging.LogWarning("CurrencyDisplayComponent", "SetSourceCurrency", "Could not find currency for walletId {0}", walletId);
                return false;
            }

            if(sourceCurrencyTypeText != null && updateTitle)
            {
                sourceCurrencyTypeText.text = sourceCurrency.fullName;
            }
            return true;
        }

        public bool SetConversionValues(double sourceValue, string targetCurrencyKey)
        {
            targetCurrencyKey = targetCurrencyKey != null ? targetCurrencyKey : defaultTargetCurrency;
            if (CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(sourceCurrency, sourceValue, targetCurrencyKey, out toLocal) &&
                CoinModeManager.walletComponent.ConvertBaseUnitToDefaultUnit(sourceCurrency, sourceValue, out toWallet))
            {
                if(sourceCurrencyValueText != null)
                {
                    sourceCurrencyValueText.text = toWallet.targetCurrencyString;
                }

                if (targetCurrencyValueText != null)
                {
                    targetCurrencyValueText.text = "≈" + toLocal.targetCurrencyString;
                }
                return true;
            }
            else
            {
                CoinModeLogging.LogWarning("CurrencyDisplayComponent", "SetConversionValues", "Could not convert from {0} to {1}", sourceCurrency.fullName, targetCurrencyKey);
                return false;
            }
        }
    }
}

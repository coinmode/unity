using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM CurrencyInputField")]
    public class CoinModeCurrencyInputField : CoinModeInputField
    {
        public delegate void CurrencyValueEvent(double value, CryptoWalletUnit unit);

        private enum EditMode
        {
            Local = 0,
            Wallet = 1,
        }

        [SerializeField]
        private EditMode editMode = EditMode.Local;

        [SerializeField]
        private Text currencyPrefix = null;

        [SerializeField]
        private Text currencySuffix = null;

        [SerializeField]
        private Button currencySuffixButton = null;

        [SerializeField]
        private RectTransform conversionDisplay = null;

        [SerializeField]
        private Text conversionPrefix = null;

        [SerializeField]
        private Text conversionText = null;

        [SerializeField]
        private Text conversionSuffix = null;

        public CurrencyValueEvent onCurrencyValueUpdated;

        public double valueAsWalletBaseUnit
        {
            get
            {
                double value = 0.0D;
                if (sourceWallet != null && double.TryParse(text, out value))
                {
                    if(editMode == EditMode.Local)
                    {
                        double valueAsWalletUnit;
                        if (CoinModeManager.walletComponent.ConvertCurrencyToBaseUnit(localCurrencyKey, value, sourceWallet, out valueAsWalletUnit))
                        {
                            return valueAsWalletUnit;
                        }
                    }
                    else
                    {
                        return CoinModeManager.walletComponent.ConvertDefaultUnitToBaseUnit(sourceWallet, value);
                    }                    
                }
                return value;
            }
        }

        private Wallet sourceWallet = null;

        private string localCurrencyKey = "";
        private BasicCurrencyInfo localCurrencyInfo = new BasicCurrencyInfo();
        private BasicCurrencyInfo sourceCurrencyInfo = new BasicCurrencyInfo();

        private CurrencyConversion toLocal;
        private CurrencyConversion toDefault;

        public void SetSourceWallet(Wallet sourceWallet)
        {
            this.sourceWallet = sourceWallet;
            if(sourceWallet != null)
            {
                if (CoinModeManager.walletComponent.TryGetCurrencyInfo(sourceWallet.currencyKey, out sourceCurrencyInfo))
                {
                    if (editMode == EditMode.Wallet)
                    {
                        currencyPrefix.text = sourceCurrencyInfo.prefix;
                        currencySuffix.text = sourceCurrencyInfo.shortcode;
                        (placeholder as Text).text = string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", 0.0D);
                    }
                    else
                    {
                        conversionPrefix.text = sourceCurrencyInfo.prefix;
                        conversionSuffix.text = sourceCurrencyInfo.shortcode;
                    }                    
                }
                UpdateConversionDisplay(text);
            }            
        }

        public void SetLocalCurrency(string currencyKey)
        {
            localCurrencyKey = currencyKey;
            if (CoinModeManager.walletComponent.TryGetCurrencyInfo(localCurrencyKey, out localCurrencyInfo))
            {
                if (editMode == EditMode.Local)
                {
                    currencyPrefix.text = localCurrencyInfo.prefix;
                    currencySuffix.text = localCurrencyInfo.shortcode;
                    (placeholder as Text).text = string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", 0.0D);
                }
                else
                {
                    conversionPrefix.text = localCurrencyInfo.prefix;
                    conversionSuffix.text = localCurrencyInfo.shortcode;                    
                }
                UpdateConversionDisplay(text);
            }            
        }

        public void Clear()
        {
            if (editMode == EditMode.Local)
            {
                UpdateInputText(string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", 0.0D));
                conversionText.text = string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", 0.0D);
            }
            else
            {
                UpdateInputText(string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", 0.0D));
                conversionText.text = string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", 0.0D);                
            }            
        }

        public void SetFromBaseUnitValue(double value, bool notifyChange = true)
        {
            string valueAsString = text;
            if (editMode == EditMode.Wallet)
            {
                if (sourceWallet != null)
                {
                    if (CoinModeManager.walletComponent.ConvertBaseUnitToDefaultUnit(sourceWallet, value, out toDefault))
                    {
                        valueAsString = string.Format("{0:F" + toDefault.targetDecimals.ToString() + "}", toDefault.targetValue);
                    }
                }
                else
                {
                    valueAsString = string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", 0.0D);
                }
            }
            else
            {
                if (sourceWallet != null)
                {
                    if (CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(sourceWallet, value,
                        localCurrencyKey, out toLocal))
                    {
                        valueAsString = string.Format("{0:F" + toLocal.targetDecimals.ToString() + "}", toLocal.targetValue);
                    }
                }
                else
                {
                    valueAsString = string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", 0.0D);
                }
            }

            if(valueAsString != text)
            {
                UpdateInputText(valueAsString);
                UpdateConversionDisplay(valueAsString);
            }            

            if (notifyChange)
            {
                onCurrencyValueUpdated?.Invoke(valueAsWalletBaseUnit, CryptoWalletUnit.Base);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (conversionDisplay != null) conversionDisplay.gameObject.SetActive(false);
            if (currencySuffixButton != null) currencySuffixButton.onClick.AddListener(SwitchDisplay);
        }

        protected override void OnEndEdit(string value)
        {
            double currencyValue = 0.0D;
            if (double.TryParse(value, out currencyValue))
            {
                string currencyText;
                if (editMode == EditMode.Local)
                {
                    currencyText = string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", currencyValue);
                }
                else
                {
                    currencyText = string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", currencyValue);
                }
                UpdateInputText(currencyText);
            }

            if (conversionDisplay != null) conversionDisplay.gameObject.SetActive(false);
            onCurrencyValueUpdated?.Invoke(valueAsWalletBaseUnit, CryptoWalletUnit.Base);
        }

        protected override void OnValueUpdated(string value)
        {
            UpdateConversionDisplay(value);
            onCurrencyValueUpdated?.Invoke(valueAsWalletBaseUnit, CryptoWalletUnit.Base);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (conversionDisplay != null)
            {
                conversionDisplay.gameObject.SetActive(true);
                UpdateConversionDisplay(text);
            }
        }

        private void UpdateConversionDisplay(string value)
        {
            double currencyValue = 0.0D;
            double.TryParse(value, out currencyValue);
            if (editMode == EditMode.Local)
            {
                if (sourceWallet != null)
                {
                    double currencyAsBase = 0.0D;
                    if (CoinModeManager.walletComponent.ConvertCurrencyToBaseUnit(localCurrencyKey, currencyValue, sourceWallet, out currencyAsBase) &&
                        CoinModeManager.walletComponent.ConvertBaseUnitToDefaultUnit(sourceWallet, currencyAsBase, out toDefault))
                    {
                        conversionText.text = string.Format("{0:F" + toDefault.targetDecimals.ToString() + "}", toDefault.targetValue);
                    }
                }
                else
                {
                    conversionText.text = string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", 0.0D);
                }
            }
            else
            {
                if (sourceWallet != null)
                {
                    double valueAsBase = CoinModeManager.walletComponent.ConvertDefaultUnitToBaseUnit(sourceWallet, currencyValue);
                    if (CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(sourceWallet, valueAsBase, localCurrencyKey, out toLocal))
                    {
                        conversionText.text = string.Format("{0:F" + toLocal.targetDecimals.ToString() + "}", toLocal.targetValue);
                    }
                }
                else
                {
                    conversionText.text = string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", 0.0D);
                }
            }
        }

        private void SwitchDisplay()
        {
            editMode = editMode == EditMode.Local ? EditMode.Wallet : EditMode.Local;

            UpdateInputText(conversionText.text);

            switch (editMode)
            {
                case EditMode.Local:
                    currencyPrefix.text = localCurrencyInfo.prefix;
                    currencySuffix.text = localCurrencyInfo.shortcode;
                    (placeholder as Text).text = string.Format("{0:F" + localCurrencyInfo.decimals.ToString() + "}", 0.0D);
                    conversionPrefix.text = sourceCurrencyInfo.prefix;
                    conversionSuffix.text = sourceCurrencyInfo.shortcode;
                    break;
                case EditMode.Wallet:
                    currencyPrefix.text = sourceCurrencyInfo.prefix;
                    currencySuffix.text = sourceCurrencyInfo.shortcode;
                    (placeholder as Text).text = string.Format("{0:F" + sourceCurrencyInfo.decimals.ToString() + "}", 0.0D);
                    conversionPrefix.text = localCurrencyInfo.prefix;
                    conversionSuffix.text = localCurrencyInfo.shortcode;
                    break;
            }

            UpdateConversionDisplay(text);
            onCurrencyValueUpdated?.Invoke(valueAsWalletBaseUnit, CryptoWalletUnit.Base);
        }

        private void UpdateInputText(string text)
        {
#if UNITY_2019_1_OR_NEWER
            SetTextWithoutNotify(text);
#else
            this.text = text;
#endif
        }
    }
}

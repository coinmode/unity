using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CoinMode.WalletComponent;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM WalletDisplayComponent")]
    public class WalletDisplayComponent : CoinModeUIBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private CoinModeText walletTypeText = null;

        [SerializeField]
        private CoinModeText walletBalanceText = null;

        [SerializeField]
        private CoinModeText availableText = null;

        [SerializeField]
        private LoadingSpinnerCircle walletLoadingSpinner = null;

        [SerializeField]
        private HoverPopUpTrigger ratePopUpTrigger = null;

        [SerializeField]
        private Button topUpButton = null;

        private CurrencyConversion conversion = new CurrencyConversion();

        private CurrencyConversion localConversion = new CurrencyConversion();

        private PlayerComponent player = null;
        private PlayerWalletComponent.Pocket currentPocket = null;

        private Wallet wallet = null;

        private bool displayModeSource = false;

        protected override void Start()
        {
            base.Start();
            SetIsLoading(true);

            if (topUpButton != null)
            {
                topUpButton.onClick.AddListener(TopUpAction);
            }
        }

        protected override void OnApplicationFocus(bool hasFocus)
        {
            base.OnApplicationFocus(hasFocus);
            if (enabled && gameObject.activeInHierarchy && hasFocus)
            {
                UpdateBalance();
            }
        }

        public void SetUp(PlayerComponent player, Wallet wallet)
        {
            this.player = player;
            this.wallet = wallet;
        }

        public void UpdateBalance()
        {
            if (player != null && player.walletComponent != null)
            {
                SetIsLoading(true);
                player.walletComponent.GetBalances(player.playToken, OnGetBalanceSuccess, OnGetBalanceFailure);
            }
        }

        private void OnGetBalanceSuccess(PlayerWalletComponent playerWallet)
        {
            if (playerWallet != null)
            {
                if (wallet != null && playerWallet.TryGetWalletPocket(this.wallet.walletId, out currentPocket))
                {
                    CoinModeManager.walletComponent.ConvertBaseUnitToDefaultUnit(this.wallet, currentPocket.confirmedBalance, out conversion);
                    CoinModeManager.walletComponent.ConvertBaseUnitToCurrency(this.wallet, currentPocket.confirmedBalance,
                        player.displayCurrencyKey, out localConversion);

                    if (walletTypeText != null)
                    {
                        walletTypeText.SetText(this.wallet.fullName);
                    }

                    if (walletBalanceText != null)
                    {
                        walletBalanceText.SetText(displayModeSource ? conversion.targetCurrencyString : localConversion.targetCurrencyString);
                    }
                    ExchangeRatePopUp.WindowData data = new ExchangeRatePopUp.WindowData(this.wallet, player.displayCurrencyKey, currentPocket.confirmedBalance);
                    ratePopUpTrigger.SetPopUpData(data);
                }
            }
            SetIsLoading(false);
        }

        private void OnGetBalanceFailure(PlayerWalletComponent wallet, CoinModeError error)
        {
            SetIsLoading(false);
        }

        private void SetIsLoading(bool loading)
        {
            if (walletTypeText != null)
            {
                walletTypeText.gameObject.SetActive(!loading);
            }

            if (walletBalanceText != null)
            {
                walletBalanceText.gameObject.SetActive(!loading);
            }

            if (availableText != null)
            {
                availableText.gameObject.SetActive(!loading);
            }

            if (ratePopUpTrigger != null)
            {
                ratePopUpTrigger.gameObject.SetActive(!loading);
            }

            if (topUpButton != null)
            {
                topUpButton.gameObject.SetActive(!loading);
            }

            if (walletLoadingSpinner != null)
            {
                walletLoadingSpinner.gameObject.SetActive(loading);
            }
        }

        private void TopUpAction()
        {
            string url = CoinModeManager.GetDepositFundsUrl(player);
            Application.OpenURL(url);
        }

        public void OnPointerDown(PointerEventData eventData)
        {          
            SwitchDisplayMode();
        }

        private void SwitchDisplayMode()
        {
            displayModeSource = !displayModeSource;
            if (walletBalanceText != null)
            {
                walletBalanceText.SetText(displayModeSource ? conversion.targetCurrencyString : localConversion.targetCurrencyString);
            }
        }
    }
}

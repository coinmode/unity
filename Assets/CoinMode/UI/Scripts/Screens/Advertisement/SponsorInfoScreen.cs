using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM SponsorInfoScreen")]
    public class SponsorInfoScreen : CoinModeMenuScreen
    {
        [SerializeField]
        private DownloadableImage logoImage = null;

        [SerializeField]
        private DownloadableImage promoImage = null;

        [SerializeField]
        private Text roundText = null;

        [SerializeField]
        private CoinModeButton moreInfoButton = null;

        [SerializeField]
        private CoinModeButton closeButton = null;

        public override bool requiresData { get; } = true;
        private RoundComponent round = null;

        private string sponsorUrl = "";

        protected override void Start()
        {
            base.Start();

            if (moreInfoButton != null)
            {
                moreInfoButton.onClick.AddListener(MoreInfoAction);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAction);
            }
        }

        protected override void OnOpen(object data)
        {
            round = ValidateObject<RoundComponent>(data);
            if (round != null)
            {
                if (CoinModeManager.advertisementComponent.advertDataAvailable)
                {
                    StyleToMatchAdvertiser();
                    DefaultAdvertAssets ad = CoinModeManager.advertisementComponent.defaultAssets;
                    sponsorUrl = ad.url;

                    logoImage.gameObject.SetActive(ad.logoUrlIsSet);
                    if(ad.logoUrlIsSet)
                    {
                        logoImage.SetImageFromUrl(ad.logoUrl);
                    }

                    promoImage.gameObject.SetActive(ad.promoImageUrlIsSet);
                    if (ad.promoImageUrlIsSet)
                    {
                        promoImage.SetImageFromUrl(ad.promoImageUrl);
                    }

                    roundText.gameObject.SetActive(ad.roundTextIsSet);
                    if(ad.roundTextIsSet)
                    {
                        roundText.text = ad.roundText;
                    }

                    moreInfoButton.gameObject.SetActive(ad.urlIsSet);
                    sponsorUrl = ad.url;
                }
                else
                {
                    CoinModeLogging.LogError("SponsorInfoScreen", "OnOpen", "Opened sponsor info screen without valid advert data available!");
                }
            }
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            RevertStyle();
        }

        protected override bool OnUpdateData(object data) { return false; }

        public override bool IsValidData(object data)
        {
            return IsValidObject<RoundComponent>(data);
        }

        private void MoreInfoAction()
        {
            Application.OpenURL(sponsorUrl);
        }

        private void CloseAction()
        {
            controller.ReturnToPreviousScreen();
        }
    }
}

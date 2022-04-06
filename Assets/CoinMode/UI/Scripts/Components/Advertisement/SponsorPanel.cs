using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CoinMode.UI
{
    [AddComponentMenu("CoinMode/UI/CM SponsorPanel")]
    public class SponsorPanel : CoinModeUIBehaviour, IPointerDownHandler
    {
        public DownloadableImage sponsorLogoImage = null;

        private RoundComponent round;
        private CoinModeMenu controller = null;
        private bool moreInfo = false;

        public void SetUp(CoinModeMenu controller, RoundComponent round)
        {
            this.controller = controller;
            this.round = round;
        }

        public void Init(string logoUrl, bool moreInfo)
        {
            this.moreInfo = moreInfo;
            if(string.IsNullOrWhiteSpace(logoUrl))
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                sponsorLogoImage.SetImageFromUrl(logoUrl);
            }            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(moreInfo)
            {
                controller.SwitchScreen<SponsorInfoScreen>(round);
            }
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CoinMode
{
    public struct DefaultAdvertAssets
    {
        public string url { get; private set; }
        public bool urlIsSet { get; private set; }
        public string logoUrl { get; private set; }
        public bool logoUrlIsSet { get; private set; }
        public string roundText { get; private set; }
        public bool roundTextIsSet { get; private set; }
        public string promoImageUrl { get; private set; }
        public bool promoImageUrlIsSet { get; private set; }
        public Color primaryColor { get; private set; }
        public bool primaryColorIsSet { get; private set; }
        public Color secondaryColor { get; private set; }
        public bool secondaryColorIsSet { get; private set; }

        internal DefaultAdvertAssets(AdvertData sourceData)
        {
            if (sourceData != null)
            {                
                urlIsSet = sourceData.advertAssets.ContainsKey("url") && 
                    !string.IsNullOrWhiteSpace(sourceData.advertAssets["url"].value);
                url = urlIsSet ? sourceData.advertAssets["url"].value : "";

                logoUrlIsSet = sourceData.advertAssets.ContainsKey("logo") && 
                    !string.IsNullOrWhiteSpace(sourceData.advertAssets["logo"].value);
                logoUrl = logoUrlIsSet ? sourceData.advertAssets["logo"].value : "";

                roundTextIsSet = sourceData.advertAssets.ContainsKey("round_text") && 
                    !string.IsNullOrWhiteSpace(sourceData.advertAssets["round_text"].value);
                roundText = roundTextIsSet ? sourceData.advertAssets["round_text"].value : "";

                promoImageUrlIsSet = sourceData.advertAssets.ContainsKey("promo_image") && 
                    !string.IsNullOrWhiteSpace(sourceData.advertAssets["promo_image"].value);
                promoImageUrl = promoImageUrlIsSet ? sourceData.advertAssets["promo_image"].value : "";

                Color temp;

                if(sourceData.advertAssets.ContainsKey("primary_color") &&
                    !string.IsNullOrWhiteSpace(sourceData.advertAssets["primary_color"].value) &&
                    ColorUtility.TryParseHtmlString(sourceData.advertAssets["primary_color"].value, out temp))
                {
                    primaryColorIsSet = true;
                    primaryColor = temp;
                }
                else
                {
                    primaryColorIsSet = false;
                    primaryColor = Color.white;
                }

                if (sourceData.advertAssets.ContainsKey("secondary_color") &&
                    !string.IsNullOrWhiteSpace(sourceData.advertAssets["secondary_color"].value) &&
                    ColorUtility.TryParseHtmlString(sourceData.advertAssets["secondary_color"].value, out temp))
                {
                    secondaryColorIsSet = true;
                    secondaryColor = temp;
                }
                else
                {
                    secondaryColorIsSet = false;
                    secondaryColor = Color.white;
                }
            }
            else
            {
                url = "";
                urlIsSet = false;
                logoUrl = "";
                logoUrlIsSet = false;
                roundText = "";
                roundTextIsSet = false;
                promoImageUrl = "";
                promoImageUrlIsSet = false;
                primaryColor = Color.white;
                primaryColorIsSet = false;
                secondaryColor = Color.white;
                secondaryColorIsSet = false;
            }
        }
    }

    public delegate void AdvertEvent(AdvertisementComponent advertComponent);

    [System.Serializable]
    public class AdvertisementComponent : CoinModeComponent
    {        
        public bool advertDataAvailable { get; private set; } = false;        
        public DefaultAdvertAssets defaultAssets { get; private set; } = new DefaultAdvertAssets();

        public AdvertEvent onAdvertDataUpdated = null;

        private ReadOnlyDictionary<string, AdvertData> _advertData = null;

        internal AdvertisementComponent() { }

        internal void SetCurrentAdvertData(ReadOnlyDictionary<string, AdvertData> advertData)
        {
            if (advertData != null)
            {
                _advertData = advertData;
                advertDataAvailable = advertData != null && advertData.Count > 0;
                if(advertDataAvailable && advertData.ContainsKey("main_advertiser"))
                {
                    defaultAssets = new DefaultAdvertAssets(advertData["main_advertiser"]);
                }
                else
                {
                    defaultAssets = new DefaultAdvertAssets();
                }
            }
            else
            {
                advertDataAvailable = false;
                advertData = null;
            }            
            onAdvertDataUpdated?.Invoke(this);
        }

        internal void ClearCurrentAdvertData()
        {
            advertDataAvailable = false;
            defaultAssets = new DefaultAdvertAssets();
            _advertData = null;
            onAdvertDataUpdated?.Invoke(this);
        }

        public bool TryGetAdvertisersData(string advertiser, out AdvertData data)
        {
            data = null;
            return _advertData != null && _advertData.TryGetValue(advertiser, out data);
        }

        public bool TryGetFirstAdvertAsset(string assetKey, out AdvertAsset asset)
        {
            bool found = false;
            asset = null;
            if(_advertData != null)
            {
                foreach (KeyValuePair<string, AdvertData> ad in _advertData)
                {
                    foreach (KeyValuePair<string, AdvertAsset> adAsset in ad.Value.advertAssets)
                    {
                        if (adAsset.Key == assetKey)
                        {
                            found = true;
                            asset = adAsset.Value;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }            
            return found;
        }

        public bool TryGetFirstAdvertAssets(string assetKey, out List<AdvertAsset> assets)
        {
            bool found = false;
            assets = new List<AdvertAsset>();
            if(_advertData != null)
            {
                foreach (KeyValuePair<string, AdvertData> ad in _advertData)
                {
                    foreach (KeyValuePair<string, AdvertAsset> adAsset in ad.Value.advertAssets)
                    {
                        if (adAsset.Key == assetKey)
                        {
                            assets.Add(adAsset.Value);
                        }
                    }
                }
            }            
            return found;
        }
    }
}

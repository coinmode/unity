using LightJson;
using System.Collections.Generic;
using UnityEngine;

namespace CoinMode
{
    public class AdvertAsset : CoinModeProperties
    {
        public string key { get; private set; } = null;
        public string type { get; private set; } = null;
        public string value { get; private set; } = null;
        public string description { get; private set; } = null;

        internal AdvertAsset() { }

        internal override void FromJson(JsonObject json)
        {
            key = json["key"];
            type = json["type"];
            value = (!string.IsNullOrWhiteSpace(type) && type == "color") ? json["color"] : json["value"];
            description = json["description"];
        }
    }

    public class AdvertData : CoinModeProperties
    {
        public Dictionary<string, AdvertAsset> advertAssets { get; private set; } = new Dictionary<string, AdvertAsset>();

        internal AdvertData() { }

        internal override void FromJson(JsonObject json)
        {
            JsonObject allAssets = json["advert_data"];
            if (allAssets != null)
            {
                foreach (KeyValuePair<string, JsonValue> pair in allAssets)
                {
                    AdvertAsset asset = new AdvertAsset();
                    asset.FromJson(pair.Value);
                    advertAssets.Add(pair.Key, asset);
                }
            }
        }
    }
}

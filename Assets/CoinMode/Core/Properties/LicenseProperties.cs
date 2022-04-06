using LightJson;
using System.Collections.Generic;

namespace CoinMode
{
    [System.Serializable]
    public class LicenseProperties : CoinModeProperties
    {
        public string licenseId { get; private set; } = null;
        public string licenseType { get; private set; } = null;
        public int? version { get; private set; } = null;
        public string[] countries { get; private set; } = null;
        public string countryUsed { get; private set; } = null;
        public string queries { get; private set; } = null;
        public string contractText { get; private set; } = null;
        public int? epochCreated { get; private set; } = null;
        public bool? isActive { get; private set; } = null;
        public string description { get; private set; } = null;

        internal override void FromJson(JsonObject json)
        {
            licenseId = json["license_id"];
            licenseType = json["license_type"];
            version = json["version"];
            JsonValue countriesVal = json["countries"];
            if (!countriesVal.IsNull)
            {
                if (countriesVal.IsJsonArray)
                {
                    JsonArray array = countriesVal.AsJsonArray;
                    countries = new string[array.Count];
                    for (int i = 0; i < array.Count; i++)
                    {
                        countries[i] = array[i].AsString;
                    }
                }
                else if (countriesVal.IsJsonObject)
                {
                    JsonObject obj = countriesVal.AsJsonObject;
                    countries = new string[obj.Count];
                    int index = 0;
                    foreach (JsonValue val in obj as IEnumerable<JsonValue>)
                    {
                        countries[index] = val.AsString;
                        index++;
                    }
                }
            }
            countryUsed = json["country_used"];
            queries = json["queries"];
            contractText = json["contract_text"];
            epochCreated = json["epoch_created"];
            isActive = json["is_active"];
            description = json["developer_description"];
        }
    }
}

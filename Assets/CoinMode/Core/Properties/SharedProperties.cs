using LightJson;

namespace CoinMode
{
    [System.Serializable]
    public class VerificationMethodProperties : CoinModeProperties
    {
        public string field { get; private set; } = null;
        public string userText { get; private set; } = null;
        public string type { get; private set; } = null;
        public int? maxLength { get; private set; } = null;

        internal VerificationMethodProperties() { }

        internal override void FromJson(JsonObject json)
        {
            field = json["field"];
            userText = json["usertext"];
            type = json["type"];
            maxLength = json["max_length"];
        }
    }
}

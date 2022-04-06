using LightJson;

namespace CoinMode.NetApi
{
    public class CoinModeResponse
    {
        public string status { get; private set; }
        public float duration { get; private set; }
        public JsonObject jsonObject { get; private set; }

        internal CoinModeResponse() { }

        internal virtual void FromJson(JsonObject json)
        {
            jsonObject = json;
            status = json["status"];
            duration = json["duration"];
        }
    }    

    public class CoinModeErrorResponse : CoinModeResponse
    {
        public bool networkError { get; private set; }
        public bool httpError { get; private set; }
        public string coinModeError { get; private set; }
        public string userMessage { get; private set; }
        public JsonObject details { get; private set; }

        public string GetErrorString()
        {
            return !string.IsNullOrWhiteSpace(coinModeError) ? string.Format("{0} {1}", coinModeError, userMessage) : userMessage;
        }

        internal CoinModeErrorResponse() { }

        internal override void FromJson(JsonObject json)
        {
            base.FromJson(json);
            this.coinModeError = json["error"];
            this.userMessage = json["user_message"];
            this.details = json["details"];
        }

        internal void FromError(bool networkError, bool httpError, string message)
        {
            this.networkError = networkError;
            this.httpError = httpError;
            this.userMessage = message;
        }
    }
}

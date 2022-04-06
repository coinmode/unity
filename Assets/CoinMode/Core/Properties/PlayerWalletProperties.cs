using LightJson;

namespace CoinMode
{
    [System.Serializable]
    public class WalletBalances : CoinModeProperties
    {
        public double? confirmed { get; private set; } = null;
        public double? pending { get; private set; } = null;

        internal WalletBalances() { }

        internal override void FromJson(JsonObject json)
        {
            confirmed = json["confirmed"];
            pending = json["pending"];
        }
    }
}

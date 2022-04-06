using LightJson;

namespace CoinMode
{
    public abstract class CoinModeProperties
    {
        internal abstract void FromJson(JsonObject json);
    }
}

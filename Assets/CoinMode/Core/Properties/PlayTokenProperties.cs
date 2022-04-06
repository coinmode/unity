using LightJson;

namespace CoinMode
{
    [System.Serializable]
    public class PlayTokenStatus : CoinModeProperties
    {
        public bool? isActive { get; private set; } = null;
        public bool? isValid { get; private set; } = null;
        public bool? wasActivated { get; private set; } = null;
        public bool? wasCancelled { get; private set; } = null;
        public bool? hasExpired { get; private set; } = null;
        public bool? sessionsRemaining { get; private set; } = null;

        internal PlayTokenStatus() { }

        internal override void FromJson(JsonObject json)
        {
            isActive = json["is_active"];
            isValid = json["is_valid"];
            wasActivated = json["was_activated"];
            wasCancelled = json["was_cancelled"];
            hasExpired = json["had_expired"];
            sessionsRemaining = json["sessions_remaining"];
        }
    }
}

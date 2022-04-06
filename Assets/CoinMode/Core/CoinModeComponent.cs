namespace CoinMode
{
    public abstract class CoinModeComponent
    {
        protected bool IsPlayerAndPlayTokenValid(PlayerComponent player)
        {
            return player != null && !string.IsNullOrEmpty(player.playToken);
        }
    }
}

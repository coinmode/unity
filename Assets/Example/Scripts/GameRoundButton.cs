using CoinMode;

public class GameRoundButton : GameButton
{
    public delegate void GameRoundButtonEvent(GameRoundButton button);

    public MinimalRoundInfo roundInfo { get; private set; } = null;

    private GameRoundButtonEvent onClick = null;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    protected override void SetSpinnerVisibility(bool visible)
    {
        base.SetSpinnerVisibility(visible);
    }

    public void SetInfo(GameRoundButtonEvent onClickedEvent, MinimalRoundInfo info)
    {
        onClick = onClickedEvent;
        roundInfo = info;
        string name = string.IsNullOrEmpty(roundInfo.name) ? roundInfo.roundId : roundInfo.name;
        SetMainButtonText(name);
    }

    private void OnClick()
    {
        onClick?.Invoke(this);
    }
}

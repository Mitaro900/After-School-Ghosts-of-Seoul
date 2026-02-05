public enum GameState
{
    Boot,
    Title,
    InGame,
    Pause,
    GameOver
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        CurrentState = GameState.Boot;
    }
}

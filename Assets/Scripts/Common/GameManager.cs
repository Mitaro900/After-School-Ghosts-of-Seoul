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


    private Player player;




    protected override void Awake()
    {
        base.Awake();
        CurrentState = GameState.Boot;  // 로딩 상태로 변환
    }


    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
    }


    public void SetPlayer(Player p)
    {
        player = p;
        CameraManager.Instance.FollowCamera(player.transform);
    }
}

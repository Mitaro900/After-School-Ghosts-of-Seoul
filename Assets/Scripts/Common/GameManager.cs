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
        // 임시로 Player Awke에서 사용중 (InGame State로 넘어갈시 플레이어쪽으로 가게하기)
        player = p;
        CameraManager.Instance.FollowCamera(player.transform);
    }
}

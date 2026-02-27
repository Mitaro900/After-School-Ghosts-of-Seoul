using UnityEngine;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] private GameObject m_NewGameBtn;

    private void Start()
    {
        UIManager.Instance.Fade(Color.black, 1f, 0f, 0.5f, 0f, true);
    }

    public void OnClickNewGameButton()
    {
        // 새로운 게임 시작
        //UIManager.Instance.Fade(Color.black, 0f, 1f, 0.5f, 0f, false, () =>
        //{
        //    SceneLoader.Instance.LoadScene(SceneType.InGame);
        //});

        SceneLoader.Instance.LoadScene(SceneType.InGame);
    }

    public void OnClickQuitButton()
    {
        UIManager.Instance.Fade(Color.black, 0f, 1f, 0.5f, 0f, false, () =>
        {
            GameManager.Instance.QuitGame();
        });
    }
}

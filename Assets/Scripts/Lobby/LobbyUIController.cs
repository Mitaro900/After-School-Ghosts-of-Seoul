using UnityEngine;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] private GameObject m_NewGameBtn;

    private void Start()
    {
        AudioManager.Instance.PlayBGM(Music.lobby);
        UIManager.Instance.Fade(Color.black, 1f, 0f, 0.5f, 0f, true);
    }

    public void OnClickNewGameButton()
    {
        AudioManager.Instance.StopBGM();
        SceneLoader.Instance.LoadScene(SceneType.Intro);
    }

    public void OnClickQuitButton()
    {
        UIManager.Instance.Fade(Color.black, 0f, 1f, 0.5f, 0f, false, () =>
        {
            GameManager.Instance.QuitGame();
        });
    }
}

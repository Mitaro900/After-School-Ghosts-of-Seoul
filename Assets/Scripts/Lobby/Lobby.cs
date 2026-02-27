using Unity.VisualScripting;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    private void Awake()
    {
        AudioManager.Instance.PlayBGM(Music.배경음악1);
    }

    public void OnClickStart()
    {
        // 인게임 씬으로 이동
        AudioManager.Instance.StopBGM();
        SceneLoader.Instance.LoadScene(SceneType.InGame);
    }
}

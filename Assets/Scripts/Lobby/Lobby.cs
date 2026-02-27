using Unity.VisualScripting;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    public void OnClickStart()
    {
        // 인게임 씬으로 이동
        SceneLoader.Instance.LoadScene(SceneType.InGame);
    }
}

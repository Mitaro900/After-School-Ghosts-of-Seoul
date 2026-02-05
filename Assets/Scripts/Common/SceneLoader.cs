
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    // 씬을 전환 합니다.
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

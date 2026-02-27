using Singleton.Component;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    Lobby,
    Title,
    Intro,
    InGame,
    Ending
}
public class SceneLoader : SingletonComponent<SceneLoader>
{
    #region Singleton
    protected override void AwakeInstance()
    {

    }

    protected override bool InitInstance()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        return true;
    }

    protected override void ReleaseInstance()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    public AsyncOperation LoadSceneAsync(SceneType sceneType)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneType.ToString());
        return asyncOp;
    }

    public void LoadScene(SceneType sceneType)
    {
        // 씬 로딩 코루틴 바로 시작
        StartCoroutine(FadeOutThenLoad(sceneType));
    }

    private IEnumerator FadeOutThenLoad(SceneType sceneType)
    {
        // 1. 버튼 누르고 살짝 텀
        yield return new WaitForSeconds(0.4f);

        // 2. 페이드 아웃 + 검정 유지
        yield return FadeSceneOut(0.5f);

        // 3. 씬 로딩
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneType.ToString());

        // 4. 씬 로딩 끝날 때까지 대기
        while (!asyncOp.isDone)
            yield return null;
    }

    public IEnumerator FadeSceneOut(float holdDuration = 0.5f)
    {
        // 페이드 아웃
        UIManager.Instance.Fade(Color.black, 0f, 1f, 0.5f, 0f, false);

        // 검정 화면 유지
        yield return new WaitForSeconds(holdDuration);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIManager.Instance.Fade(Color.black, 1f, 0f, 0.5f, 0f, true);
    }
}
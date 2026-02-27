using Singleton.Component;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SceneType
{
    Lobby,
    Title,
    InGame,
    Ending
}
public class SceneLoader : SingletonComponent<SceneLoader>
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    public Image FadeImage => fadeImage;

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

        CutsceneController.Instance.PlayCutscene(CutsceneType.Intro);
    }

    private IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
        Color color = fadeImage.color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, time / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = to;
        fadeImage.color = color;

        if (to == 0f)
            fadeImage.gameObject.SetActive(false);
    }

    public IEnumerator FadeSceneOut(float holdDuration = 0.5f)
    {
        if (fadeImage == null) yield break;

        // 페이드 아웃
        yield return Fade(0, 1);

        // 검정 화면 유지
        yield return new WaitForSeconds(holdDuration);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeSceneIn());
    }

    public IEnumerator FadeSceneIn()
    {
        yield return Fade(1, 0);
    }
}
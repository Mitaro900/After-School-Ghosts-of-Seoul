using Singleton.Component;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum CutsceneType
{
    Intro,
    Ending
}

public class CutsceneController : SingletonComponent<CutsceneController>
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private GameObject opCanvas;
    [SerializeField] private GameObject edCanvas;
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayableAsset introTimeline;
    [SerializeField] private PlayableAsset endingTimeline;

    private CutsceneType currentType;
    private bool isPlaying = false;

    #region Singleton
    protected override void AwakeInstance()
    {
        
    }

    protected override bool InitInstance()
    {
        return true;
    }

    protected override void ReleaseInstance()
    {
        
    }
    #endregion

    void Update()
    {
        if (isPlaying && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Skip();
        }
    }

    public void PlayCutscene(CutsceneType type)
    {
        if (isPlaying) return;

        isPlaying = true;
        opCanvas.SetActive(true);
        edCanvas.SetActive(true);

        // 타임라인 선택
        director.playableAsset =
            (type == CutsceneType.Intro) ? introTimeline : endingTimeline;
        currentType = type;
        director.Play();
        director.stopped += OnCutsceneEnd;
    }

    private void OnCutsceneEnd(PlayableDirector obj)
    {
        Finish();
    }

    private void Skip()
    {
        fadeImage.gameObject.SetActive(true);
        director.time = director.duration;
        director.Evaluate();
        director.Stop();
        Finish();
    }

    private void Finish()
    {
        FinishRoutine();
    }

    private void FinishRoutine()
    {
        isPlaying = false;

        // 이벤트 중복 방지
        director.stopped -= OnCutsceneEnd;

        // 컷씬 정리
        AudioManager.Instance.PlayBGM(Music.배경음악2);

        fadeImage.gameObject.SetActive(false);
        opCanvas.SetActive(false);
        edCanvas.SetActive(false);
        if (currentType == CutsceneType.Intro)
        {
            SceneLoader.Instance.LoadScene(SceneType.InGame);
        }
        else
        {
            SceneLoader.Instance.LoadScene(SceneType.Ending);
        }
    }

    //private IEnumerator Fade(float start, float end)
    //{
    //    float time = 0f;
    //    Color color = fadeImage.color;

    //    while (time < fadeDuration)
    //    {
    //        time += Time.deltaTime;
    //        float alpha = Mathf.Lerp(start, end, time / fadeDuration);

    //        color.a = alpha;
    //        fadeImage.color = color;

    //        yield return null;
    //    }

    //    color.a = end;
    //    fadeImage.color = color;
    //}
}
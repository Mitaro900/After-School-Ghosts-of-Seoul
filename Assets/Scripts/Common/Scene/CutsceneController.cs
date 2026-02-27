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

        opCanvas.SetActive(false);
        edCanvas.SetActive(false);
        if (currentType == CutsceneType.Intro)
        {
            SceneLoader.Instance.LoadScene(SceneType.InGame);
            AudioManager.Instance.PlayBGM(Music.ingame);
        }
        else
        {
            SceneLoader.Instance.LoadScene(SceneType.Ending);
            AudioManager.Instance.PlayBGM(Music.ending);
        }
    }
}
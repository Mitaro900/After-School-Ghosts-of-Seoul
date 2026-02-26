using Singleton.Component;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public enum CutsceneType
{
    Intro,
    Ending
}

public class CutsceneController : SingletonComponent<CutsceneController>
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayableAsset introTimeline;
    [SerializeField] private PlayableAsset endingTimeline;

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

        // 플레이어 움직임 막기
        GameManager.Instance.Player.PlayerMove(false);

        // 타임라인 선택
        director.playableAsset =
            (type == CutsceneType.Intro) ? introTimeline : endingTimeline;

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
        isPlaying = false;

        // 플레이어 다시 활성화
        GameManager.Instance.Player.PlayerMove(true);
    }
}
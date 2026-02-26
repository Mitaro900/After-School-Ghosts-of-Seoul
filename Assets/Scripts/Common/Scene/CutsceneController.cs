using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public enum CutsceneType
{
    Intro,
    Ending
}

public class CutsceneController : Singleton<CutsceneController>
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayableAsset introTimeline;
    [SerializeField] private PlayableAsset endingTimeline;
    [SerializeField] private Player player; // 플레이어 스크립트

    private bool isPlaying = false;

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
        player.PlayerMove(false);

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
        player.PlayerMove(true);
    }
}
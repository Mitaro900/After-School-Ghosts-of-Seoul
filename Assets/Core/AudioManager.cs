using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] AudioSource bgm;   // 브금
    [SerializeField] AudioSource sfx;   // 효과음


    // 배경음악을 재생합니다.
    public void PlayerBGM(AudioClip clip)
    {
        if (bgm.clip == clip)  // 지금 나오는 bgm과 같다면 실행 안함
        return;

        bgm.clip = clip;
        bgm.loop = true;
        bgm.Play();
    }


    // 효과음을 한 번 재생합니다.
    public void PlaySFX(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }
}

using Singleton.Component;
using System.Collections.Generic;
using UnityEngine;

public enum Music
{
    배경음악1,
    배경음악2,
    배경음악3,
    배경음악4,
    배경음악5,
    COUNT
}

public enum SFX
{
    COUNT
}

public class AudioManager : SingletonComponent<AudioManager>
{
    public Transform MusicTrs;
    public Transform SFXTrs;

    private const string AUDIO_PATH = "Audio";

    private Dictionary<Music, AudioSource> m_MusicPlayer = new Dictionary<Music, AudioSource>();
    private AudioSource m_CurrMusicSource;

    private Dictionary<SFX, AudioSource> m_SFXPlayer = new Dictionary<SFX, AudioSource>();

    private float masterVolume = 0.5f;
    private float musicVolume = 0.5f;
    private float sfxVolume = 0.5f;

    [SerializeField] private GameObject VolumeSetting;
    private bool openSetting = true;

    #region Singleton
    protected override void AwakeInstance()
    {
        LoadBGMPlayer();
        LoadSFXPlayer();

        ApplyVolumes();   // ⭐ 초기 50% 적용
    }

    protected override bool InitInstance()
    {
        return true;
    }

    protected override void ReleaseInstance()
    {

    }
    #endregion

    private void LoadBGMPlayer()
    {
        for (int i = 0; i < (int)Music.COUNT; i++)
        {
            var audioName = ((Music)i).ToString();
            var pathStr = $"{AUDIO_PATH}/{audioName}";
            var audioClip = Resources.Load(pathStr, typeof(AudioClip)) as AudioClip;
            if (!audioClip)
            {
                Debug.LogError($"{audioName} clip does not exist.");
                continue;
            }

            var newGO = new GameObject(audioName);
            var newAudioSource = newGO.AddComponent<AudioSource>();
            newAudioSource.clip = audioClip;
            newAudioSource.loop = true;
            newAudioSource.playOnAwake = false;
            newGO.transform.parent = MusicTrs;

            m_MusicPlayer[(Music)i] = newAudioSource;
        }
    }

    private void LoadSFXPlayer()
    {
        for (int i = 0; i < (int)SFX.COUNT; i++)
        {
            var audioName = ((SFX)i).ToString();
            var pathStr = $"{AUDIO_PATH}/{audioName}";
            var audioClip = Resources.Load(pathStr, typeof(AudioClip)) as AudioClip;
            if (!audioClip)
            {
                Debug.LogError($"{audioName} clip does not exist.");
                continue;
            }

            var newGO = new GameObject(audioName);
            var newAudioSource = newGO.AddComponent<AudioSource>();
            newAudioSource.clip = audioClip;
            newAudioSource.loop = false;
            newAudioSource.playOnAwake = false;
            newGO.transform.parent = SFXTrs;

            m_SFXPlayer[(SFX)i] = newAudioSource;
        }
    }

    public void OnLoadUserData()
    {
        var userSettingsData = UserDataManager.Instance.GetUserData<UserSettingsData>();
        if (userSettingsData != null)
        {
            MuteMusic(userSettingsData.Settings.Music_Mute);
            MuteSFX(userSettingsData.Settings.SFX_Mute);
        }
    }

    public void PlayBGM(Music bgm)
    {
        if (m_CurrMusicSource)
        {
            m_CurrMusicSource.Stop();
            m_CurrMusicSource = null;
        }

        if (!m_MusicPlayer.ContainsKey(bgm))
        {
            Debug.LogError($"Invalid clip name. {bgm}");
            return;
        }

        m_CurrMusicSource = m_MusicPlayer[bgm];
        m_CurrMusicSource.Play();
    }

    public void PauseBGM()
    {
        if (m_CurrMusicSource) m_CurrMusicSource.Pause();
    }

    public void ResumeBGM()
    {
        if (m_CurrMusicSource) m_CurrMusicSource.UnPause();
    }

    public void StopBGM()
    {
        if (m_CurrMusicSource) m_CurrMusicSource.Stop();
    }

    public void PlaySFX(SFX sfx)
    {
        if (!m_SFXPlayer.ContainsKey(sfx))
        {
            Debug.LogError($"Invalid clip name. ({sfx})");
            return;
        }

        m_SFXPlayer[sfx].Play();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void MuteMusic(bool _mute)
    {
        foreach (var audioSourceItem in m_MusicPlayer)
        {
            audioSourceItem.Value.mute = _mute;
        }
    }

    public void MuteSFX(bool _mute)
    {
        foreach (var audioSourceItem in m_SFXPlayer)
        {
            audioSourceItem.Value.mute = _mute;
        }
    }

    private void ApplyVolumes()
    {
        float finalMusic = masterVolume * musicVolume;
        float finalSFX = masterVolume * sfxVolume;

        foreach (var item in m_MusicPlayer)
        {
            item.Value.volume = finalMusic;
        }

        foreach (var item in m_SFXPlayer)
        {
            item.Value.volume = finalSFX;
        }
    }

    public void OnVolumesSetting()
    {
        if (openSetting)
        {
            VolumeSetting.SetActive(true);
            openSetting = false;
        }
        else
        {
            VolumeSetting.SetActive(false);
            openSetting = true;
        }
    }
}

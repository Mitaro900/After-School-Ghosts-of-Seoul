using UnityEngine;
using UnityEngine.UI;

public class SoundSliderUI : MonoBehaviour
{
    public enum VolumeType
    {
        Master,
        Music,
        SFX
    }

    public VolumeType volumeType;
    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnValueChanged(float value)
    {
        switch (volumeType)
        {
            case VolumeType.Master:
                AudioManager.Instance.SetMasterVolume(value);
                break;

            case VolumeType.Music:
                AudioManager.Instance.SetMusicVolume(value);
                break;

            case VolumeType.SFX:
                AudioManager.Instance.SetSFXVolume(value);
                break;
        }
    }
}
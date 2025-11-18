
using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_SoundSlider : MonoBehaviour
{
    [Serializable]
    public enum SliderType
    {
        Music,
        FX
    }
    public SliderType sliderType;
    public Slider slider;
    void Start()
    {
        switch (sliderType)
        {
            case SliderType.Music:
                slider.value = SoundManager.Instance.musicVolume;
                slider.onValueChanged.AddListener((value) =>
                {
                    SoundManager.Instance.SetMusicVolume(value);
                });
                break;
            case SliderType.FX:
                slider.value = SoundManager.Instance.fxVolume;
                slider.onValueChanged.AddListener((value) =>
                {
                    SoundManager.Instance.SetFXVolume(value);
                });
                break;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer mainMixer;
    
    
    public void SetMasterVolume(float level)
    {
        mainMixer.SetFloat("masterVolume", Mathf.Log10(level) * 20);
    }
    
    public void SetSoundFXVolume(float level)
    {
        mainMixer.SetFloat("soundFXVolume", Mathf.Log10(level) * 20);
    }

    public void SetMusicVolume(float level)
    {
        mainMixer.SetFloat("musicVolume", Mathf.Log10(level) * 20);
    }
}

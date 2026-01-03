using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO _audioClipRefsSo;

    private AudioSource soundFXObject;
    public static SoundFXManager Instance;
    private AudioSource _quitSelectSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // MAIN MENU EVENTS
        PlayButtonEvents.OnPlayButtonSelected += PlayPlayButtonSelectSFX;
        PlayButtonEvents.OnPlayButtonSubmitted += PlayPlayButtonSubmitSFX;
        QuitButtonEvents.OnQuitButtonSelected += PlayQuitButtonSelectSFX;
        QuitButtonEvents.OnQuitButtonSubmitted += PlayQuitButtonSubmitSFX;
        QuitButtonEvents.OnQuitButtonDeselected += StopQuitButtonSelectSFX;
        SettingsButtonEvents.OnSettingsButtonSelected += PlaySettingsButtonSelectSFX;
        SettingsButtonEvents.OnSettingsButtonSubmitted += PlaySettingsButtonSubmitSFX;
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSelected += PlayOptionsMenuReturnButtonSelectSFX;
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSubmitted += PlayOptionsMenuReturnButtonSubmitSFX;
        OptionMenuSliderEvents.OnSliderSlideRight += PlayVolumeUpSFX;
        OptionMenuSliderEvents.OnSliderSlideLeft += PlayVolumeDownSFX;
        UIController.OnOptionMenuBackButtonPressed += PlayOptionsMenuReturnButtonSubmitSFX;
        PauseMenu.OnPauseSFXEvent += PlayPauseMenuOpenSFX;
        PauseMenu.OnResumeSFXEvent += PlayPauseMenuCloseSFX;
    }

    private void OnDestroy()
    {
        // MAIN MENU EVENTS
        PlayButtonEvents.OnPlayButtonSelected -= PlayPlayButtonSelectSFX;
        PlayButtonEvents.OnPlayButtonSubmitted -= PlayPlayButtonSubmitSFX;
        QuitButtonEvents.OnQuitButtonSelected -= PlayQuitButtonSelectSFX;
        QuitButtonEvents.OnQuitButtonSubmitted -= PlayQuitButtonSubmitSFX;
        QuitButtonEvents.OnQuitButtonDeselected -= StopQuitButtonSelectSFX;
        SettingsButtonEvents.OnSettingsButtonSelected -= PlaySettingsButtonSelectSFX;
        SettingsButtonEvents.OnSettingsButtonSubmitted -= PlaySettingsButtonSubmitSFX;
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSelected -= PlayOptionsMenuReturnButtonSelectSFX;
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSubmitted -= PlayOptionsMenuReturnButtonSubmitSFX;
        OptionMenuSliderEvents.OnSliderSlideRight -= PlayVolumeUpSFX;
        OptionMenuSliderEvents.OnSliderSlideLeft -= PlayVolumeDownSFX;
        UIController.OnOptionMenuBackButtonPressed -= PlayOptionsMenuReturnButtonSubmitSFX;
        PauseMenu.OnPauseSFXEvent -= PlayPauseMenuOpenSFX;
        PauseMenu.OnResumeSFXEvent -= PlayPauseMenuCloseSFX;
    }
    
    
    public void PlaySoundFXClip(GameObject audioPrefab, Transform spawnTransform, float volume = 1)
    {
        GameObject go = Instantiate(audioPrefab, spawnTransform.position, Quaternion.identity);
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(go, clipLength);
    }
    
    public void PlayRandomSoundFXClip(GameObject[] audioPrefabs, Transform spawnTransform, float volume = 1)
    {
        int randomIndex = Random.Range(0, audioPrefabs.Length);
        GameObject prefab = audioPrefabs[randomIndex];
        var go = Instantiate(prefab, spawnTransform.position, Quaternion.identity);
        var audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(go, clipLength);
    }

    public void PlayPlayButtonSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.playButtonSelectSFX, Camera.main.transform);
    }
    
    public void PlayPlayButtonSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.playButtonSubmitSFX, Camera.main.transform);
    }
    
    private AudioSource PlayAndReturnSoundFXClip(GameObject audioPrefab, Transform spawnTransform, float volume = 1)
    {
        GameObject go = Instantiate(audioPrefab, spawnTransform.position, Quaternion.identity);
        AudioSource audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(go, clipLength);
        return audioSource;
    }

    
    public void PlayQuitButtonSelectSFX()
    {
        _quitSelectSource = PlayAndReturnSoundFXClip(
            _audioClipRefsSo.exitButtonSelectSFX,
            Camera.main.transform
        );
    }

    public void PlayQuitButtonSubmitSFX()
    {
        PlaySoundFXClip(
            _audioClipRefsSo.exitButtonSubmitSFX,
            Camera.main.transform
        );
    }

    public void StopQuitButtonSelectSFX()
    {
        if (_quitSelectSource != null && _quitSelectSource.isPlaying)
        {
            _quitSelectSource.Stop();
        }
    }
    public void PlaySettingsButtonSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.settingsButtonSelectSFX, Camera.main.transform);
    }
    
    public void PlaySettingsButtonSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.settingsButtonSubmitSFX, Camera.main.transform);
    }
    
    public void PlayOptionsMenuReturnButtonSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.backButtonSelectSFX, Camera.main.transform);
    }
    
    public void PlayOptionsMenuReturnButtonSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.backButtonSubmitSFX, Camera.main.transform);
    }
    
    public void PlayFullScreenOnSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.fullScreeneOnSFX, Camera.main.transform);
    }
    
    public void PlayFullScreenOffSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.fullScreenOffSFX, Camera.main.transform);
    }
    
    public void PlayVolumeUpSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.SoundUpSFX, Camera.main.transform);
    }
    
    public void PlayVolumeDownSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.SoundDownSFX, Camera.main.transform);
    }

    public void PlayPauseMenuOpenSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.pauseMenuOpenSFX, Camera.main.transform);
    }
    
    public void PlayPauseMenuCloseSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.pauseMenuCloseSFX, Camera.main.transform);
    }

}

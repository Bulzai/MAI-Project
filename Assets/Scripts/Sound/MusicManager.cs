using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource mainMenuMusic;
    [SerializeField] AudioSource MainGameMusicStart;
    [SerializeField] AudioSource MainGameMusicLoop;
    [SerializeField] AudioSource MainGameMusicEnd;
    bool cancelIntroToLoop;
    private Coroutine mainGameIntroCoroutine;
    AudioSource _currentlyPlaying;   // last playing music source
    private bool gameIsPaused = false;

    private void Awake()
    {
        GameEvents.OnMenuStateEntered += RestartMainMenuMusic;
        GameEvents.OnScoreStateEntered += RestartMainMenuMusic;
        PauseMenu.OnPauseSFXEvent += PauseMusic;
        PauseMenu.OnResumeSFXEvent += ResumeMusic;

        PlaceItemState.CountDownStarted += StopMainMenuMusic;
        PlaceItemState.CountDownFinished += PlayMainGameMusicStart;
    }

    private void OnDestroy()
    {
        GameEvents.OnMenuStateEntered -= RestartMainMenuMusic;
        GameEvents.OnScoreStateEntered -= RestartMainMenuMusic;
        PlaceItemState.CountDownStarted -= StopMainMenuMusic;
        PlaceItemState.CountDownFinished -= PlayMainGameMusicStart;
        PauseMenu.OnPauseSFXEvent -= PauseMusic;
        PauseMenu.OnResumeSFXEvent -= ResumeMusic;

    }
    
    void UpdateCurrentPlaying()
    {
        // Call this whenever you change music
        _currentlyPlaying = null;

        if (MainGameMusicLoop.isPlaying)
            _currentlyPlaying = MainGameMusicLoop;
        if (MainGameMusicStart.isPlaying)
            _currentlyPlaying = MainGameMusicStart;
        if (mainMenuMusic.isPlaying)
            _currentlyPlaying = mainMenuMusic;
    }

    public void PauseMusic()
    {
        gameIsPaused = true;

        UpdateCurrentPlaying();

        if (_currentlyPlaying != null)
            _currentlyPlaying.Pause();    // keeps current time
    }

    public void ResumeMusic()
    {

        if (_currentlyPlaying != null && !_currentlyPlaying.isPlaying)
        {
            _currentlyPlaying.Play();     // resumes from paused time
        }
        gameIsPaused = false;
    }
    
    private void RestartMainMenuMusic()
    {
        cancelIntroToLoop = true;

        if (MainGameMusicStart.isPlaying)
            StartCoroutine(FadeOutAndStop(MainGameMusicStart, 1.5f));

        if (MainGameMusicLoop.isPlaying)
            StartCoroutine(FadeOutAndStop(MainGameMusicLoop, 1.5f));

        if (mainMenuMusic.isPlaying) return;
        mainMenuMusic.Play();
    }
    
    
    private void StopMainMenuMusic()
    {
        StartCoroutine(FadeOutAndStop(mainMenuMusic, 3f));
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null || !source.isPlaying)
            yield break;

        float startVolume = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;                  // or Time.deltaTime
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume; // reset for next time
    }
    
    /*
    private void PlayMainGameMusicStart()
    {
        // stop any previous intro/loop logic
        if (mainGameIntroCoroutine != null)
        {
            StopCoroutine(mainGameIntroCoroutine);
            mainGameIntroCoroutine = null;
        }
        cancelIntroToLoop = false;

        // make sure loop is stopped & rewound
        MainGameMusicLoop.Stop();
        MainGameMusicLoop.time = 0f;

        MainGameMusicStart.Play();
        mainGameIntroCoroutine = StartCoroutine(PlayLoopWhenIntroDone());
    }*/

    private void PlayMainGameMusicStart()
    {
        if (MainGameMusicStart.isPlaying) return;
            MainGameMusicStart.Play();
    }
    private IEnumerator PlayLoopWhenIntroDone()
    {
        // optional one-frame delay so isPlaying is valid
        yield return null;

        while (MainGameMusicStart != null && MainGameMusicStart.isPlaying && !gameIsPaused)
        {
            if (cancelIntroToLoop)
                yield break; // someone requested stop
            yield return null;
        }

        if (cancelIntroToLoop)
            yield break;

        MainGameMusicLoop.loop = true;
        MainGameMusicLoop.Play();
        mainGameIntroCoroutine = null;
    }


}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioSource mainMenuMusic;
    [SerializeField] AudioSource MainGameMusic;
    [SerializeField] AudioSource InBetweenMusic;

    private void Awake()
    {
        GameEvents.OnMenuStateEntered += RestartMainMenuMusic;
        GameEvents.OnMainGameStateEntered += StopMainMenuMusic;
    }

    private void OnDestroy()
    {
        GameEvents.OnMenuStateEntered -= RestartMainMenuMusic;
        GameEvents.OnMainGameStateEntered -= StopMainMenuMusic;

    }
    
    private void StopAllMusic()
    {

    }

    private void StopMainMenuMusic()
    {
        mainMenuMusic.Stop();
    }

    private void RestartMainMenuMusic()
    {
        if (mainMenuMusic.isPlaying) return;
        mainMenuMusic.Play();
    }
}

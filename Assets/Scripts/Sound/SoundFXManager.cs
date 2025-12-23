using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO _audioClipRefsSo;

    private AudioSource soundFXObject;
    public static SoundFXManager Instance;
    
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

    }

    private void OnDestroy()
    {
        // MAIN MENU EVENTS
        PlayButtonEvents.OnPlayButtonSelected -= PlayPlayButtonSelectSFX;
        PlayButtonEvents.OnPlayButtonSubmitted -= PlayPlayButtonSubmitSFX;
    }
    
    
    public void PlaySoundFXClip(GameObject audioPrefab, Transform spawnTransform, float volume)
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
        PlayRandomSoundFXClip(_audioClipRefsSo.jumpSFX, Camera.main.transform);
    }
    
    public void PlayPlayButtonSubmitSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.jumpSFX, Camera.main.transform);
    }
}

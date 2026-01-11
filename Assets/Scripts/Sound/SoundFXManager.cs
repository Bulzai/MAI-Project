using System.Collections;
using System.Collections.Generic;
using TarodevController;
using UnityEditor;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [SerializeField] private AudioClipRefsSO _audioClipRefsSo;

    private AudioSource soundFXObject;
    public static SoundFXManager Instance;
    private AudioSource _quitSelectSource;
    private AudioSource _bigFLameSource;
    private AudioSource _auraSpawnSource;
    private AudioSource _pointsIncreaseSource;
    
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
        
        
        
        // SETTINGS MENU EVENTS
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSelected += PlayOptionsMenuReturnButtonSelectSFX;
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSubmitted += PlayOptionsMenuReturnButtonSubmitSFX;
        OptionMenuSliderEvents.OnSliderSlideRight += PlayVolumeUpSFX;
        OptionMenuSliderEvents.OnSliderSlideLeft += PlayVolumeDownSFX;
        UIController.OnOptionMenuBackButtonPressed += PlayOptionsMenuReturnButtonSubmitSFX;
        SettingsMenu.OnFullscreenEnabled += PlayFullScreenOnSFX;
        SettingsMenu.OnFullscreenDisabled += PlayFullScreenOffSFX;
        
        
        // PAUSE MENU EVENTS
        PauseMenu.OnPauseSFXEvent += PlayPauseMenuOpenSFX;
        PauseMenu.OnResumeSFXEvent += PlayPauseMenuCloseSFX;
        PauseMenuMenuButtonEvents.OnPauseMenuMenuButtonSelected+= PlayPauseMenuButtonSelectSFX;
        PauseMenuQuitButtonEvents.OnPauseMenuQuitButtonSelected += PlayPauseMenuButtonSelectSFX;
        PauseMenuResumeButtonEvents.OnPauseMenuResumeButtonSelected += PlayPauseMenuButtonSelectSFX;
        
        // PlayerSelect Events
        PlayerManager.OnPlayerJoinedSFX += PlayPlayerJoinedSFX;
        PlayerSelectionManager.OnNotAllPlayersReady += PlayForbiddenSignSFX;
        PlayerSelectionManager.OnNobodyJoinedYet += PlayForbiddenSignSFX;
        PlayerSelectionManager.OnStartGameSFX += PlayGameStartSFX;
        PlayerSelectionManager.OnPlayerReadySFX += PlayPlayerReadySFX;
        
        // Surpriseboxstate Events
        GameEvents.OnSurpriseBoxStateEntered += PlayCountdownSFX;
        SurpriseBoxState.OnSurpriseBoxStateCounterStarted += PlayCountdownSFX;
        CursorController.OnEnableCursor += PlayEnableCursorSFX;
        
        // PLACEITEM STATE EVENTS
        PlaceItemState.CountDownStarted += PlayCountdownSFX;
        PlaceItemState.CountDownStarted += PlayMainGameBigFlameStartSFX;
        CursorController.OnCantPlaceItem += PlayForbiddenSignSFX;
        GridItem.OnRotateItem += PlayRotateItemSFX;
        GridItem.OnGridItemPlaced;

        // Main Game Events
        GameEvents.OnMainGameStateExited += PlayMainGameBigFlameEndSFX;
        PlayerItemHandler.OnRepelAuraActivated;
        PlayerItemHandler.OnDamageAuraActivated;
        PlayerItemHandler.OnOtherPlayerSlowed;
        PlayerItemHandler.OnRepelAuraDeactivated;
        PlayerItemHandler.OnSpeedAuraActivated;
        PlayerHealthSystem.OnMilkCollected;

        // ScoreState Events
        PlayerScoreManager.OnPointsIncrease += PlayPointsIncreaseSFX;
        PlayerScoreManager.OnPointsIncreaseEnd += StopPointsIncreaseSFX;
        
        // CHARACTER EVENTS
        PlayerHealthSystem.OnPlayerTakeDamage += PlayPlayerTakeDamageSFX;
        PlayerHealthSystem.OnPlayerDeath += PlayPlayerDeathSFX;
        PlayerController.OnPlayerJumped += PlayJumpSFX;
        PlayerController.OnPlayerLanded += PlayLandSFX;
        PlayerController.OnPlayerRunning += PlayFootstepSFX;
        
        // Aura Events
        PlayerItemHandler.OnAuraPickedUp += PlayAuraPickUpSFX;
        PlayerItemHandler.OnAuraExpires += PlayAuraExpireSFX;
        SpawnItem.OnAuraSpawns += PlayAuraSpawnSFX;
        GameEvents.OnMainGameStateExited += StopAuraSpawnSFX;

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
        
        // SETTINGS MENU EVENTS
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSelected -= PlayOptionsMenuReturnButtonSelectSFX;
        OptionMenuReturnButtonEvents.OnOptionsMenuReturnButtonSubmitted -= PlayOptionsMenuReturnButtonSubmitSFX;
        OptionMenuSliderEvents.OnSliderSlideRight -= PlayVolumeUpSFX;
        OptionMenuSliderEvents.OnSliderSlideLeft -= PlayVolumeDownSFX;
        UIController.OnOptionMenuBackButtonPressed -= PlayOptionsMenuReturnButtonSubmitSFX;
        SettingsMenu.OnFullscreenEnabled -= PlayFullScreenOnSFX;
        SettingsMenu.OnFullscreenDisabled -= PlayFullScreenOffSFX;
        
        // PAUSE MENU EVENTS
        PauseMenu.OnPauseSFXEvent -= PlayPauseMenuOpenSFX;
        PauseMenu.OnResumeSFXEvent -= PlayPauseMenuCloseSFX;
        
        // PlayerSelect Events
        PlayerManager.OnPlayerJoinedSFX -= PlayPlayerJoinedSFX;
        PlayerSelectionManager.OnNotAllPlayersReady -= PlayForbiddenSignSFX;
        PlayerSelectionManager.OnNobodyJoinedYet -= PlayForbiddenSignSFX;
        PlayerSelectionManager.OnStartGameSFX -= PlayGameStartSFX;
        PlayerSelectionManager.OnPlayerReadySFX -= PlayPlayerReadySFX;

        // Surpriseboxstate Events
        GameEvents.OnSurpriseBoxStateEntered -= PlayCountdownSFX;
        SurpriseBoxState.OnSurpriseBoxStateCounterStarted -= PlayCountdownSFX;
        CursorController.OnEnableCursor -= PlayEnableCursorSFX;

        
        // PLACEITEM STATE EVENTS
        PlaceItemState.CountDownStarted -= PlayCountdownSFX;
        PlaceItemState.CountDownStarted -= PlayMainGameBigFlameStartSFX;
        CursorController.OnCantPlaceItem -= PlayForbiddenSignSFX;
        GridItem.OnRotateItem -= PlayRotateItemSFX;
        
        // Main Game Events
        GameEvents.OnMainGameStateExited -= PlayMainGameBigFlameEndSFX;
        
        // ScoreState Events
        PlayerScoreManager.OnPointsIncrease -= PlayPointsIncreaseSFX;
        PlayerScoreManager.OnPointsIncreaseEnd -= StopPointsIncreaseSFX;        
        
        // CHARACTER EVENTS
        PlayerHealthSystem.OnPlayerTakeDamage -= PlayPlayerTakeDamageSFX;
        PlayerHealthSystem.OnPlayerDeath -= PlayPlayerDeathSFX;
        PlayerController.OnPlayerJumped -= PlayJumpSFX;
        PlayerController.OnPlayerLanded -= PlayLandSFX;
        PlayerController.OnPlayerRunning -= PlayFootstepSFX;
        
        
        
        // Aura Events
        PlayerItemHandler.OnAuraPickedUp -= PlayAuraPickUpSFX;
        PlayerItemHandler.OnAuraExpires -= PlayAuraExpireSFX;
        SpawnItem.OnAuraSpawns -= PlayAuraSpawnSFX;
        GameEvents.OnScoreStateEntered -= StopAuraSpawnSFX;
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

    public void PlayRandomPauseMenuUiSoundFXClip(GameObject[] audioPrefabs, Transform spawnTransform, float volume = 1)
    {
        int randomIndex = Random.Range(0, audioPrefabs.Length);
        GameObject prefab = audioPrefabs[randomIndex];
        var go = Instantiate(prefab, spawnTransform.position, Quaternion.identity);
        var audioSource = go.GetComponent<AudioSource>();
        audioSource.volume = volume;
        audioSource.ignoreListenerPause = true;
        audioSource.Play();
        float clipLength = audioSource.clip.length;
        Destroy(go, clipLength);
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
    
    public void PlayPlayButtonSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.playButtonSelectSFX, Camera.main.transform);
    }
    
    public void PlayPlayButtonSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.playButtonSubmitSFX, Camera.main.transform);
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
            Destroy(_quitSelectSource.gameObject);
            _quitSelectSource = null;
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
    
    public void PlayCountdownSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.countDownGameStartSFX, Camera.main.transform);
    }
    
    public void PlayMainGameBigFlameStartSFX()
    {
        
        _bigFLameSource = PlayAndReturnSoundFXClip(_audioClipRefsSo.bigFlameStartSFX, Camera.main.transform, 0.1f);
        
    }

    public void PlayMainGameBigFlameEndSFX()
    {
        if (_bigFLameSource != null && _bigFLameSource.isPlaying)
        {
            _bigFLameSource.Stop();
            Destroy(_bigFLameSource.gameObject);
            _bigFLameSource = null;
        }
        PlaySoundFXClip(_audioClipRefsSo.bigFlameEndSFX, Camera.main.transform);
    }

    public void PlayPauseMenuButtonSelectSFX()
    {
        PlayRandomPauseMenuUiSoundFXClip(_audioClipRefsSo.buttonSelectSFX, Camera.main.transform);
    }
    
    public void PlayPlayerTakeDamageSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.impactSFX, Camera.main.transform);
        PlayRandomSoundFXClip(_audioClipRefsSo.hurtSFX, Camera.main.transform);
    }
    
    public void PlayPlayerDeathSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.deathSFX, Camera.main.transform);
    }
    
    public void PlayAuraPickUpSFX()
    {
        if (_auraSpawnSource != null && _auraSpawnSource.isPlaying)
        {
            _auraSpawnSource.Stop();
            Destroy(_auraSpawnSource.gameObject);
            _auraSpawnSource = null;
        }
        PlaySoundFXClip(_audioClipRefsSo.auraCollectSFX, Camera.main.transform);
    }
    
    public void PlayAuraExpireSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.auraExpiresSFX, Camera.main.transform);
    }
    
    public void PlayAuraSpawnSFX()
    {
        _auraSpawnSource = PlayAndReturnSoundFXClip(_audioClipRefsSo.auraSpawnSFX, Camera.main.transform, 0.7f);
    }
    
    public void StopAuraSpawnSFX()
    {
        if (_auraSpawnSource != null && _auraSpawnSource.isPlaying)
        {
            _auraSpawnSource.Stop();
        }
    }
    
    public void PlayFootstepSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.runningSFX, Camera.main.transform);
    }

    public void PlayJumpSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.jumpSFX, Camera.main.transform);
    }
    
    public void PlayLandSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.landSFX, Camera.main.transform);
    }
    
    public void PlayPlayerJoinedSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.joinSFX, Camera.main.transform);
    }
    
    public void PlayForbiddenSignSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.forbiddenSignSFX, Camera.main.transform);
    }
    
    public void PlayGameStartSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.startGameSFX, Camera.main.transform);
        PlayRandomSoundFXClip(_audioClipRefsSo.startGameSFX, Camera.main.transform);
        PlayRandomSoundFXClip(_audioClipRefsSo.startGameSFX, Camera.main.transform);
    }
    
    public void PlayPlayerReadySFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.readySFX, Camera.main.transform);
    }
    
    public void PlayPointsIncreaseSFX()
    {
        _pointsIncreaseSource = PlayAndReturnSoundFXClip(_audioClipRefsSo.pointsIncreaseSFX, Camera.main.transform);
    }
    
    public void StopPointsIncreaseSFX()
    {
        if (_pointsIncreaseSource != null && _pointsIncreaseSource.isPlaying)
        {
            _pointsIncreaseSource.Stop();
            Destroy(_pointsIncreaseSource.gameObject);
            _pointsIncreaseSource = null;
        }
    }
    
    public void PlayRotateItemSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.jumpSFX, Camera.main.transform);
    }
    
    public void PlayEnableCursorSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.jumpSFX, Camera.main.transform);
    }
    
}

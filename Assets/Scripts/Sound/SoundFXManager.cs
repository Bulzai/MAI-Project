using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private readonly List<AudioSource> _repelAuraActivatedSources = new List<AudioSource>();
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
        PauseMenuMenuButtonEvents.OnPauseMenuMenuButtonSelected += PlayPauseMenuButtonSelectSFX;
        PauseMenuQuitButtonEvents.OnPauseMenuQuitButtonSelected += PlayPauseMenuButtonSelectSFX;
        PauseMenuResumeButtonEvents.OnPauseMenuResumeButtonSelected += PlayPauseMenuButtonSelectSFX;

        // PlayerSelect Events
        PlayerManager.OnPlayerJoinedSFX += PlayPlayerJoinedSFX;
        PlayerSelectionManager.OnNotAllPlayersReady += PlayForbiddenSignSFX;
        PlayerSelectionManager.OnNobodyJoinedYet += PlayForbiddenSignSFX;
        PlayerSelectionManager.OnStartGameSFX += PlayGameStartSFX;
        PlayerSelectionManager.OnPlayerReadySFX += PlayPlayerReadySFX;
        PlayerSelectionManager.OnSurpriseBoxStateTransitionStarted += PlayTransitionSFX;
        // Surpriseboxstate Events
        GameEvents.OnSurpriseBoxStateEntered += PlayCountdownSFX;
        SurpriseBoxState.OnSurpriseBoxStateCounterStarted += PlayCountdownSFX;
        CursorController.OnEnableCursor += PlayEnableCursorSFX;
        SurpriseBoxState.OnPlayerPickedItem += HandleItemSubmitSfx;
        GridItem.OnPlayerSelecedtItem += PlayPlayerSelecedtItemSFX;

        // PLACEITEM STATE EVENTS
        PlaceItemState.CountDownStarted += PlayCountdownSFX;
        PlaceItemState.CountDownStarted += PlayMainGameBigFlameStartSFX;
        CursorController.OnCantPlaceItem += PlayForbiddenSignSFX;
        GridItem.OnRotateItem += PlayRotateItemSFX;
        GridItem.OnGridItemPlaced += HandleItemSubmitSfx;

        // Main Game Events

        GameEvents.OnMainGameStateExited += PlayMainGameBigFlameEndSFX;
        ExtinguisherPickUp.OnMilkCollected += PlayMilkCollectedSFX;
        PlayerHealthSystem.OnPlayerKnockedBack += PlayPlayerKnockedBackSFX;
        BreakableCracker.OnCrackerBroken += PlayCookieBreakSFX;

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
        PlayerItemHandler.OnRepelAuraActivated += PlayRepelAuraActivatedSFX;
        PlayerItemHandler.OnRepelAuraDeactivated += StopOldestRepelAuraActivatedSFX;
        GameEvents.OnMainGameStateExited += StopAndClearAllRepelAuraActivatedSFX;
        PlayerItemHandler.OnDamageAuraActivated += PlayDamageAuraActivatedSFX;
        PlayerItemHandler.OnOtherPlayerSlowed += PlayOtherPLayerSlowedSFX;
        PlayerItemHandler.OnSpeedAuraActivated += PlaySpeedAuraActivatedSFX;

        // Transition Events
        SurpriseBoxState.OnFireTransitionAnimationStarted += PlayTransitionSFX;

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
        PlayerSelectionManager.OnSurpriseBoxStateTransitionStarted -= PlayTransitionSFX;

        // Surpriseboxstate Events
        GameEvents.OnSurpriseBoxStateEntered -= PlayCountdownSFX;
        SurpriseBoxState.OnSurpriseBoxStateCounterStarted -= PlayCountdownSFX;
        CursorController.OnEnableCursor -= PlayEnableCursorSFX;
        GridItem.OnPlayerSelecedtItem -= PlayPlayerSelecedtItemSFX;


        // PLACEITEM STATE EVENTS
        PlaceItemState.CountDownStarted -= PlayCountdownSFX;
        PlaceItemState.CountDownStarted -= PlayMainGameBigFlameStartSFX;
        CursorController.OnCantPlaceItem -= PlayForbiddenSignSFX;
        GridItem.OnRotateItem -= PlayRotateItemSFX;
        GridItem.OnGridItemPlaced -= HandleItemSubmitSfx;

        // Main Game Events
        GameEvents.OnMainGameStateExited -= PlayMainGameBigFlameEndSFX;
        ExtinguisherPickUp.OnMilkCollected -= PlayMilkCollectedSFX;
        PlayerHealthSystem.OnPlayerKnockedBack -= PlayPlayerKnockedBackSFX;
        BreakableCracker.OnCrackerBroken -= PlayCookieBreakSFX;

        
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
        GameEvents.OnMainGameStateExited -= StopAuraSpawnSFX;
        GameEvents.OnMainGameStateExited -= StopAndClearAllRepelAuraActivatedSFX;
        PlayerItemHandler.OnRepelAuraActivated -= PlayRepelAuraActivatedSFX;
        PlayerItemHandler.OnRepelAuraDeactivated -= StopOldestRepelAuraActivatedSFX;
        PlayerItemHandler.OnDamageAuraActivated -= PlayDamageAuraActivatedSFX;
        PlayerItemHandler.OnOtherPlayerSlowed -= PlayOtherPLayerSlowedSFX;
        PlayerItemHandler.OnSpeedAuraActivated -= PlaySpeedAuraActivatedSFX;

        // Transition Events
        SurpriseBoxState.OnFireTransitionAnimationStarted -= PlayTransitionSFX;

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
        _auraSpawnSource = PlayAndReturnSoundFXClip(_audioClipRefsSo.auraSpawnSFX, Camera.main.transform, 0.1f);
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

    public void PlayTransitionSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.bigFlameTransitionSFX, Camera.main.transform);
    }



    // ITEMS

    private void PlayPlayerSelecedtItemSFX(GameObject item)
    {
        Debug.Log("Item selected: " + item.name);
        var rawName = item.name.Replace("(Clone)", "");
        Debug.Log("raw name: " + rawName);
        switch (rawName)
        {
            case "Cracker":
                PlayCookieSelectSFX();
                break;

            case "Spike":
                PlayBratApfelSubmitSFX();
                break;

            case "Chocolate":
                PlayChocolateSubmitSFX();
                break;

            case "Effect_Shooter":
                PlayCanonSubmitSFX();
                break;

            case "Candle":
                PlayCandleSelectSFX();
                break;

            case "Log":
                PlayLogSelectSFX();
                break;

            case "StickyTile":
                PlayStickyTileSubmitSFX();
                break;

            case "cane":
                PlayCaneSubmitAndSelectSFX();
                break;

        } 
    }
    
    public void PlayCookieSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.cookieSelectSFX, Camera.main.transform);
    }
    
    public void PlayCandleSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.candleSelectSFX, Camera.main.transform);
    }
    public void PlayLogSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.woodBlockSelectSFX, Camera.main.transform);
    }
    private void HandleItemSubmitSfx(GameObject item)
    {
        Debug.Log("Item selected: " + item.name);
        var rawName = item.name.Replace("(Clone)", "");
        Debug.Log("raw name: " + rawName);
        switch (rawName)
        {
            case "Cracker":
                PlayCookieSubmitSFX();
                break;
            

            case "Spike":
                PlayBratApfelSubmitSFX();
                break;

            case "Chocolate":
                PlayChocolateSubmitSFX();
                break;

            case "Effect_Shooter":
                PlayCanonSubmitSFX();
                break;

            case "Candle":
                PlayCandleSubmitSFX();
                break;

            case "Log":
                PlayLogSubmitSFX();
                break;

            case "StickyTile":
                PlayStickyTileSubmitSFX();
                break;

            case "cane":
                PlayCaneSubmitAndSelectSFX();
                break;

        }
    }


    public void PlayCookieSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.cookieSubmitSFX, Camera.main.transform);
    }
    

    public void PlayBratApfelSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.bratApfelSubmitSFX, Camera.main.transform);
    }

    public void PlayChocolateSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.chocolateSubmitSFX, Camera.main.transform);
    }

    public void PlayCanonSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.canonSubmitSFX, Camera.main.transform);
    }

    public void PlayCandleSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.candleSubmitSFX, Camera.main.transform);
    }

    public void PlayLogSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.woodBlockSubmitSFX, Camera.main.transform);
    }

    public void PlayStickyTileSubmitSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.chocolateSubmitSFX, Camera.main.transform);
    }

    public void PlayCaneSubmitAndSelectSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.caneSubmitAndSelectSFX, Camera.main.transform);
    }

    public void PlayPlayerKnockedBackSFX()
    {
        PlayRandomSoundFXClip(_audioClipRefsSo.knockBackSFX, Camera.main.transform);
    }

    public void PlayMilkCollectedSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.milkCollectSFX, Camera.main.transform);

    }

    public void PlayMilkSpawnSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.milkSpawnSFX, Camera.main.transform);

    }

    public void PlayRepelAuraActivatedSFX()
    {
        var src = PlayAndReturnSoundFXClip(
            _audioClipRefsSo.repelAuraSFX,
            Camera.main.transform, 0.6f);

        if (src != null)
        {
            _repelAuraActivatedSources.Add(src);
        }
    }

    public void StopOldestRepelAuraActivatedSFX()
    {
        if (_repelAuraActivatedSources.Count == 0)
            return;

        var oldest = _repelAuraActivatedSources[0];

        if (oldest != null)
        {
            oldest.Stop();
            Destroy(oldest.gameObject);
        }

        _repelAuraActivatedSources.RemoveAt(0);
    }

    public void StopAndClearAllRepelAuraActivatedSFX()
    {
        foreach (var src in _repelAuraActivatedSources)
        {
            if (src != null)
            {
                src.Stop();
                Destroy(src.gameObject);
            }
        }

        _repelAuraActivatedSources.Clear();
    }

    public void PlayDamageAuraActivatedSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.poisonAuraCollectSFX, Camera.main.transform);
    }

    public void PlayOtherPLayerSlowedSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.slowAuraHitSFX, Camera.main.transform);
    }

    public void PlaySpeedAuraActivatedSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.speedAuraCollectSFX, Camera.main.transform);
    }
    
    public void PlayCookieBreakSFX()
    {
        PlaySoundFXClip(_audioClipRefsSo.cookieBreakSFX, Camera.main.transform);
    }
}

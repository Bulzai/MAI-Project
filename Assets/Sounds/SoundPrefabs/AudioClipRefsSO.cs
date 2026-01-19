using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject
{
    [Header("Main Menu SFX")]
    public GameObject playButtonSelectSFX;
    public GameObject playButtonSubmitSFX;
    public GameObject exitButtonSelectSFX;
    public GameObject exitButtonSubmitSFX;
    public GameObject settingsButtonSelectSFX;
    public GameObject settingsButtonSubmitSFX;
    
    [Header("Settings Menu SFX")]
    public GameObject backButtonSelectSFX;
    public GameObject backButtonSubmitSFX;
    public GameObject fullScreeneOnSFX;
    public GameObject fullScreenOffSFX;
    public GameObject[] SoundUpSFX;
    public GameObject[] SoundDownSFX;

    
    [Header("Character SFX")]
    public GameObject[] jumpSFX;
    public GameObject[] landSFX;
    public GameObject[] runningSFX;
    public GameObject[] hurtSFX;
    public GameObject[] deathSFX;
    public GameObject[] joinSFX;
    public GameObject[] readySFX;
    public GameObject[] startGameSFX;



    [Header("Item SFX")] 
    public GameObject caneSubmitAndSelectSFX;
    public GameObject[] knockBackSFX;
    public GameObject cookieSelectSFX;
    public GameObject cookieSubmitSFX;
    public GameObject cookieBreakSFX;
    
    public GameObject woodBlockSelectSFX;
    public GameObject woodBlockSubmitSFX;
    
    public GameObject bratApfelSelectSFX;
    public GameObject bratApfelSubmitSFX;
    public GameObject[] bratApfelHitSFX;
    
    public GameObject chocolateSelectSFX;
    public GameObject chocolateSubmitSFX;
    public GameObject chocolateHitSFX;
    
    public GameObject canonSelectSFX;
    public GameObject canonSubmitSFX;
    public GameObject canonShootSFX;
    
    public GameObject candleSelectSFX;
    public GameObject candleSubmitSFX;
    public GameObject candleIgniteSFX;

    public GameObject iceSelectSFX;
    public GameObject iceSubmitSFX;
    
    public GameObject forbiddenSignSFX;

    public GameObject[] impactSFX;
    public GameObject milkSpawnSFX;
    public GameObject milkCollectSFX;
    
    [Header("Aura SFX")]
    public GameObject auraExpiresSFX;
    public GameObject auraHitSFX;
    public GameObject auraCollectSFX;

    public GameObject poisonAuraCollectSFX;
    public GameObject speedAuraCollectSFX;
    public GameObject slowAuraHitSFX;
    public GameObject repelAuraSFX;
    public GameObject confusionAuraSFX;
    public GameObject auraSpawnSFX;
    
    
    [Header("Final Score SFX")]
    public GameObject pointsIncreaseSFX;
    
    [Header("Main Game SFX")]
    public GameObject bigFlameStartSFX;
    public GameObject bigFlameEndSFX;
    public GameObject countdownSurpriseBoxSFX;
    public GameObject countDownGameStartSFX;

    [Header("Pause Game SFX")]
    public GameObject pauseMenuOpenSFX;
    public GameObject pauseMenuCloseSFX;
    public GameObject[] buttonSelectSFX;
 
    [Header("Transition SFX")]
    public GameObject bigFlameTransitionSFX;

}

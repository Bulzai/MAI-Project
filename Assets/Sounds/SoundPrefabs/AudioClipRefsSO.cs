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
    public GameObject[] musicUpSFX;
    public GameObject[] musicDownSFX;
    public GameObject[] sfxUpSFX;
    public GameObject[] sfxDownSFX;

    
    [Header("Countdown and Start SFX")]
    public GameObject countdown3SFX;
    public GameObject countdown2SFX;
    public GameObject countdown1SFX;
    public GameObject startSFX;
    
    [Header("Character SFX")]
    public GameObject[] jumpSFX;
    public GameObject[] runningSFX;
    public GameObject[] hurtSFX;
    public GameObject[] deathSFX;
    public GameObject[] joinSFX;
    public GameObject[] readySFX;
    public GameObject[] characterWinSFX;
    
        
        
    [Header("Item SFX")]
    public GameObject cookieSelectSFX;
    public GameObject cookieSubmitSFX;
    
    public GameObject woodBlockSelectSFX;
    public GameObject woodBlockSubmitSFX;
    
    public GameObject bratApfelSelectSFX;
    public GameObject bratApfelSubmitSFX;
    public GameObject bratApfelHitSFX;
    
    public GameObject chocolateSelectSFX;
    public GameObject chocolateSubmitSFX;
    public GameObject chocolateHitSFX;
    
    public GameObject canonSelectSFX;
    public GameObject canonSubmitSFX;
    public GameObject canonShootSFX;
    
    public GameObject candleSelectSFX;
    public GameObject candleSubmitSFX;
    public GameObject candleBurningSFX;
    public GameObject candleWaxDropSFX;

    public GameObject iceSelectSFX;
    public GameObject iceSubmitSFX;
    
    public GameObject forbiddenSignSFX;
    
    
    [Header("Aura SFX")]
    public GameObject auraSpawnSFX;
    public GameObject auraFlyingSFX;
    
    public GameObject poisonAuraCollectSFX;
    public GameObject speedAuraCollectSFX;
    public GameObject bumpAuraCollectSFX;
    public GameObject slowAuraCollectSFX;
    
    public GameObject poisonAuraActiveSFX;
    public GameObject speedAuraActiveSFX;
    public GameObject bumpAuraActiveSFX;
    public GameObject slowAuraActiveSFX;
    
    
    [Header("Final Score SFX")]
    public GameObject pointsIncreaseSFX;
}

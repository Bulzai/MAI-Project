using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class AISelector : MonoBehaviour
{
    public static AISelector Instance { get; private set; }

    
    [SerializeField] private AdaptivePlacementStrategy adaptivePlacementStrategy;
    [SerializeField] private RandomPlacementStrategy randomPlacementStrategy;
    public PlaythroughType currentPlaythroughType { get; private set; }
    
    void Awake(){
        GameEvents.OnSelectAIStateEntered += StartNextPlaythrough;
        SinglePlayerScoreManager.OnPlaythroughStarted += StartNextPlaythrough;
        SinglePlayerScoreManager.OnNextRoundStarted += StartNextRound;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    
     void OnDestroy(){
        GameEvents.OnSelectAIStateEntered -= StartNextPlaythrough;
        SinglePlayerScoreManager.OnPlaythroughStarted -= StartNextPlaythrough;
        SinglePlayerScoreManager.OnNextRoundStarted -= StartNextRound;
    }

    private void StartNextRound()
    {
        StartPlacementStrategy();
    }
     private void StartPlacementStrategy()
     {
         switch (currentPlaythroughType)
         {
             case PlaythroughType.Undefined:
                 Debug.LogError("Playthrough type is undefined!");
                 break;
             case PlaythroughType.VersionA:
                 randomPlacementStrategy.StartRandomPlacement();
                 break;
             case PlaythroughType.VersionB:
                 adaptivePlacementStrategy.StartAdaptivePlacement();
                 break;
             default:
                 Debug.LogError("Unhandled playthrough type!");
                 break;
         }
     }
    private void StartNextPlaythrough()
    {
        SelectPlacementStrategy();
        StartPlacementStrategy();
    }
     /*
    public void HandleSelectAIStateEntered()
    {
        Debug.Log("HandleSelectAIStateEntered");

#if AI_VERSION_A
        if (isSecondPlaythrough)
        {
            randomPlacementStrategy.StartRandomPlacement();
            return;
        }   
        adaptivePlacementStrategy.StartAdaptivePlacement();
#elif AI_VERSION_B
        if (isSecondPlaythrough)
        {
            Debug.Log("Starting adaptive placement strategy");
            adaptivePlacementStrategy.StartAdaptivePlacement();
            return;
        }
        Debug.Log("Starting random placement strategy");
        randomPlacementStrategy.StartRandomPlacement();
#else
        //Debug.LogError("No AI version defined!");
#endif

        Debug.Log("BuildId: " + BuildConfig.BuildId);
    }
    */
    
    // first playthough is randomly selected, second is the other one, third+ are random again
    // the case where playtester shuts down the game after the first playthrough isn't handled
    public void SelectPlacementStrategy()
    {
        int gamesPlayed = PlayTestDataManager.Instance.previousGamesPlayed; 
    
        currentPlaythroughType = gamesPlayed switch
        {
            0 => Random.Range(0f, 1f) < 0.5f ? PlaythroughType.VersionA : PlaythroughType.VersionB, 
            1 => currentPlaythroughType == PlaythroughType.VersionA ? PlaythroughType.VersionB : PlaythroughType.VersionA,  // 2nd: other
            _ => Random.Range(0f, 1f) < 0.5f ? PlaythroughType.VersionA : PlaythroughType.VersionB  // 3+: random
        };
    }
}
public enum PlaythroughType
{
    VersionA, 
    VersionB,
    Undefined
}

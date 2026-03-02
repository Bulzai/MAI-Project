using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISelector : MonoBehaviour
{
    
    [SerializeField] private AdaptivePlacementStrategy adaptivePlacementStrategy;
    [SerializeField] private RandomPlacementStrategy randomPlacementStrategy;
    private bool isSecondPlaythrough = false;
    void Awake(){
        GameEvents.OnSelectAIStateEntered += HandleSelectAIStateEntered;
        SinglePlayerScoreManager.SecondPlaythroughStarted += StartSecondPlaythrough;
    }
    
    
     void OnDestroy(){
        GameEvents.OnSelectAIStateEntered -= HandleSelectAIStateEntered;
        SinglePlayerScoreManager.SecondPlaythroughStarted -= StartSecondPlaythrough;
    }

    private void StartSecondPlaythrough()
    {
        isSecondPlaythrough = true;
        HandleSelectAIStateEntered();
    }
     
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
}

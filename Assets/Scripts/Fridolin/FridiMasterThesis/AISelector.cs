using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISelector : MonoBehaviour
{
    
    [SerializeField] private AdaptivePlacementStrategy adaptivePlacementStrategy;
    [SerializeField] private RandomPlacementStrategy randomPlacementStrategy;

    void Awake(){
        GameEvents.OnSelectAIStateEntered += HandleSelectAIStateEntered;
    }
    
    
     void OnDestroy(){
        GameEvents.OnSelectAIStateEntered -= HandleSelectAIStateEntered;
    }
     
    public void HandleSelectAIStateEntered()
    {
        Debug.Log("HandleSelectAIStateEntered");    
        
#if AI_VERSION_A
        adaptivePlacementStrategy.StartAdaptivePlacement();
#elif AI_VERSION_B
        randomPlacementStrategy.StartRandomPlacement();
#else
        //Debug.LogError("No AI version defined!");
#endif

        Debug.Log("BuildId: " + BuildConfig.BuildId);
    }
}

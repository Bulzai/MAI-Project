using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptivePlacementStrategy : MonoBehaviour
{

    
    public void StartAdaptivePlacement()
    {
        PlacementFinished();
    }
    
    public void PlacementFinished()
    {
        GameEvents.ChangeState(GameState.MainGameState);
    }
}

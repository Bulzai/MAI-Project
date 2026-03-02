using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptivePlacementStrategy : MonoBehaviour
{

    
    public void StartAdaptivePlacement()
    {
        Debug.Log("AdaptivePlacementStrategy: StartAdaptivePlacement");
        PlacementFinished();
    }
    
    public void PlacementFinished()
    {
        StartCoroutine(CountdownManager.Instance.StartCountdown());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCaneTriggerScript : MonoBehaviour
{
    public Collider2D enableCollider;



    private void Awake()
    {

        GameEvents.OnPlaceItemStateEntered += DisableTrigger;
        GameEvents.OnMainGameStateEntered += EnableTrigger;
    }


    void DisableTrigger()
    {
        if (enableCollider != null)
        {
            enableCollider.isTrigger = false;
        }
    }

    void EnableTrigger()
    {
        if (enableCollider != null)
        {
            enableCollider.isTrigger = true;
        }
    }
}


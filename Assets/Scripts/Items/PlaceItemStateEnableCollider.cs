using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceItemStateEnableCollider : MonoBehaviour
{

    public Collider2D enableCollider;



    private void Awake()
    {

        GameEvents.OnPlaceItemStateEntered += EnableCollider;
        GameEvents.OnMainGameStateEntered += DisableCollider;
    }


    void DisableCollider()
    {
        if(enableCollider != null)
        {
            enableCollider.enabled = false;   
        }
    }

    void EnableCollider()
    {
        if(enableCollider != null)
        {
            enableCollider.enabled = true;   
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableItemCursorCollider : MonoBehaviour
{

    public Collider2D disableCollider;
    public Collider2D enableCollider;



    private void Start()
    {
        disableCollider = GetComponent<BoxCollider2D>();

    }

    private void OnEnable()
    {

        GameEvents.OnMainGameStateEntered += DisableCursorCollider;
    }



    void DisableCursorCollider()
    {
        if (disableCollider != null)
        {
            disableCollider.enabled = false;
        }

        if(enableCollider != null)
        {
            enableCollider.enabled = true;   
        }
    }

}

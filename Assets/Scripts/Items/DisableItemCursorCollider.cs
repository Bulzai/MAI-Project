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

        GameEvents.OnPlaceItemStateEntered += MakeColliderSmaller;
    }


    void MakeColliderSmaller()
    {

        if (disableCollider != null)
        {
            disableCollider.offset = new Vector2(-0.02170485f, 0.1969473f);
            if(disableCollider.GetComponent<BoxCollider2D>())
                disableCollider.GetComponent<BoxCollider2D>().size = new Vector2(1.204323f, 2.858709f);
        }

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableItemCursorCollider : MonoBehaviour
{

    private BoxCollider2D boxCollider;
    public Collider2D childrenCollider;



    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning("RotateSpike: No BoxCollider2D found!");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += DisableCursorCollider;
    }



    void DisableCursorCollider()
    {
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
            childrenCollider.enabled = true;
        }
    }

}

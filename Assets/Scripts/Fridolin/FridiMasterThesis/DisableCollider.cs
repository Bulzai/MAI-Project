using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableCollider : MonoBehaviour
{
    [SerializeField] private Collider2D colliderToDisable;
    // Start is called before the first frame update
    void Awake()
    {
        GameEvents.OnMainGameStateEntered += DisableColliderFunc;
    }
    
    void OnDestroy()
    {
        GameEvents.OnMainGameStateEntered -= DisableColliderFunc;
    }
    
    void DisableColliderFunc()
    {
        if (colliderToDisable != null)
        {
            colliderToDisable.enabled = false;
        }
    }
}

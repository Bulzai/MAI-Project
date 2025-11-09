using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForbiddenSignPlayerSpawn : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        GameEvents.OnPlaceItemStateEntered += ShowSign;
        GameEvents.OnMainGameStateEntered += HideSign;
        ShowSign();
    }

    private void OnDestroy()
    {
        GameEvents.OnPlaceItemStateEntered -= ShowSign;
        GameEvents.OnMainGameStateEntered -= HideSign;
    }

    private void ShowSign()
    {
        Debug.Log("ShowSign");
        spriteRenderer.enabled = true;
    }

    private void HideSign()
    {
        spriteRenderer.enabled = false;
    }
}

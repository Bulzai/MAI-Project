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
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
    }

    private void HideSign()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
    }
}
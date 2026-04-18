using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExtinguisherPickUp : MonoBehaviour
{
    private ExtingSpawner spawner;
    public static event Action OnMilkCollected;

    public void Init(ExtingSpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var health = collision.GetComponent<PlayerHealthSystem>();
        if (health == null) return;

        health.Extinguish();
        OnMilkCollected?.Invoke();

        PlayerInput playerInput = collision.GetComponentInParent<PlayerInput>();

        if (playerInput != null)
        {
            PopupTextManager.Instance?.ShowPopupForPlayer("Milk Power +6s", playerInput.playerIndex, Color.cyan);
            GameMetricsLogger.Instance?.RegisterMilkCollected(playerInput.playerIndex);
        }

        Destroy(gameObject);

        spawner?.RequestAdvance();
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameDmg : MonoBehaviour
{
    public int damageAmount;

    [SerializeField] float damageInterval = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameEvents.CurrentState == GameState.MainGameState)
        {
            var player = other.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {
                StartCoroutine(DamageTickRoutine(player, other));
            }
        }
    }

    private IEnumerator DamageTickRoutine(PlayerHealthSystem player, Collider2D playerCollider)
    {
        while (player != null && playerCollider.IsTouching(GetComponent<Collider2D>()))
        {

            player.TakeDamage(damageAmount, true, gameObject.name);

            yield return new WaitForSeconds(damageInterval);
        }
    }
}

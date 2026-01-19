using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamage : MonoBehaviour
{

    public int damgeAmount = 50;

    public float knockbackForce = 0;
    private void OnTriggerEnter2D(Collider2D other)
    {


        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {
                Debug.Log("Player hit." + damgeAmount + " damage took");
                player.TakeDamage(damgeAmount, true);


                Vector2 dir = (other.transform.position - transform.position);
                if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

                player.Knockback(dir, knockbackForce);
            }
            else
            {
                Debug.Log("No PlayerHealthSystem Script");
            }
        }
    }
}

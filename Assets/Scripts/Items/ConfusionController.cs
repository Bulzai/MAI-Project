using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfusionController : MonoBehaviour
{
    public float confusionDuration = 3f;
    public ParticleSystem confusionEffect;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealthSystem healthSystem = collision.collider.GetComponent<PlayerHealthSystem>();

            if (healthSystem != null)
            {
                if (confusionEffect != null)
                    confusionEffect.Play();

                // confusion effect
                healthSystem.ApplyConfusion(confusionDuration);

                Debug.Log("Confusion applied to player!");
            }

            // Optionally destroy the trap after activation
            //Destroy(gameObject, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealthSystem healthSystem = collision.GetComponent<PlayerHealthSystem>();

            if (healthSystem != null)
            {
                if (confusionEffect != null)
                    confusionEffect.Play();

                // confusion effect
                healthSystem.ApplyConfusion(confusionDuration);

                Debug.Log("Confusion applied to player!");
            }

            // Optionally destroy the trap after activation
            //Destroy(gameObject, 1f);
        }
    }
}

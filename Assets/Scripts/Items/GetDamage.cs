using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDamage : MonoBehaviour
{

    public int damgeAmount = 50;
    private void OnTriggerEnter2D(Collider2D other)
    {


        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {
                Debug.Log("Player hit." + damgeAmount + " damage took");
                player.TakeDamage(damgeAmount);
            }
            else
            {
                Debug.Log("No PlayerHealthSystem Script");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameDmg : MonoBehaviour
{
    public int damageAmount;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && GameEvents.CurrentState == GameState.MainGameState)
        {

            var player = other.GetComponent<PlayerHealthSystem>();
            if (player != null)
            {


                player.TakeDamage(damageAmount, true);


            }
            else
            {
                Debug.Log("No PlayerHealthSystem Script");
            }


        }
    }
}

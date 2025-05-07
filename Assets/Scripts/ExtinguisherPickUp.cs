using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguisherPickUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Extinguisher Picked Up!");
        HealthSystem health = collision.GetComponent<HealthSystem>();
        if (health != null)
        {
            health.Extinguish();
            Destroy(gameObject); // remove extinguisher
        }
    }
 
}

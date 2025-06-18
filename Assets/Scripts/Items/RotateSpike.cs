using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpike : MonoBehaviour
{

    public float rotationSpeed = 180f; // Grad pro Sekunde




    void Update()
    {
        // Dreht das Objekt um die Z-Achse
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    
}

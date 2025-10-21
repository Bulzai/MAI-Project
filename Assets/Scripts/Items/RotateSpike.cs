using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpike : MonoBehaviour
{

    [Header("Rotation Settings")]
    public float rotationSpeed = 180f; // Degrees per second

    private bool shouldRotate = false;



    private void OnEnable()
    {
        shouldRotate = true;

    }

    private void OnDisable()
    {
        shouldRotate = false;

    }

    private void Update()
    {
        if (shouldRotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }



}

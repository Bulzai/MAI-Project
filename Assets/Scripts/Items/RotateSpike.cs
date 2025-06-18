using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSpike : MonoBehaviour
{

    [Header("Rotation Settings")]
    public float rotationSpeed = 180f; // Degrees per second

    private bool shouldRotate = false;
    private BoxCollider2D boxCollider;
    private CircleCollider2D killCollider;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        killCollider = GetComponentInChildren<CircleCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning("RotateSpike: No BoxCollider2D found!");
        }
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateEntered += EnableRotation;
        GameEvents.OnMainGameStateExited += DisableRotation;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= EnableRotation;
        GameEvents.OnMainGameStateExited -= DisableRotation;
    }

    private void Update()
    {
        if (shouldRotate)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }
    }

    private void EnableRotation()
    {
        shouldRotate = true;
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
            killCollider.enabled = true; // Enable the kill collider
        }
    }

    private void DisableRotation()
    {
        shouldRotate = false;
    }

}

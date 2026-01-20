using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScaler : MonoBehaviour
{
    [Header("Scale Settings")]
    public float defaultScale = 3.4f;
    public float surpriseBoxScale = 0.1f;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        UpdateSpriteScale();
    }

    private void Update()
    {
        UpdateSpriteScale();
    }

    void UpdateSpriteScale()
    {
        if (GameEvents.CurrentState == GameState.SurpriseBoxState)
        {
            transform.localScale = Vector2.one * surpriseBoxScale;
        }
        else
        {
            transform.localScale = Vector2.one * defaultScale;
        }
    }
}
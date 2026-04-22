using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TarodevController;

public class PlayerHealthSystem : MonoBehaviour
{
    public static event Action OnPlayerTakeDamage;
    public static event Action OnPlayerDeath;
    public static event Action OnPlayerKnockedBack;

    private PlayerInput _playerInput;
    public PlayerAnimator animator;

    public int maxHealth = 100;
    public float burnTickInterval = 1f;
    public int burnDamagePerTick = 5;
    public float reigniteDelay = 5f;

    public int currentHealth;

    public bool isBurning = false;
    public bool isConfused = false;

    public SpriteRenderer spriteRenderer;
    public Color originalColor;

    public ParticleSystem deathParticles;

    PlayerController playerController;
    private Coroutine burnCoroutine;
    private Coroutine reigniteCoroutine;
    private Coroutine confusedCoroutine;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 12f;
    [SerializeField] private float verticalBoost = 2.5f;

    [Header("Confusion Visuals")]
    [SerializeField] private GameObject confusionLogo;
    [SerializeField] private Color confusionColor = new Color(1f, 0.4f, 0.8f);
    private Coroutine _confusionEffectCo;

    private GameObject fireSprite;

    private void Start()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        currentHealth = maxHealth;
        fireSprite = transform.GetChild(0).GetChild(0).gameObject;
        fireSprite.gameObject.SetActive(false);

        originalColor = spriteRenderer.color;
        playerController = GetComponent<TarodevController.PlayerController>();
    }

    private void OnEnable()
    {
        GameEvents.OnMainGameStateExited += ResetConfusion;
        GameEvents.OnMainGameStateEntered += ResetConfusion;

        // Respawn player when main game starts instead of old countdown finish
        GameEvents.OnMainGameStateEntered += RespawnPlayer;
    }

    private void OnDisable()
    {
        GameEvents.OnMainGameStateEntered -= RespawnPlayer;
        GameEvents.OnMainGameStateExited -= ResetConfusion;
        GameEvents.OnMainGameStateEntered -= ResetConfusion;
    }

    public void SetOnFire()
    {
        if (!isBurning)
        {
            isBurning = true;

            if (gameObject.activeSelf)
                burnCoroutine = StartCoroutine(BurnOverTime());

            if (fireSprite != null)
                fireSprite.gameObject.SetActive(true);
        }
    }

    public void Extinguish()
    {
        if (isBurning)
        {
            isBurning = false;

            if (burnCoroutine != null)
                StopCoroutine(burnCoroutine);

            if (fireSprite != null)
                fireSprite.gameObject.SetActive(false);

            Invoke("SetOnFire", reigniteDelay);
        }
    }

    private IEnumerator BurnOverTime()
    {
        while (isBurning)
        {
            TakeDamage(burnDamagePerTick, false);
            yield return new WaitForSeconds(burnTickInterval);
        }
    }

    public void TakeDamage(int amount, bool isItemDmg)
    {
        currentHealth -= amount;

        if (isItemDmg && amount > 0)
        {
            OnPlayerTakeDamage?.Invoke();

            if (animator != null)
                animator.PlayHitReaction();

            StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
            OnPlayerDeath?.Invoke();
            animator.PlayDeath();

            if (_playerInput != null && GameMetricsLogger.Instance != null)
            {
                GameMetricsLogger.Instance.RegisterDeath(_playerInput.playerIndex);
            }

            Die();
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("No SpriteRenderer assigned for red flash.");
            yield break;
        }

        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = originalColor;
    }

    public void ResetDeathFlags()
    {
        animator._dead = false;
        animator.ResetDeath();
    }

    void RespawnPlayer()
    {
        if (playerController != null)
            playerController.EnableControls();

        DisableOrEnableFireSprite(true);

        animator._dead = false;
        animator.ResetDeath();
        transform.gameObject.SetActive(true);
    }

    public void ApplyConfusion(float duration)
    {
        if (_confusionEffectCo != null)
        {
            StopCoroutine(_confusionEffectCo);
        }

        _confusionEffectCo = StartCoroutine(ConfusionVisualRoutine(duration));
    }

    private IEnumerator ConfusionVisualRoutine(float duration)
    {
        isConfused = true;

        if (confusionLogo) confusionLogo.SetActive(true);
        float elapsed = 0f;
        float blinkInterval = 0.2f;

        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (spriteRenderer.color == Color.white) ? confusionColor : Color.white;
            }

            elapsed += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        ResetConfusion();
    }

    private void ResetConfusion()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        if (confusionLogo) confusionLogo.SetActive(false);

        isConfused = false;
        _confusionEffectCo = null;
    }

    public bool IsConfused()
    {
        return isConfused;
    }

    private void Die()
    {
        GameEvents.PlayerEliminated(_playerInput);

        if (playerController != null)
            playerController.DisableControls();

        DisableOrEnableFireSprite(false);
        Invoke("DisableCharacter", 1f);
    }

    void DisableCharacter()
    {
        transform.gameObject.SetActive(false);
    }

    private void DisableOrEnableCollider(bool enabled)
    {
        transform.GetComponent<CapsuleCollider2D>().enabled = enabled;
    }

    private void DisableOrEnableFireSprite(bool enabled)
    {
        transform.GetChild(0).GetChild(0).gameObject.SetActive(enabled);
    }

    public void Knockback(Vector2 direction, float strength)
    {
        if (direction.sqrMagnitude > 0.0001f) direction.Normalize();

        var pc = GetComponent<TarodevController.PlayerController>();
        if (pc == null) return;

        Vector2 impulse = direction * (strength * knockbackMultiplier);
        if (Mathf.Abs(direction.y) < 0.25f) impulse.y += verticalBoost;

        pc.AddImpulse(impulse);
        OnPlayerKnockedBack?.Invoke();
    }
}
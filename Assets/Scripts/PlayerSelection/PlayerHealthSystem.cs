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
    [SerializeField] private float knockbackMultiplier = 12f; // tweak feel
    [SerializeField] private float verticalBoost = 2.5f;      // extra lift when grounded 


    [Header("Confusion Visuals")]
    [SerializeField] private GameObject confusionLogo; // Drag your UI icon/Sprite here
    [SerializeField] private Color confusionColor = new Color(1f, 0.4f, 0.8f); // Pinkish
    private Coroutine _confusionEffectCo;


    private GameObject fireSprite;
    private void Start()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        currentHealth = maxHealth;
        fireSprite = transform.GetChild(0).GetChild(0).gameObject;
        fireSprite.gameObject.SetActive(false); // start off

        originalColor = spriteRenderer.color;


        playerController = GetComponent<TarodevController.PlayerController>();
        //Invoke("SetOnFire", 3f);
    }
    private void OnEnable()
    {
        GameEvents.OnMainGameStateExited += ResetConfusion;
        GameEvents.OnMainGameStateEntered += ResetConfusion;
        PlaceItemState.CountDownFinished += RespawnPlayer;
    }
    private void OnDisable()
    {
        PlaceItemState.CountDownFinished -= RespawnPlayer;
        GameEvents.OnMainGameStateExited -= ResetConfusion;
        GameEvents.OnMainGameStateEntered -= ResetConfusion;


    }
    public void SetOnFire()
    {
        if (!isBurning)
        {

            isBurning = true;
            if(gameObject.activeSelf)
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
            TakeDamage(burnDamagePerTick,false);
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
        playerController.EnableControls();
        DisableOrEnableFireSprite(true);
        //DisableOrEnableCollider(true);
        animator._dead = false;
        animator.ResetDeath();
        transform.gameObject.SetActive(true);

    }
    public void ApplyConfusion(float duration)
    {
        // If already confused, stop the old routine to refresh the duration
        if (_confusionEffectCo != null)
        {
            StopCoroutine(_confusionEffectCo);
        }

        _confusionEffectCo = StartCoroutine(ConfusionVisualRoutine(duration));
    }

    private IEnumerator ConfusionVisualRoutine(float duration)
    {
        isConfused = true; // Steuerung invertieren START

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

        isConfused = false; // Steuerung invertieren ENDE
        _confusionEffectCo = null;
    }
    public bool IsConfused()
    {
        return isConfused;
    }

    private void Die()
    {
        GameEvents.PlayerEliminated(_playerInput);
        playerController.DisableControls();
        DisableOrEnableFireSprite(false);
        //DisableOrEnableCollider(false);
        //transform.gameObject.SetActive(false);
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
        // Normalize for safety
        if (direction.sqrMagnitude > 0.0001f) direction.Normalize();

        // Convert "strength" into a delta-velocity, not force.
        // We push a little upward if the player is grounded to feel snappier.
        var pc = GetComponent<TarodevController.PlayerController>();
        if (pc == null) return;

        // Optional: small vertical boost if mostly horizontal hit
        Vector2 impulse = direction * (strength * knockbackMultiplier);
        if (Mathf.Abs(direction.y) < 0.25f) impulse.y += verticalBoost;

        pc.AddImpulse(impulse);
        OnPlayerKnockedBack?.Invoke();
    }

}

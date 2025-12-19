using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TarodevController;

public class PlayerHealthSystem : MonoBehaviour
{
    
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


    private Coroutine burnCoroutine;
    private Coroutine reigniteCoroutine;
    private Coroutine confusedCoroutine;

    [Header("Knockback")]
    [SerializeField] private float knockbackMultiplier = 12f; // tweak feel
    [SerializeField] private float verticalBoost = 2.5f;      // extra lift when grounded 

    private GameObject fireSprite;
    private void Start()
    {
        _playerInput = GetComponentInParent<PlayerInput>();
        currentHealth = maxHealth;
        fireSprite = transform.GetChild(0).GetChild(0).gameObject;
        fireSprite.gameObject.SetActive(false); // start off

        originalColor = spriteRenderer.color;

        //Invoke("SetOnFire", 3f);
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

            Debug.Log(" Fire extinguished!");

            Invoke("SetOnFire", 5f);
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
            if (animator != null)
                animator.PlayHitReaction();

            StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0)
        {
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

    public void ApplyConfusion(float duration)
    {
        if(confusedCoroutine != null)
        {
            StopCoroutine(confusedCoroutine);
        }

        confusedCoroutine = StartCoroutine(ConfusionRoutine(duration));
    }

    private IEnumerator ConfusionRoutine(float duration)
    {
        isConfused = true;
        Debug.Log("Reversed controls");
        yield return new WaitForSeconds(duration);
        isConfused = false;
        Debug.Log("Confusion done");
    }

    public bool IsConfused()
    {
        return isConfused;
    }

    private void Die()
    {
        GameEvents.PlayerEliminated(_playerInput);
        transform.gameObject.SetActive(false);
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
    }

}

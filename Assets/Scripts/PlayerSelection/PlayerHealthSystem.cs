using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerHealthSystem : MonoBehaviour
{
    
    private PlayerInput _playerInput;
    
    
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
        Debug.Log("EnteredFireState");
        if (!isBurning)
        {
            isBurning = true;
            if(gameObject.activeSelf)
                burnCoroutine = StartCoroutine(BurnOverTime());

            if (fireSprite != null)
                fireSprite.gameObject.SetActive(true);

            Debug.Log(" Player is on fire!");
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
        Debug.Log("Player took damage! Health: " + currentHealth);

        if(isItemDmg)
            StartCoroutine(FlashRed());

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

    }
}

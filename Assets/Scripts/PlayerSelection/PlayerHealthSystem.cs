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

    private Coroutine burnCoroutine;
    private Coroutine reigniteCoroutine;


    private GameObject fireSprite;
    private void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        currentHealth = maxHealth;
        fireSprite = transform.GetChild(0).GetChild(0).gameObject;
        fireSprite.gameObject.SetActive(false); // start off
        //Invoke("SetOnFire", 3f);
    }

    public void SetOnFire()
    {
        Debug.Log("EnteredFireState");
        if (!isBurning)
        {
            isBurning = true;

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
            TakeDamage(burnDamagePerTick);
            yield return new WaitForSeconds(burnTickInterval);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Player took damage! Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameEvents.PlayerEliminated(_playerInput);
        gameObject.SetActive(false);

    }
}

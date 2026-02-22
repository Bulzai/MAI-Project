using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SinglePlayerManager : MonoBehaviour
{

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform spawnPointPlayer;

    public static SinglePlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        
        GameEvents.OnMainGameStateEntered += SpawnPlayer;
        //GameEvents.OnMainGameStateExited += DespawnPlayer;
    }

    private void OnDestroy()
    {
        GameEvents.OnMainGameStateEntered -= SpawnPlayer;
        //GameEvents.OnMainGameStateExited -= DespawnPlayer;
    }

    public void DespawnPlayer()
    {
        GameObject root = playerObject;
        var characterTf = root.transform.Find("PlayerNoPI");
        if (characterTf != null)
        {
            var character = characterTf.gameObject;
            character.SetActive(false);
        }

        var pi = root.GetComponent<PlayerInput>();
        if (pi != null) pi.DeactivateInput();

    }

    public void SpawnPlayer()
    {
        GameObject root = playerObject;

        var characterTf = root.transform.Find("PlayerNoPI");

        var characterGO = characterTf.gameObject;
        characterGO.SetActive(true);

        var health = characterGO.GetComponent<PlayerHealthSystem>();

        if (health != null)
        {
            health.ResetDeathFlags();

            if (health.spriteRenderer != null)
                health.spriteRenderer.color = health.originalColor;

            health.currentHealth = health.maxHealth;
            health.isBurning = false;
            // If SetOnFire() actually sets burning, consider renaming;
            // keeping your call to preserve behavior.
            health.SetOnFire();
            Debug.Log("current health: " + health.currentHealth);
        }
            
        var pi = root.GetComponent<PlayerInput>();
        if (pi != null)
        {
            pi.ActivateInput();
            pi.SwitchCurrentActionMap("Player");
            Debug.Log("PlayerInput activated and switched to Player action map.");
        }
        
        characterGO.GetComponent<TarodevController.PlayerController>().EnableControls();
            
        characterGO.transform.position = spawnPointPlayer.position;

    }
    
}

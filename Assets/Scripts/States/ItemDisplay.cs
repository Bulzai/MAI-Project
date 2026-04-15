using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public static ItemDisplay Instance { get; private set; }

    [SerializeField] private GameObject selectionManagerGO;
    [SerializeField] private GameObject playerManagerGO;
    private PlayerSelectionManager selectionManager;
    private PlayerManager playerManager;

    [Header("Item Pool")]
    [SerializeField] private List<GameObject> generalItemPool;

    [SerializeField] private GameObject[] playerReadyGO;
    [SerializeField] private GameObject[] playerUnreadyGO;

    private void Awake()
    {
        // singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        selectionManager = selectionManagerGO.GetComponent<PlayerSelectionManager>();
        playerManager = playerManagerGO.GetComponent<PlayerManager>();
    }

    private void OnEnable()
    {
        selectionManager.ResetAllPlayersReadyStatus();
        ShowPlayerSlots();
    }

    private void OnDisable()
    {
        selectionManager.ResetAllPlayersReadyStatus();
        playerManager.DeactivateCharacterPrefab();
        ResetReadiness();
    }

    // GETTER
    public List<GameObject> getGeneralItemPool()
    {
        return generalItemPool;
    }
  
    // READY OR NOT
    private void ShowPlayerSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            bool joined = selectionManager.isPlayerJoined(i);

            // all are unready
            if (i < playerReadyGO.Length && playerReadyGO[i] != null)
                playerReadyGO[i].SetActive(false);

            if (i < playerUnreadyGO.Length && playerUnreadyGO[i] != null)
                playerUnreadyGO[i].SetActive(joined);
        }
    }

    public void UpdateReadyStatus(PlayerInput playerInput, PlayerSelectionData data)
    {
        int index = playerInput.playerIndex;

        Debug.Log($"{index} player index is {data.IsReady}");

        if (index >= 0 && index < playerReadyGO.Length && index < playerUnreadyGO.Length)
        {
            // turn on ready and off unready if IsReady is true
            // turn off ready and on unready if IsReady is false
            if (playerReadyGO[index] != null)
                playerReadyGO[index].SetActive(data.IsReady);

            if (playerUnreadyGO[index] != null)
                playerUnreadyGO[index].SetActive(!data.IsReady);
        }
        else
        {
            Debug.LogWarning($"Player Index {index} is out of range for the UI arrays!");
        }
    }

    private void ResetReadiness()
    {
        for(int i = 0; i < playerReadyGO.Length; i++)
        {
            if (playerReadyGO[i] != null)
                playerReadyGO[i].SetActive(false);
            if (playerUnreadyGO[i] != null)
                playerUnreadyGO[i].SetActive(false);
        }
    }
}

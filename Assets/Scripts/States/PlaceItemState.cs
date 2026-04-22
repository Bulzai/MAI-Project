using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceItemState : MonoBehaviour
{
    public static PlaceItemState Instance { get; private set; }

    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameObject GameWorld;
    //[SerializeField] private GridPlacementSystem gridPlacementSystem;

    //public static Action CountDownStarted;
    //public static Action CountDownFinished;

    public GameObject guideScreen;
    public RoundController roundController;

    private bool isTransitioningToMainGame = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.OnPlaceItemStateEntered += BeginPlacementPhaseAll;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaceItemStateEntered -= BeginPlacementPhaseAll;
    }

    public void HideAllCursors()
    {
        if (playerManager == null || playerManager.playerRoots == null)
        {
            Debug.LogWarning("PlayerManager or playerRoots is missing.");
            return;
        }

        foreach (var root in playerManager.playerRoots.Values)
        {
            if (root == null)
                continue;

            var cursorTransform = root.transform.Find("CursorNoPI");
            if (cursorTransform != null)
                cursorTransform.gameObject.SetActive(false);

            var playerInput = root.GetComponent<PlayerInput>();
            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Cursor");
        }
    }

    private void AllPlayersFinishedPlacing()
    {
        if (GridPlacementSystem.Instance != null)
            GridPlacementSystem.Instance.HideGrid();

        if (playerManager != null)
        {
            playerManager.pickedPrefabByPlayer.Clear();
            playerManager.playersThatPlaced.Clear();
        }

        HideAllCursors();

        if (!isTransitioningToMainGame)
        {
            StartCoroutine(ShowGuideSequence());
            Debug.Log("Finished placing, starting next sequence");
        }
    }

    private IEnumerator ShowGuideSequence()
    {
        isTransitioningToMainGame = true;

        if (roundController != null && roundController.currentRound == 0)
        {
            yield return new WaitForSeconds(1.45f);

            if (guideScreen != null)
                guideScreen.SetActive(true);

            float showTime = 6.0f;
            yield return new WaitForSeconds(showTime);

            if (guideScreen != null)
                guideScreen.SetActive(false);

            yield return new WaitForSeconds(0.7f);
        }
        else
        {
            yield return new WaitForSeconds(0.7f);
        }

        GameEvents.ChangeState(GameState.MainGameState);
        isTransitioningToMainGame = false;
    }

    private void BeginPlacementPhaseAll()
    {
        if (GameWorld != null)
            GameWorld.SetActive(true);

        if (playerManager == null)
        {
            Debug.LogError("PlayerManager is missing.");
            return;
        }

        if (playerManager.pickedPrefabByPlayer == null || playerManager.pickedPrefabByPlayer.Count == 0)
        {
            Debug.Log("No picks yet, aborting");
            return;
        }

        playerManager.playersThatPlaced.Clear();

        if (GridPlacementSystem.Instance != null)
            GridPlacementSystem.Instance.ShowGrid();

        foreach (var kv in playerManager.pickedPrefabByPlayer)
        {
            int idx = kv.Key;
            GameObject prefab = kv.Value;

            if (!playerManager.playerRoots.TryGetValue(idx, out var root) || root == null)
            {
                Debug.LogError("Missing root for idx=" + idx);
                continue;
            }

            var cursorTransform = root.transform.Find("CursorNoPI");
            var characterTransform = root.transform.Find("PlayerNoPI");

            if (cursorTransform == null)
            {
                Debug.LogError("CursorNoPI not found for idx=" + idx);
                continue;
            }

            if (characterTransform == null)
            {
                Debug.LogError("PlayerNoPI not found for idx=" + idx);
                continue;
            }

            var cursor = cursorTransform.gameObject;
            var character = characterTransform.gameObject;

            playerManager.ResetCursorPositionItemPlacement(idx);

            cursor.SetActive(true);
            character.SetActive(false);

            var pi = root.GetComponent<PlayerInput>();
            if (pi != null)
                pi.SwitchCurrentActionMap("Cursor");

            var cc = cursor.GetComponent<CursorController>();
            if (cc != null)
            {
                cc.BeginPlacementPhase(prefab, cc.transform);
            }
            else
            {
                Debug.LogError("CursorController missing on cursor for idx=" + idx);
            }
        }
    }

    public void NotifyPlayerPlaced(int idx)
    {
        if (playerManager == null)
        {
            Debug.LogError("PlayerManager is missing.");
            return;
        }

        if (!playerManager.playersThatPlaced.Contains(idx))
            playerManager.playersThatPlaced.Add(idx);

        if (playerManager.playerRoots.TryGetValue(idx, out var root) && root != null)
        {
            var cursorTransform = root.transform.Find("CursorNoPI");
            if (cursorTransform != null)
                cursorTransform.gameObject.SetActive(false);
        }

        if (playerManager.playersThatPlaced.Count == playerManager.playerCount)
        {
            AllPlayersFinishedPlacing();
        }
    }
}
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
    private Coroutine countdownCoroutine;

    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GameObject GameWorld;
    //[SerializeField] private GridPlacementSystem gridPlacementSystem;

    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text milkText;

    public static Action CountDownStarted;
    public static Action CountDownFinished;


    public GameObject guideScreen;
    public Animator guideAnimator;
    
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

    public void HideAllCursors()
    {
        foreach (var root in playerManager.playerRoots.Values)
        {
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);
            root.GetComponent<PlayerInput>().SwitchCurrentActionMap("Cursor");
        }
    }


    private void AllPlayersFinishedPlacing()
    {

        GridPlacementSystem.Instance.HideGrid();
        playerManager.pickedPrefabByPlayer.Clear();
        playerManager.playersThatPlaced.Clear();
        HideAllCursors();

        if (countdownCoroutine == null)
        {
            StartCoroutine(ShowGuideSequence());
        }

    }

    private IEnumerator ShowGuideSequence()
    {

        Debug.Log("in coroutine");
        guideScreen.SetActive(true);

        // 1. Trigger the "Open" animation
        // Use a Trigger named "Open" or a Bool named "isOpen"
        guideAnimator.SetTrigger("Open");

        // 2. Wait for the screen to stay visible
        // Adjust 'showTime' to how long you want players to read the guide
        float showTime = 6.0f;
        yield return new WaitForSeconds(showTime);

        // 3. Trigger the "Close" animation
        guideAnimator.SetTrigger("Close");

        // 4. WAIT for the closing animation to actually finish
        // We look at the animator's current state to get the exact clip length
        float closeAnimDuration = 2f;
        yield return new WaitForSeconds(closeAnimDuration);
        guideScreen.SetActive(false);

        // 5. NOW start the actual game countdown
        countdownCoroutine = StartCoroutine(CountdownBeforeMainGame());

        Debug.Log("finished");
    }
    private IEnumerator CountdownBeforeMainGame( int countdown = 2, float timing = 0.6f)
    {
        CountDownStarted?.Invoke();
        
        while (countdown > 0)
        {
            milkText.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(true);
            countdownText.text = countdown.ToString();



            yield return new WaitForSeconds(timing);
            countdown--;
        }

        GameEvents.ChangeState(GameState.MainGameState);
        CountDownFinished?.Invoke();
        countdownText.gameObject.SetActive(false);
        milkText.gameObject.SetActive(false);
        countdownCoroutine = null;               

    }

    private void BeginPlacementPhaseAll()
    {
        GameWorld.SetActive(true);

        if (playerManager.pickedPrefabByPlayer.Count == 0)
        {
            Debug.Log("No picks yet, aborting");
            return;
        }


        playerManager.playersThatPlaced.Clear();
        GridPlacementSystem.Instance.ShowGrid();

        foreach (var kv in playerManager.pickedPrefabByPlayer)
        {
            int idx = kv.Key;
            GameObject prefab = kv.Value;

            if (!playerManager.playerRoots.TryGetValue(idx, out var root))
            {
                Debug.LogError("Missing root for idx=" + idx);
                continue;
            }

            // enable cursor, disable character
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            var character = root.transform.Find("PlayerNoPI").gameObject;
            playerManager.ResetCursorPositionItemPlacement(idx);
            cursor.SetActive(true);
            character.SetActive(false);
            /*
            // position at placement spawn. should be already set in playermanagerfinal
            Vector3 pos = (idx < playerManagerFinal.spawnPositionsForPlacement.Length)
                ? playerManagerFinal.spawnPositionsForPlacement[idx]
                : Vector3.zero;
            //otherwise should work like the following line

            //cursor.transform.position = spawnPositionsForSelection[idx].transform.position;

            cursor.transform.position = pos;
            */

            //Debug.Log("Cursor idx=" + idx + " moved to " + pos);

            // switch input map
            var pi = root.GetComponent<PlayerInput>();
            pi.SwitchCurrentActionMap("Cursor");

            // begin placement on cursor controller
            var cc = cursor.GetComponent<CursorController>();
            cc.BeginPlacementPhase(prefab, cc.transform);
        }

    }



    // Called by CursorController when a player places their item
    public void NotifyPlayerPlaced(int idx)
    {
        //Debug.Log("NotifyPlayerPlaced idx=" + idx);

        if (!playerManager.playersThatPlaced.Contains(idx))
            playerManager.playersThatPlaced.Add(idx);

        // deactivate cursor
        if (playerManager.playerRoots.TryGetValue(idx, out var root))
        {
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);
            //Debug.Log("Deactivated cursor idx=" + idx);
        }

       // Debug.Log("placedCount=" + playerManagerFinal.playersThatPlaced.Count +
              //    " joinedCount=" + playerManagerFinal.PlayerCount);

        if (playerManager.playersThatPlaced.Count == playerManager.playerCount)
        {
           // Debug.Log("All placed → AllPlayersFinishedPlacing");
            AllPlayersFinishedPlacing();

        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaceItemState : MonoBehaviour
{
    public static PlaceItemState Instance { get; private set; }

    [SerializeField] private PlayerManager playerManagerFinal;
    [SerializeField] private GameObject GameWorld;
    //[SerializeField] private GridPlacementSystem gridPlacementSystem;



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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        GameEvents.OnPlaceItemStateEntered += BeginPlacementPhaseAll;
    }

    public void HideAllCursors()
    {
        foreach (var root in playerManagerFinal.playerRoots.Values)
        {
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);
            root.GetComponent<PlayerInput>().SwitchCurrentActionMap("Cursor");
        }
    }


    private void AllPlayersFinishedPlacing()
    {

        GridPlacementSystem.Instance.HideGrid();
        playerManagerFinal.pickedPrefabByPlayer.Clear();
        playerManagerFinal.playersThatPlaced.Clear();
        Debug.Log("AllPlayersFinishedPlacing ");
        HideAllCursors();
        GameEvents.ChangeState(GameState.MainGameState);

    }

    private void BeginPlacementPhaseAll()
    {
        Debug.Log("BeginPlacementPhaseAll start");
        GameWorld.SetActive(true);

        if (playerManagerFinal.pickedPrefabByPlayer.Count == 0)
        {
            Debug.Log("No picks yet, aborting");
            return;
        }


        playerManagerFinal.playersThatPlaced.Clear();
        GridPlacementSystem.Instance.ShowGrid();

        foreach (var kv in playerManagerFinal.pickedPrefabByPlayer)
        {
            int idx = kv.Key;
            GameObject prefab = kv.Value;

            if (!playerManagerFinal.playerRoots.TryGetValue(idx, out var root))
            {
                Debug.LogError("Missing root for idx=" + idx);
                continue;
            }

            // enable cursor, disable character
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            var character = root.transform.Find("PlayerNoPI").gameObject;
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
            Debug.Log("Switched to Cursor map for idx=" + idx);

            // begin placement on cursor controller
            var cc = cursor.GetComponent<CursorController>();
            cc.BeginPlacementPhase(prefab, cc.transform);
        }

        Debug.Log("BeginPlacementPhaseAll end");
    }



    // Called by CursorController when a player places their item
    public void NotifyPlayerPlaced(int idx)
    {
        //Debug.Log("NotifyPlayerPlaced idx=" + idx);

        if (!playerManagerFinal.playersThatPlaced.Contains(idx))
            playerManagerFinal.playersThatPlaced.Add(idx);

        // deactivate cursor
        if (playerManagerFinal.playerRoots.TryGetValue(idx, out var root))
        {
            var cursor = root.transform.Find("CursorNoPI").gameObject;
            cursor.SetActive(false);
            //Debug.Log("Deactivated cursor idx=" + idx);
        }

       // Debug.Log("placedCount=" + playerManagerFinal.playersThatPlaced.Count +
              //    " joinedCount=" + playerManagerFinal.PlayerCount);

        if (playerManagerFinal.playersThatPlaced.Count == playerManagerFinal.playerCount)
        {
           // Debug.Log("All placed → AllPlayersFinishedPlacing");
            AllPlayersFinishedPlacing();

        }
    }

}

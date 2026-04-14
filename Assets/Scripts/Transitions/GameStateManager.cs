using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameStateManager : MonoBehaviour
{
    [Header("State GameObjects")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject playerSelectionUI;
    [SerializeField] private GameObject playerSelectionState;
    [SerializeField] private GameObject placeableItemSelectionUI;
    [SerializeField] private GameObject placeableItemSelectionState;
    [SerializeField] private GameObject surpriseBoxState;
    [SerializeField] private GameObject placeItemUI;
    [SerializeField] private GameObject placeItemState;
    [SerializeField] private GameObject GameMap;
    [SerializeField] private GameObject Scoreboard;

    private Dictionary<GameState, List<GameObject>> stateMap;

    private void Awake()
    {
        // Map Enums to GameObjects for easy switching
        stateMap = new Dictionary<GameState, List<GameObject>>
        {
            { GameState.MenuState, new List<GameObject> { mainMenuUI } },

            { GameState.PlayerSelectionState, new List<GameObject> { playerSelectionUI, playerSelectionState } },

            { GameState.PlaceableItemSelectionState, new List<GameObject> { placeableItemSelectionUI, placeableItemSelectionState } },

            { GameState.SurpriseBoxState, new List<GameObject> { surpriseBoxState } },

            { GameState.PlaceItemState, new List<GameObject> { placeItemUI, placeItemState } },

            { GameState.MainGameState, new List<GameObject> { GameMap } },

            //{ GameState.ScoreState, new List<GameObject> { Scoreboard } },

            //{ GameState.FinalScoreState, new List<GameObject> { Scoreboard } }
        };
    }

    private void OnEnable() => GameEvents.OnStateChanged += HandleStateChanged;
    private void OnDisable() => GameEvents.OnStateChanged -= HandleStateChanged;

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        // We always start the Coroutine, but the Coroutine decides IF it needs to wait
        StartCoroutine(ProcessStateTransition(newState));
    }

    private IEnumerator ProcessStateTransition(GameState newState)
    {
        // 1. ONLY wait if the new state is ScoreState
        if (newState == GameState.ScoreState)
        {
            yield return new WaitForSeconds(1f);
        }
        // If it's NOT ScoreState, the code skips the 'if' and runs immediately!

        // 2. Turn everything OFF
        foreach (var stateList in stateMap.Values)
        {
            foreach (var obj in stateList)
            {
                if (newState == GameState.ScoreState) continue;

                if (obj != null) obj.SetActive(false);
            }
        }

        // 3. Turn the NEW state objects ON
        if (stateMap.TryGetValue(newState, out List<GameObject> nextObjects))
        {
            foreach (var obj in nextObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }

    //private void HandleStateChanged(GameState oldState, GameState newState)
    //{
    //    if (GameEvents.CurrentState == GameState.ScoreState) new WaitForSeconds(0.45f);
    //    // 1. Loop through every List in the dictionary and turn everything OFF
    //    foreach (var stateList in stateMap.Values)
    //    {
    //        foreach (var obj in stateList)
    //        {
    //            if (obj != null) obj.SetActive(false);
    //        }
    //    }

    //    // 2. Find the specific list for the NEW state and turn everything ON
    //    if (stateMap.TryGetValue(newState, out List<GameObject> nextObjects))
    //    {
    //        foreach (var obj in nextObjects)
    //        {
    //            if (obj != null) obj.SetActive(true);
    //        }
    //    }
    //}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

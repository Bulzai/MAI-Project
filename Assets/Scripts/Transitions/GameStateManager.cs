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
        // 1. Loop through every List in the dictionary and turn everything OFF
        foreach (var stateList in stateMap.Values)
        {
            foreach (var obj in stateList)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 2. Find the specific list for the NEW state and turn everything ON
        if (stateMap.TryGetValue(newState, out List<GameObject> nextObjects))
        {
            foreach (var obj in nextObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

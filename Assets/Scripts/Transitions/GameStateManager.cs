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
    [SerializeField] private GameObject itemDisplayUI;
    [SerializeField] private GameObject itemDisplayManager;
    [SerializeField] private GameObject automaticPlacementState;
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

            { GameState.ItemDisplay, new List<GameObject> { itemDisplayUI, itemDisplayManager } },

            // { GameState.SurpriseBoxState, new List<GameObject> { surpriseBoxState } },

            { GameState.AutomaticPlacement, new List<GameObject> { automaticPlacementState } },

            { GameState.PlaceItemState, new List<GameObject> {  placeItemState } },

            { GameState.MainGameState, new List<GameObject> { GameMap } },
        };
    }

    private void OnEnable() => GameEvents.OnStateChanged += HandleStateChanged;
    private void OnDisable() => GameEvents.OnStateChanged -= HandleStateChanged;

    private void HandleStateChanged(GameState oldState, GameState newState)
    {
        StartCoroutine(ProcessStateTransition(newState));
    }

    private IEnumerator ProcessStateTransition(GameState newState)
    {
        // wait for transition
        if (newState == GameState.ScoreState)
        {
            yield return new WaitForSeconds(1f);
        }

        // all off
        foreach (var stateList in stateMap.Values)
        {
            foreach (var obj in stateList)
            {
                if (newState == GameState.ScoreState) continue;

                if (obj != null) obj.SetActive(false);
            }
        }

        // new state objects on
        if (stateMap.TryGetValue(newState, out List<GameObject> nextObjects))
        {
            foreach (var obj in nextObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }
}

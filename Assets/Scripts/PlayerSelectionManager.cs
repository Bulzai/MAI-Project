using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerSelectionManager : MonoBehaviour
{
    
    private readonly Dictionary<PlayerInput, PlayerSelectionData> _playerSelection =
        new Dictionary<PlayerInput, PlayerSelectionData>();

    // using the A and B UI button colors
    [SerializeField] private Color readyColor    = new Color();  // green-ish : 39EF07
    [SerializeField] private Color notReadyColor = new Color();  // red-ish : CF0000 using the A and B button colors

    
    
    void Awake()
    {
        PlayerManager.OnPlayerJoinedGlobal += HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal   += HandlePlayerLeft;
        TarodevController.PlayerController.OnPlayerReady += HandlePlayerReady;
        TarodevController.PlayerController.OnTryStartGame += TryStartGame;
        GameEvents.OnPlayerSelectionStateExited += HandlePlayerSelectionStateExit;
    }

    void OnDestroy()
    {
        PlayerManager.OnPlayerJoinedGlobal -= HandlePlayerJoined;
        PlayerManager.OnPlayerLeftGlobal   -= HandlePlayerLeft;
        TarodevController.PlayerController.OnPlayerReady -= HandlePlayerReady;
        TarodevController.PlayerController.OnTryStartGame -= TryStartGame;
    }

    private void HandlePlayerJoined(PlayerInput playerInput, Transform characterTf)
    {
        var readyTf = characterTf.Find("ReadyText");
        var readyTMP = readyTf.GetComponent<TextMesh>();

        var data = new PlayerSelectionData
        {
            IsReady = false,
            CharacterTransform = characterTf,
            ReadyText = readyTMP
        };
        
        _playerSelection[playerInput] = data;
        
        data.ReadyText.text = "Not Ready";
        data.ReadyText.color = notReadyColor;
        
        Debug.Log(
            $"Player joined: input={playerInput.playerIndex}, " +
            $"isReady={data.IsReady}, " +
            $"characterTf={data.CharacterTransform.name}, " +
            $"readyText=\"{data.ReadyText.text}\" color={data.ReadyText.color}",
            characterTf   // optional context so clicking the log selects this object
        );
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        _playerSelection.Remove(playerInput);
    }
    
    
    private void HandlePlayerReady(PlayerInput playerInput)
    {
        if (!_playerSelection.TryGetValue(playerInput, out var data))
            return;

        data.IsReady = !data.IsReady;

        if (data.ReadyText != null)
        {
            data.ReadyText.text = data.IsReady ? "Ready" : "Not Ready";
            data.ReadyText.color = data.IsReady ? readyColor : notReadyColor;  
        }
        _playerSelection[playerInput] = data;

    }

    private void TryStartGame()
    {
        if (_playerSelection.Count == 0)
            return; // nobody joined yet

        bool everyoneReady = _playerSelection.Values.All(p => p.IsReady);

        if (!everyoneReady)
        {
            Debug.Log("Not all players are ready yet.");
            return;
        }

        Debug.Log("All players ready, starting game!");
        GameEvents.ChangeState(GameState.SurpriseBoxState);
    }

    private void HandlePlayerSelectionStateExit()
    {
        foreach (var data in _playerSelection.Values)
        {
            if (data.ReadyText != null)
                data.ReadyText.text = string.Empty;
        }

        _playerSelection.Clear();        _playerSelection.Clear();
    }
    
    
}

public struct PlayerSelectionData
{
    public bool IsReady;
    public Transform CharacterTransform;
    public TextMesh ReadyText;   // or TextMeshProUGUI
}
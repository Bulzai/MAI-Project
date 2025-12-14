using UnityEngine;

public class LobbyInputHandler : MonoBehaviour
{
    void Update()
    {
        // Only handle input in PlayerSelectionState
        if (GameEvents.CurrentState != GameState.PlayerSelectionState)
            return;

        // Check for each player's input (up to 4 players)
        for (int i = 0; i < 4; i++)
        {
            // Y button (Ready)
            if (Input.GetKeyDown(GetYButton(i)))
            {
                Debug.Log($"Player {i} pressed Y (Ready)");
                if (PlayerManager.Instance != null)
                    PlayerManager.Instance.OnPlayerReadyPressed(i);
            }

            // A button (Start) - only player 1 can start
            if (i == 0 && Input.GetKeyDown(GetAButton(i)))
            {
                Debug.Log("Player 1 pressed A (Start)");
                if (PlayerManager.Instance != null)
                    PlayerManager.Instance.OnStartPressed();
            }

            // B button (Return)
            if (Input.GetKeyDown(GetBButton(i)))
            {
                Debug.Log($"Player {i} pressed B (Return)");
                if (PlayerManager.Instance != null)
                    PlayerManager.Instance.OnReturnPressed();
            }
        }
    }

    private KeyCode GetYButton(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return KeyCode.Joystick1Button3; // Y on Xbox, △ on PS
            case 1: return KeyCode.Joystick2Button3;
            case 2: return KeyCode.Joystick3Button3;
            case 3: return KeyCode.Joystick4Button3;
            default: return KeyCode.None;
        }
    }

    private KeyCode GetAButton(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return KeyCode.Joystick1Button0; // A on Xbox, X on PS
            case 1: return KeyCode.Joystick2Button0;
            case 2: return KeyCode.Joystick3Button0;
            case 3: return KeyCode.Joystick4Button0;
            default: return KeyCode.None;
        }
    }

    private KeyCode GetBButton(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0: return KeyCode.Joystick1Button1; // B on Xbox, ○ on PS
            case 1: return KeyCode.Joystick2Button1;
            case 2: return KeyCode.Joystick3Button1;
            case 3: return KeyCode.Joystick4Button1;
            default: return KeyCode.None;
        }
    }
}
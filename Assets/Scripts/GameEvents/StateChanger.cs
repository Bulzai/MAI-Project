using UnityEngine;

public class StateChanger : MonoBehaviour
{

    public void GoToMenuState()           => GameEvents.ChangeState(GameState.MenuState);
    public void GoToPlayerSelectState()   => GameEvents.ChangeState(GameState.PlayerSelectionState);
    public void GoToSurpriseBoxState()       => GameEvents.ChangeState(GameState.SurpriseBoxState);
    public void GoToPlaceItemState()      => GameEvents.ChangeState(GameState.PlaceItemState);
    public void GoToMainGameState()       => GameEvents.ChangeState(GameState.MainGameState);
    public void GoToScoreState()          => GameEvents.ChangeState(GameState.ScoreState);

}
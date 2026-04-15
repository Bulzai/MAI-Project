using UnityEngine;

public class StateChanger : MonoBehaviour
{

    public void GoToMenuState()           => GameEvents.ChangeState(GameState.MenuState);
    public void GoToPlayerSelectState()   => GameEvents.ChangeState(GameState.PlayerSelectionState);
    public void GoToItemDisplayState() => GameEvents.ChangeState(GameState.ItemDisplay);
    public void GoToAutomaticPlacementState() => GameEvents.ChangeState(GameState.AutomaticPlacement);
    public void GoToSurpriseBoxState()       => GameEvents.ChangeState(GameState.SurpriseBoxState);
    public void GoToPlaceItemState()      => GameEvents.ChangeState(GameState.PlaceItemState);
    public void GoToMainGameState()       => GameEvents.ChangeState(GameState.MainGameState);
    public void GoToScoreState()          => GameEvents.ChangeState(GameState.ScoreState);

}
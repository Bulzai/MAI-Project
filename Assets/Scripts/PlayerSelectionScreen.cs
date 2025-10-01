using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelectionScreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {

        if(PlayerManager.Instance.playerCount >0) GameEvents.ChangeState(GameState.SurpriseBoxState);
    }



}

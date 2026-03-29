using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreHubManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI currentMilkScore;
    [SerializeField] private TextMeshProUGUI currentHealth;
    [SerializeField] private PlayerHealthSystem playerHealthSystem;
    public int lastMilkScore;
    public int lastHealthLeft;
    
    public static ScoreHubManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        EmptyScoreTexts();
        GameEvents.OnMainGameStateExited += EmptyScoreTexts;
        

    }

    private void OnDestroy()
    {
        GameEvents.OnMainGameStateExited -= EmptyScoreTexts;
    }

    void Update()
    {
        if(GameEvents.CurrentState != GameState.MainGameState) return;
        currentMilkScore.text = "Milk \n" + PlayTestDataManager.Instance.roundMilkCollected.Value.ToString() + "/10";
        currentHealth.text = "Health \n" + playerHealthSystem.currentHealth.ToString();
        lastHealthLeft = playerHealthSystem.currentHealth;
        lastMilkScore = PlayTestDataManager.Instance.roundMilkCollected.Value;
    }

    private void EmptyScoreTexts()
    {
        currentMilkScore.text = "";
        currentHealth.text = "";
    }
}

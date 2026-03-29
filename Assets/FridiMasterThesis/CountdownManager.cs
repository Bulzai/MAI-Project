using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownManager : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TextMeshProUGUI tutorialText;
    private Coroutine countdownCoroutine;
    public static CountdownManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator StartCountdown( int countdown = 2, float timing = 0.7f)
    {
        PlaceItemState.CountDownStarted?.Invoke();
        GameEvents.ChangeState(GameState.MainGameState);
        
        while (countdown > 0)
        {
            tutorialText.gameObject.SetActive(true);
            countdownText.gameObject.SetActive(true);
            countdownText.text = countdown.ToString();



            yield return new WaitForSeconds(timing);
            countdown--;
        }

        PlaceItemState.CountDownFinished?.Invoke();
        countdownText.gameObject.SetActive(false);
        tutorialText.gameObject.SetActive(false);
        countdownCoroutine = null;               
        PlayTestDataManager.Instance.StartRoundTimer();
    }
}

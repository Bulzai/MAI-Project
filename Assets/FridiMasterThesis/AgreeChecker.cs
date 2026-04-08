using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class AgreeChecker : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject ConsentScreen;
    [SerializeField] private GameObject MainMenuScreen;
    [SerializeField] private GameObject BackgroundSurveyScreen;
    [SerializeField] private GameObject OpenInitialQuestionnaireButton;
    [SerializeField] private GameObject ContinueButton;
    [SerializeField] private CircleLoadAnim circleLoadAnim;
    [SerializeField] private GameObject circleAnimGO;
    [SerializeField] private GameObject TextToCopyGO;
    [SerializeField] private TMP_Text textToCopyText;
    [SerializeField] private GameObject headerGO;
    
    private string backgroundSurveyUrl = "https://forms.gle/NMEZHJnNVi6Cf5Rn8";

    private void Start()
    {
        inputField.onEndEdit.AddListener(CheckAgreement);
    }

    private void OnDestroy()
    {
        inputField?.onEndEdit.RemoveListener(CheckAgreement);
    }

    private async void CheckAgreement(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return;

        string trimmed = input.Trim();
        bool isAgree = trimmed.Equals("agree", StringComparison.OrdinalIgnoreCase);

        if (isAgree)
        {
            // set all child objects of ConsentScreen to inactive
            foreach (Transform child in ConsentScreen.transform)
            {
                child.gameObject.SetActive(false);
            }
            StartCoroutine(WaitForAnalyticsToInitialize());
            await UnityAnalyticsConsentManager.Instance.StartDataCollectionAsync();
        }
    }
    
    private IEnumerator WaitForAnalyticsToInitialize()
    {
        BackgroundSurveyScreen.SetActive(true);
        circleAnimGO.SetActive(true);
        circleLoadAnim.StartCircleAnim();
        while (CircleLoadAnim.playAnim)
        {
            Debug.Log("CircleLoadAnim.playAnim: " + CircleLoadAnim.playAnim);
            yield return null;
        }
        circleAnimGO.SetActive(false);

        while (CircleLoadAnim.playAnim)
        {
            yield return null;
        }
        circleAnimGO.SetActive(false);
        headerGO.SetActive(true);
        textToCopyText.text = "userId " + PlayTestDataManager.Instance.GetUserID() + "\n" +
                              "sessionId " + PlayTestDataManager.Instance.GetSessionID() + "\n" +
                              "previousGamesPlayed " + PlayTestDataManager.Instance.previousGamesPlayed;
        TextToCopyGO.SetActive(true);
        OpenInitialQuestionnaireButton.SetActive(true);

    }
    
    public void AfterInitialQuestionnaireButtonClicked()
    {
        Application.OpenURL(backgroundSurveyUrl);

        ContinueButton.SetActive(true);
    }
    public void AfterContinueButtonClicked()
    {
        TextToCopyGO.SetActive(false);
        MainMenuScreen.SetActive(true);
        BackgroundSurveyScreen.SetActive(false);
        ContinueButton.SetActive(false);
        ConsentScreen.SetActive(false);
    }
}
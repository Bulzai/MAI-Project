using System.Collections;
using TMPro;
using UnityEngine;

public class FinalQuestionnairePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionIDText;
    [SerializeField] private TMP_Text versionText;

    [Header("Questionnaire URL")]
    [SerializeField] private string questionnaireURL;

    [Header("Copy Feedback")]
    [SerializeField] private GameObject copiedFeedbackText;
    [SerializeField] private float feedbackDuration = 1.2f;

    private Coroutine feedbackRoutine;

    private void OnEnable()
    {
        if (sessionIDText != null)
            sessionIDText.text = "Your Session ID: " + SessionData.SessionID;

        if (versionText != null)
            versionText.text = "Game Version: A";

        if (copiedFeedbackText != null)
            copiedFeedbackText.SetActive(false);
    }

    public void CopySessionID()
    {
        GUIUtility.systemCopyBuffer = SessionData.SessionID;

        if (copiedFeedbackText != null)
        {
            if (feedbackRoutine != null)
                StopCoroutine(feedbackRoutine);

            feedbackRoutine = StartCoroutine(ShowCopiedFeedback());
        }
    }

    public void OpenQuestionnaire()
    {
        GUIUtility.systemCopyBuffer = SessionData.SessionID;

        if (!string.IsNullOrEmpty(questionnaireURL))
        {
            Application.OpenURL(questionnaireURL);
        }
        else
        {
            Debug.LogWarning("Questionnaire URL is empty.");
        }
    }

    private IEnumerator ShowCopiedFeedback()
    {
        copiedFeedbackText.SetActive(true);

        yield return new WaitForSeconds(feedbackDuration);

        copiedFeedbackText.SetActive(false);
        feedbackRoutine = null;
    }
}
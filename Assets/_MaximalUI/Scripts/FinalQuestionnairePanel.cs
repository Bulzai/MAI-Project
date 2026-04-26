using System.Collections;
using TMPro;
using UnityEngine;

public class FinalQuestionnairePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionIDText;

    [Header("Questionnaire URL")]
    [SerializeField] private string questionnaireURL;

    [SerializeField] private GameObject copiedFeedbackText;
    [SerializeField] private float feedbackDuration = 1.2f;

    private void OnEnable()
    {
        if (sessionIDText != null)
        {
            sessionIDText.text = "Your Session ID: " + SessionData.SessionID;
        }
    }

    public void CopySessionID()
    {
        string sessionID = sessionIDText.text;

        GUIUtility.systemCopyBuffer = sessionID;

        // Show feedback
        if (copiedFeedbackText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowCopiedFeedback());
        }
    }

    private IEnumerator ShowCopiedFeedback()
    {
        copiedFeedbackText.SetActive(true);

        yield return new WaitForSeconds(feedbackDuration);

        copiedFeedbackText.SetActive(false);
    }

    public void OpenQuestionnaire()
    {
        GUIUtility.systemCopyBuffer = SessionData.SessionID;
        Debug.Log("Opening questionnaire. Session ID copied: " + SessionData.SessionID);

        if (!string.IsNullOrEmpty(questionnaireURL))
        {
            Application.OpenURL(questionnaireURL);
        }
        else
        {
            Debug.LogWarning("Questionnaire URL is empty.");
        }
    }
}
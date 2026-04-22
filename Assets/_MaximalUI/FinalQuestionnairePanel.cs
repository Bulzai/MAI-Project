using TMPro;
using UnityEngine;

public class FinalQuestionnairePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionIDText;

    [Header("Questionnaire URL")]
    [SerializeField] private string questionnaireURL;

    private void OnEnable()
    {
        if (sessionIDText != null)
        {
            sessionIDText.text = "Your Session ID: " + SessionData.SessionID;
        }
    }

    public void CopySessionID()
    {
        GUIUtility.systemCopyBuffer = SessionData.SessionID;
        Debug.Log("Session ID copied: " + SessionData.SessionID);
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
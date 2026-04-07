using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class CopyAndHighlightByButton : MonoBehaviour
{
    private TMP_Text _textField;
    [SerializeField] private TMP_Text CopiedFeedbackText;


    private void Awake()
    {
        _textField = GetComponent<TMP_Text>();
    }

    public void CopyToClipboard()
    {
        string textToCopy = _textField.text;
        string copiedTextWithoutTags = Regex.Replace(textToCopy, "<.*?>", string.Empty);
        GUIUtility.systemCopyBuffer = copiedTextWithoutTags;
        Debug.Log("Copied to clipboard:" + copiedTextWithoutTags);
        StartCoroutine(ShowCopiedFeedback());
    }
    
    private IEnumerator ShowCopiedFeedback()
    {
        if (CopiedFeedbackText != null)
        {
            CopiedFeedbackText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f); // Show feedback for 2 seconds
            CopiedFeedbackText.gameObject.SetActive(false);
        }
    }
    
}

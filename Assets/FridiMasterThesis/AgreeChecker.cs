using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class AgreeChecker : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject ConsentScreen;
    [SerializeField] private GameObject MainMenuScreen;
    
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
            ConsentScreen.SetActive(false);
            MainMenuScreen.SetActive(true);
            await UnityAnalyticsConsentManager.Instance.StartDataCollectionAsync();
        }
    }
    
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScreenScript : MonoBehaviour
{
    [SerializeField] private GameObject consentScreen;
    [SerializeField] private GameObject continueToConsentScreenButton;

    [SerializeField] private GameObject hideAfter10SecondsTxt;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private float timer = 10f;
    // Update is called once per frame
    void Update()
    {
        timer = timer - Time.deltaTime;
        if (timer <= 0f)
        {
            continueToConsentScreenButton.SetActive(true);
            hideAfter10SecondsTxt.SetActive(false);
        }

    }
    
    public void OnContinueToConsentScreenButtonClicked()
    {
        if (consentScreen != null)
        {
            consentScreen.SetActive(true);
            this.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Consent screen reference is not set.");
        }
    }
}

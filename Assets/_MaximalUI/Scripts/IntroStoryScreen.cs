using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class IntroStoryScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private Button continueButton;

    [Header("Flow")]
    [SerializeField] private StateChanger stateChanger;
    [SerializeField] private GameObject playerSelectionGameObject;
    [SerializeField] private GameObject playerSelectionState;
    [SerializeField] private float lineDelay = 0.8f;

    private bool skipRequested = false;
    private bool isTyping = false;

    private string[] storyLines =
    {
        "Two rivals. One arena.",
        "",
        "Cutesy and Jokesy have entered a chaotic challenge",
        "where only one can survive.",
        "",
        "Collect milk to stay alive.",
        "Use auras to outplay your opponent.",
        "React fast. Stay alert.",
        "",
        "Only one will make it to the end.",
        "",
        "Are you ready?"
    };

    public void StartIntro()
    {
        gameObject.SetActive(true);

        skipRequested = false;
        isTyping = true;

        if (storyText != null)
            storyText.text = "";

        if (continueButton != null)
            continueButton.gameObject.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(PlayIntro());
    }

    private void Update()
    {
        if (!isTyping) return;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            skipRequested = true;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            skipRequested = true;

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
            skipRequested = true;
    }

    private IEnumerator PlayIntro()
    {
        if (storyText == null)
        {
            Debug.LogWarning("StoryText is not assigned!");
            yield break;
        }

        storyText.text = "";

        foreach (string line in storyLines)
        {
            if (skipRequested)
                break;

            storyText.text += line + "\n";
            yield return new WaitForSeconds(lineDelay);
        }

        if (skipRequested)
            storyText.text = string.Join("\n", storyLines);

        isTyping = false;

        if (continueButton != null)
            continueButton.gameObject.SetActive(true);
    }

    public void ContinueToNext()
    {
        gameObject.SetActive(false);

        if (playerSelectionGameObject != null)
            playerSelectionGameObject.SetActive(true);

        if (stateChanger != null)
            stateChanger.GoToPlayerSelectState();

        if (playerSelectionState != null)
            playerSelectionState.SetActive(true);
    }
}
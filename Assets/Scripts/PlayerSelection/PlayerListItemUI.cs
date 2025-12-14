using UnityEngine;
using TMPro;

public class PlayerListItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text readyStatusText;

    public void SetReady(bool isReady)
    {
        if (readyStatusText != null)
        {
            readyStatusText.text = isReady ? "READY!" : "NOT READY!";
            readyStatusText.color = isReady ? Color.green : Color.red;
        }
        else
        {
            Debug.LogError("readyStatusText is null!");
        }
    }
}
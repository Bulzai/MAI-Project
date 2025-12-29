using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class ScoreboardRowUI : MonoBehaviour
{
    
    
    // event for menu button

    [SerializeField] private Image avatar;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image barFill;      // Image Type = Filled (Horizontal)
    [SerializeField] private TMP_Text roundText; // optional
    [SerializeField] private TMP_Text totalText; // optional
    [SerializeField] private TMP_Text placeText;   // <-- NEW
    [SerializeField] private Image background;
    [SerializeField] private Sprite spriteBackground;

    private static float EaseOutCubic(float t) => 1 - Mathf.Pow(1 - t, 3);
    public void SetPlace(int place)
    {
        if (placeText) placeText.text = place.ToString() + ".";
    }

    public void SetStatic(PlayerInput pi, Sprite avatarSprite)
    {
        if (nameText) nameText.text = pi.gameObject.name;
        if (avatar) avatar.sprite = avatarSprite;
        if (background) background.sprite = spriteBackground;
    }

    public IEnumerator AnimateScores(int oldTotal, int roundGain, int newTotal, int maxTotal, float duration = 1.1f)
    {
        maxTotal = Mathf.Max(1, maxTotal);
        if (roundText) roundText.text = (roundGain > 0 ? $"+{roundGain}" : "+0");
        if (totalText) totalText.text = oldTotal.ToString();

        float start = Mathf.Clamp01((float)oldTotal / maxTotal);
        float end = Mathf.Clamp01((float)newTotal / maxTotal);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = EaseOutCubic(Mathf.Clamp01(t));
            float f = Mathf.Lerp(start, end, k);
            if (barFill) barFill.fillAmount = f;
            if (totalText) totalText.text = Mathf.RoundToInt(Mathf.Lerp(oldTotal, newTotal, k)).ToString();
            yield return null;
        }
        if (barFill) barFill.fillAmount = end;
        if (totalText) totalText.text = newTotal.ToString();
        
    }

    public void SetAvatar(Sprite s) 
    { 
        if (avatar) avatar.sprite = s; 
    }

}

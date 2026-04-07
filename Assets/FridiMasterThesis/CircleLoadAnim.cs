using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleLoadAnim : MonoBehaviour
{
    public Image image;
    public List<Sprite> sprites;
    private float animSpeed = 0.15f;
    private int index;
    public static bool playAnim = false;
    
    public void StartCircleAnim()
    {
        StartCoroutine(StartAnim());
    }

    public  IEnumerator StartAnim()
    {
        while(playAnim)
        {
            yield return new WaitForSeconds(animSpeed);
            index++;
            if(index >= sprites.Count)
                index = 0;
            else
                image.sprite = sprites[index];
        }
    }
}

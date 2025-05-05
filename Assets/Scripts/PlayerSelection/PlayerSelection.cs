using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelection : MonoBehaviour
{
    public GameObject SurpriseBox;
    public GameObject PlayerSelectionObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartSurpriseBoxSequence()
    {
        SurpriseBox.SetActive(true);
        PlayerSelectionObject.SetActive(false);
    }
}

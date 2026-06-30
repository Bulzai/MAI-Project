using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestingManager : MonoBehaviour
{
    public static TestingManager Instance { get; private set; }

    private string sessionID;
    [SerializeField] private TMP_Text sessionIDGO;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // generate session ID
        // globally unique id
        sessionID = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
        sessionIDGO.text = "session id: " + sessionID.ToString();
        Debug.Log($" ----- Session ID: {sessionID} ----- ");
    }

    public string GetSessionID()
    { return sessionID; }
}

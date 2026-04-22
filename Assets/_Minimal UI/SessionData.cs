using UnityEngine;

public class SessionData : MonoBehaviour
{
    public static string SessionID;
    public static string BuildType = "Minimal"; // Change this in Maximal build

    private void Awake()
    {
        if (string.IsNullOrEmpty(SessionID))
        {
            SessionID = GenerateCode();
            Debug.Log("Generated Session ID: " + SessionID);
        }
    }

    private string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code = "";

        for (int i = 0; i < 4; i++)
        {
            code += chars[Random.Range(0, chars.Length)];
        }

        return code;
    }
}
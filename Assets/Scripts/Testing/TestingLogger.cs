using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class TestingLogger
{
    private const string mode = "playerled";

    public static void LogToCSV(string data, string file = "EventMetricTracker")
    {
        string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

        string path = Path.Combine(documentsPath, file);
        //string path = Application.persistentDataPath + "/" + file + ".csv";

        // check if files exists
        bool fileExists = File.Exists(path);

        // TO DO ADD SO SESSION ID AND MODE ON TOP IS ONLY ON TOP
        using (StreamWriter writer = File.AppendText(path))
        {
            if (!fileExists)
            {
                writer.WriteLine($"{mode}");
                writer.WriteLine($"{TestingManager.Instance.GetSessionID()}");
                writer.WriteLine("Round,EventType,Name,Value");
            }
            writer.WriteLine($"{data}");
        }
    }
}
 
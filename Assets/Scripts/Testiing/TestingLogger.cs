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

        using (StreamWriter writer = File.AppendText(path))
        {
            if (!fileExists) writer.WriteLine("Mode,SessionID,Round,EventType,Name,Value");
            writer.WriteLine($"{mode},{data}");
        }
    }
}
 
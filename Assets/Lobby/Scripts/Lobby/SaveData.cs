using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveData : MonoBehaviour
{
    private static string filePath;

    private static void Init()
    {
        filePath = Application.persistentDataPath + "/Data.dat";
        //File.Delete(filePath);

        if (File.Exists(filePath) == false)
        {
            var file = File.Create(filePath);

            file.Close();
        }
    }       

    private static void CheckPath()
    {
        if (string.IsNullOrEmpty(filePath) == true)
        {
            Init();
        }
    }

    public static void Save()
    {
        string saveString = DataPacker.PackData();

        StreamWriter file = new StreamWriter(filePath);

        file.WriteLine(saveString);
        file.Flush();

        file.Close();
    }

    public static string Load()
    {
        CheckPath();
        string[] lines = File.ReadAllLines(filePath);

        string temp = "";

        if (lines.Length > 0)
            temp = lines[0];

        return temp;
    }
}

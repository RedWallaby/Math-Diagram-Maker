using UnityEngine;
using System.IO;

public class JsonManager
{
    static string workingDirectory;

    public static void SetWorkingDirectory(string directory)
    {
        workingDirectory = directory;
    }

    public static string GetWorkingDirectory()
    {
        if (string.IsNullOrEmpty(workingDirectory))
        {
            Debug.LogError("Working directory is not set.");
            return null;
        }
        return Application.persistentDataPath + $"/{workingDirectory}";
    }

    public static void CreateDirectory(string directory)
    {
        if (string.IsNullOrEmpty(directory))
        {
            Debug.LogError("Working directory is not set.");
            return;
        }

        string path = Application.persistentDataPath + $"/{directory}";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log($"Created directory: {path}");
        }
    }

    public static string GetJsonFilePath(string fileName)
    {
        return Application.persistentDataPath + $"/{workingDirectory}/" + fileName + ".json";
    }

    public static string[] GetFilePaths()
    {
        if (string.IsNullOrEmpty(workingDirectory))
        {
            Debug.LogError("Working directory is not set.");
            return null;
        }

        string path = GetWorkingDirectory();
        if (!Directory.Exists(path))
        {
            Debug.LogError($"Directory does not exist: {path}");
            return null;
        }

        return Directory.GetFiles(path, "*.json");
    }
}

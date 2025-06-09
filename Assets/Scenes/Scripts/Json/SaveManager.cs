using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static JsonDiagram;

public class SaveManager : MonoBehaviour
{
    public Diagram diagram;

    public TMP_InputField inputField;
    public TMP_Text title;
    public GameObject alertObject;
    public TMP_Text alertText;
    public TMP_Text saveText;

    private bool saved;

    /// <summary>
    /// Opens the save menu and sets the title and input field text based on the current diagram name
    /// </summary>
    public void OpenMenu()
    {
        if (string.IsNullOrEmpty(diagram.diagramName))
        {
            title.text = "New Diagram";
            inputField.text = "New Diagram";
        }
        else
        {
            title.text = diagram.diagramName;
            inputField.text = diagram.diagramName;
        }
        SetSaved(false);
        alertObject.SetActive(false);
    }

    /// <summary>
    /// Sets the saved state of the diagram and updates the save button text and color accordingly
    /// </summary>
    /// <param name="value">Whether the diagram is saved or not</param>
    public void SetSaved(bool value) {
        saved = value;
        if (value)
        {
            saveText.text = "Saved!";
            saveText.color = Color.green;
        }
        else
        {
            saveText.text = "Save";
            saveText.color = Color.white;
        }
    }
    /// <summary>
    /// Saves the current diagram with the name from the input field
    /// Validates the name and checks if it already exists before saving
    /// </summary>
    public void SaveCurrentDiagram()
    {
        if (saved) return;
        string name = inputField.text;
        if (!ValidateName(name)) return;

        // Rename the file if the name has changed
        string currentPath = JsonManager.GetJsonFilePath(diagram.diagramName);
        if (File.Exists(currentPath) && name.ToLower() != diagram.diagramName.ToLower())
        {
            File.Move(currentPath, JsonManager.GetJsonFilePath(name));
        }

        SetSaved(true);
        title.text = name;
        diagram.diagramName = name;
        SaveJsonDiagram(inputField.text);
    }

    /// <summary>
    /// Validates the name of the diagram before saving
    /// </summary>
    /// <param name="name">The diagram name to be saved</param>
    /// <returns>Whether the name was valid</returns>
    public bool ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            SetError("Please enter a name for the diagram.");
            return false;
        }
        string[] existingFiles = JsonManager.GetFilePaths();
        if (name != diagram.diagramName && existingFiles.Any(file => Path.GetFileNameWithoutExtension(file).ToLower() == name.ToLower())) // Needs to be ToLower() to avoid case sensitivity issues
        {
            SetError("A diagram with this name already exists.");
            return false;
        }
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (name.Contains(c))
            {
                SetError("The name cannot contain any of the following \\ / : * ? \" < > |");
                return false;
            }
        }
        if (name.Length > 50)
        {
            SetError("The name cannot be longer than 50 characters.");
            return false;
        }
        alertObject.SetActive(false);
        return true;
    }

    public void SaveJsonDiagram(string name)
    {
        JsonDiagram jsonDiagram = SaveDiagram();
        jsonDiagram.name = name;
        string diagram = JsonUtility.ToJson(jsonDiagram, true);
        string filePath = JsonManager.GetJsonFilePath(name);
        File.WriteAllText(filePath, diagram);
    }

    /// <summary>
    /// Saves the current diagram as a PNG file to a user-selected location
    /// </summary>
    public void SaveCurrentDiagramToFile()
    {
        string path = EditorUtility.SaveFilePanel("Save Diagram", JsonManager.GetWorkingDirectory(), diagram.diagramName, "png");
        if (string.IsNullOrEmpty(path)) return; // User cancelled the save dialog

        // Setup RenderTexture
        diagram.SetBoundsOnTextureCamera();
        RenderTexture texture = new(1024, 1024, 32);
        diagram.textureCamera.targetTexture = texture;
        diagram.textureCamera.Render();

        // Convert RenderTexture to Texture2D and save as PNG
        Texture2D tex = new(1024, 1024);
        RenderTexture.active = texture;
        tex.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());

        diagram.notification.SetNotification("Diagram saved as " + Path.GetFileName(path), 5f);
    }

    public void SetError(string error)
    {
        alertObject.SetActive(true);
        alertText.text = error;
    }

    /// <summary>
    /// Converts the current diagram to a <c>JsonDiagram</c> object
    /// </summary>
    /// <returns>The new <c>JsonDiagram</c></returns>
    public JsonDiagram SaveDiagram()
    {
        Dictionary<Element, int> elementToIdMap = FillDictionary();

        JsonDiagram jsonDiagram = new()
        {
            points = new List<JsonPoint>(),
            lines = new List<JsonLine>(),
            circles = new List<JsonCircle>(),
            angles = new List<JsonAngle>()
        };

        foreach (Element element in diagram.elements)
        {
            if (element is Point point)
            {
                jsonDiagram.points.Add(new JsonPoint(point, elementToIdMap));
            }
            else if (element is Line line)
            {
                jsonDiagram.lines.Add(new JsonLine(line, elementToIdMap));
            }
            else if (element is Circle circle)
            {
                jsonDiagram.circles.Add(new JsonCircle(circle, elementToIdMap));
            }
            else if (element is Angle angle)
            {
                jsonDiagram.angles.Add(new JsonAngle(angle, elementToIdMap));
            }
        }

        return jsonDiagram;
    }

    /// <summary>
    /// Fills a dictionary mapping each <c>Element</c> to an index
    /// </summary>
    /// <returns>The created mapping</returns>
    public Dictionary<Element, int> FillDictionary()
    {
        Dictionary<Element, int> elementToIdMap = new();
        for (int i = 0; i < diagram.elements.Count; i++)
        {
            Element element = diagram.elements[i];
            if (!elementToIdMap.ContainsKey(element))
            {
                elementToIdMap.Add(element, i);
            }
        }
        return elementToIdMap;
    }
}

using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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
        saveText.text = "Save";
        saveText.color = Color.white;
        saved = false;
        alertObject.SetActive(false);
    }

    public void SaveCurrentDiagram()
    {
        if (saved) return;
        string name = inputField.text;
        if (!ValidateName(name)) return;

        // Rename the file if the name has changed
        string currentPath = Application.persistentDataPath + $"/{diagram.diagramName}.json";
        if (File.Exists(currentPath) && name.ToLower() != diagram.diagramName.ToLower())
        {
            File.Move(currentPath, Application.persistentDataPath + $"/{name}.json");
        }

        saveText.text = "Saved!";
        saveText.color = Color.green;
        saved = true;
        title.text = name;
        diagram.diagramName = name;
        SaveJsonDiagram(inputField.text);
    }

    public bool ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            alertObject.SetActive(true);
            alertText.text = "Please enter a name for the diagram.";
            return false;
        }
        string[] existingFiles = Directory.GetFiles(Application.persistentDataPath, "*.json", SearchOption.TopDirectoryOnly);
        if (name != diagram.diagramName && existingFiles.Any(file => Path.GetFileNameWithoutExtension(file).ToLower() == name.ToLower())) // NEEDS TO BE TO LOWER AS WINDOWS IS CASE INSENSITIVE
        {
            alertObject.SetActive(true);
            alertText.text = "A diagram with this name already exists.";
            return false;
        }
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (name.Contains(c))
            {
                alertObject.SetActive(true);
                alertText.text = "The name cannot contain any of the following \\ / : * ? \" < > |";
                return false;
            }
        }
        alertObject.SetActive(false);
        return true;
    }

    public void SaveJsonDiagram(string name)
    {
        JsonDiagram jsonDiagram = SaveDiagram();
        jsonDiagram.name = name;
        string diagram = JsonUtility.ToJson(jsonDiagram, true);
        string filePath = Application.persistentDataPath + $"/{name}.json";
        File.WriteAllText(filePath, diagram);
    }

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

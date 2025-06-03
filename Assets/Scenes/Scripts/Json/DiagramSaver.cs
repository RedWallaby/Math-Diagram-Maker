using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JsonDiagram;

public class DiagramSaver : MonoBehaviour
{
    public Diagram diagram;

    Dictionary<Element, int> elementToIdMap = new();

    public TMP_InputField inputField;

    public void OpenMenu() // SET THE TITLE OF THE MENU TO THE CURRENT NAME OF THE DIAGRAM THEN UPDATE IT ONCE THE PROJECT IS SAVED
    {
        inputField.text = diagram.diagramName;
    } // TODO ALSO MAKE IT SO THE INPUT FIELD GIVES FEEDBACK FROM USER INPUT


    public void SaveCurrentDiagram() // TODO WE CAN IMPLEMENT SANITATION HERE
    {
        string name = inputField.text;
        if (string.IsNullOrEmpty(name)) return; //<------------

        // Rename the file if the name has changed
        string currentPath = Application.persistentDataPath + $"/{diagram.diagramName}.json";
        if (File.Exists(currentPath) && name != diagram.diagramName)
        {
            File.Move(currentPath, Application.persistentDataPath + $"/{name}.json");
        }

        diagram.diagramName = name;
        SaveJsonDiagram(inputField.text);
    }

    public JsonDiagram GetJsonDiagram(string path)
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<JsonDiagram>(json);
    }

    public void SaveJsonDiagram(string name)
    {
        JsonDiagram jsonDiagram = SaveDiagram();
        jsonDiagram.name = name;
        string diagram = JsonUtility.ToJson(jsonDiagram, true);
        string filePath = Application.persistentDataPath + $"/{name}.json";
        File.WriteAllText(filePath, diagram);
        Debug.Log(filePath);
    }

    public JsonDiagram LoadJsonDiagram(string path)
    {
        if (File.Exists(path))
        {
            JsonDiagram jsonDiagram = GetJsonDiagram(path);
            LoadFromDiagram(jsonDiagram);
            Debug.Log("Loaded diagram name: " + jsonDiagram.name);
            return jsonDiagram;
        }
        else
        {
            Debug.LogError("File not found: " + path);
            return null;
        }
    }

    public void FillDictionary()
    {
        elementToIdMap.Clear();
        for (int i = 0; i < diagram.elements.Count; i++)
        {
            Element element = diagram.elements[i];
            Debug.Log(element.gameObject.name + " ID: " + i);
            if (!elementToIdMap.ContainsKey(element))
            {
                elementToIdMap.Add(element, i);
            }
        }
    }

    public JsonDiagram SaveDiagram() // TODO IMPLEMENT LABEL SAVING
    {
        FillDictionary();

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

    public void LoadFromDiagram(JsonDiagram jsonDiagram)
    {
        diagram.diagramName = jsonDiagram.name;

        diagram.ClearDiagram();

        Dictionary<int, Point> idToPointMap = new();
        LoadPoints(jsonDiagram.points, idToPointMap, jsonDiagram);
        LoadLines(jsonDiagram.lines, idToPointMap, jsonDiagram);
        LoadCircles(jsonDiagram.circles, idToPointMap, jsonDiagram);
        LoadAngles(jsonDiagram.angles, idToPointMap, jsonDiagram);
    }

    public void LoadPoints(List<JsonPoint> jsonPoints, Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonPoint jsonPoint in jsonDiagram.points)
        {
            Point point = diagram.CreatePoint(jsonPoint.position);
            point.percentage = jsonPoint.percentage;
            idToPointMap[jsonPoint.id] = point;
            diagram.elements.Add(point);
        }
    }

    public void LoadLines(List<JsonLine> jsonLines, Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonLine jsonLine in jsonDiagram.lines)
        {
            Line line = diagram.CreateLine();
            for (int i = 0; i < jsonLine.pointIDs.Length; i++)
            {
                Point point = idToPointMap[jsonLine.pointIDs[i]];
                line.points[i] = point;
                point.attatchedLines.Add(line);
            }
            line.ForceUpdateLineRenderer();
            line.SetPosition();
            foreach (int pointID in jsonLine.attachedPointIDs)
            {
                line.AttachPoint(idToPointMap[pointID]);
            }
            diagram.elements.Add(line);
        }
    }

    public void LoadCircles(List<JsonCircle> jsonCircles, Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonCircle jsonCircle in jsonDiagram.circles)
        {
            Point point = idToPointMap[jsonCircle.centreID];
            Circle circle = diagram.CreateCircle(point.position);
            circle.centre = point;
            point.circles.Add(circle);
            foreach (int pointID in jsonCircle.attachedPointIDs)
            {
                circle.AttachPoint(idToPointMap[pointID]);
            }

            circle.radius = jsonCircle.radius;
            circle.DrawCircle();
            diagram.elements.Add(circle);
        }
    }

    public void LoadAngles(List<JsonAngle> jsonAngles, Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonAngle jsonAngle in jsonDiagram.angles)
        {
            Angle angle = diagram.CreateAngle();
            angle.start = idToPointMap[jsonAngle.startID];
            angle.centre = idToPointMap[jsonAngle.centreID];
            angle.end = idToPointMap[jsonAngle.endID];
            diagram.elements.Add(angle);
            angle.transform.position = angle.centre.position;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using static JsonDiagram;

public class DiagramSaver : MonoBehaviour
{
    public Diagram diagram;

    Dictionary<Element, int> elementToIdMap = new();

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveJsonDiagram();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadJsonDiagram();
        }
    }


    public void SaveJsonDiagram()
    {
        JsonDiagram jsonDiagram = SaveDiagram(this.diagram);
        string diagram = JsonUtility.ToJson(jsonDiagram, true);
        Debug.Log("saving: " + diagram);
        Debug.Log("Elements: " + jsonDiagram.points.Count);
        string filePath = Application.persistentDataPath + "/diagram.json";
        System.IO.File.WriteAllText(filePath, diagram);
        Debug.Log(filePath);

    }

    public void LoadJsonDiagram()
    {
        string filePath = Application.persistentDataPath + "/diagram.json";
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            JsonDiagram jsonDiagram = JsonUtility.FromJson<JsonDiagram>(json);
            Debug.Log("Loaded diagram: " + json);
            Debug.Log("Elements: " + jsonDiagram.points.Count);
            LoadFromDiagram(jsonDiagram, diagram);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
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

    public JsonDiagram SaveDiagram(Diagram diagram)
    {
        this.diagram = diagram;
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

    public void LoadFromDiagram(JsonDiagram jsonDiagram, Diagram diagram)
    {
        // Reset Diagram
        foreach (Element element in diagram.elements)
        {
            if (element != null)
            {
                Debug.Log("Deleting element: " + element.gameObject.name);
                element.Delete();
            }
            else
            {
                Debug.Log("Found null element in diagram elements list.");
            }
        }
        diagram.elements.Clear();

        Dictionary<int, Point> idToPointMap = new();
        foreach (JsonPoint jsonPoint in jsonDiagram.points)
        {
            Point point = diagram.CreatePoint(jsonPoint.position);
            point.percentage = jsonPoint.percentage;
            idToPointMap[jsonPoint.id] = point;
        }
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
        }
        foreach (JsonCircle jsonCircle in jsonDiagram.circles)
        {
            Point point = idToPointMap[jsonCircle.centreID];
            Circle circle = diagram.CreateCircle(point.position);
            circle.centre = point;
            foreach (int pointID in jsonCircle.attachedPointIDs)
            {
                circle.AttachPoint(idToPointMap[pointID]);
            }
            circle.radius = jsonCircle.radius;
            circle.CreateCircle();
        }
        foreach (JsonAngle jsonAngle in jsonDiagram.angles)
        {
            Angle angle = diagram.CreateAngle();
            angle.start = idToPointMap[jsonAngle.startID];
            angle.centre = idToPointMap[jsonAngle.centreID];
            angle.end = idToPointMap[jsonAngle.endID];
        }
    }
}

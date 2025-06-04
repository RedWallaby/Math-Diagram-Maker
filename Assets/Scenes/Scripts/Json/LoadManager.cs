using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static JsonDiagram;

public class LoadManager : MonoBehaviour
{
    public Diagram diagram;
    public Camera textureCamera;

    public GameObject loadObjectPrefab;
    public Transform content;

    public Color unselectedColor;
    public Color selectedColor;

    public LoadObject loadObject;
    public List<LoadObject> loadObjects;

    public void SelectLoadObject(LoadObject obj)
    {
        if (loadObject != null)
        {
            loadObject.mainBody.color = unselectedColor; // Reset previous selection color
        }
        loadObject = obj;
        loadObject.mainBody.color = selectedColor; // Highlight the selected object
    }

    public void DeleteLoadObject(LoadObject obj)
    {
        string path = Application.persistentDataPath + $"/{obj.diagram.name}.json";
        File.Delete(path);
        Destroy(obj.gameObject);
    }

    public void OpenLoadDiagram()
    {
        if (loadObject == null) return;
        LoadFromDiagram(loadObject.diagram);
        diagram.SetActive(true);
        gameObject.SetActive(false);
        loadObjects.Remove(loadObject);
        Destroy(loadObject.gameObject);
    }

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            LoadNewTextures();
        }
    }

    public void OpenLoadMenu()
    {
        DeselectLoadObject();
        LoadNewTextures();
    }

    public void DeselectLoadObject()
    {
        foreach (LoadObject obj in loadObjects)
        {
            if (obj.mainBody.color == selectedColor)
            {
                obj.mainBody.color = unselectedColor;
                break; // Leave loop after resetting the only possible selected object
            }
        }
        loadObject = null; // Clear the currently selected object
    }

    public void LoadNewTextures()
    {
        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string path in paths)
        {
            JsonDiagram jsonDiagram = GetJsonDiagram(path);
            if (CheckIfExists(jsonDiagram)) continue; // Skip if diagram already exists

            LoadJsonDiagram(path);
            diagram.SetBoundsOnCamera(textureCamera);

            GameObject newObj = Instantiate(loadObjectPrefab, content);
            newObj.transform.SetAsFirstSibling(); // Allows most recently loaded diagram to be on top
            LoadObject loadObject = newObj.GetComponent<LoadObject>();

            RenderTexture(loadObject);
            loadObject.title.text = diagram.diagramName;
            loadObject.diagram = jsonDiagram;
            loadObject.loadManager = this;
            loadObjects.Add(loadObject);
        }
    }

    public bool CheckIfExists(JsonDiagram jsonDiagram)
    {
        foreach (LoadObject loadObj in loadObjects)
        {
            if (loadObj.diagram.name.Equals(jsonDiagram.name)) return true;
        }
        return false;
    }

    public void RenderTexture(LoadObject loadObj)
    {
        RenderTexture texture = new(1024, 1024, 32);
        textureCamera.targetTexture = texture;
        textureCamera.Render();

        Material material = new(Shader.Find("UI/Default"))
        {
            mainTexture = texture
        };
        loadObj.image.material = material;
    }

    public JsonDiagram GetJsonDiagram(string path)
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<JsonDiagram>(json);
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

    public void LoadFromDiagram(JsonDiagram jsonDiagram)
    {
        diagram.diagramName = jsonDiagram.name;

        diagram.ClearDiagram();

        Dictionary<int, Point> idToPointMap = new();
        LoadPoints(idToPointMap, jsonDiagram);
        LoadLines(idToPointMap, jsonDiagram);
        LoadCircles(idToPointMap, jsonDiagram);
        LoadAngles(idToPointMap, jsonDiagram);

        // Initialise labels for all elements
        foreach (Element element in diagram.elements)
        {
            Debug.Log("Loading element: " + element.gameObject.name);
            if (element.isLabelVisible)
            {
                Debug.Log("Creating label for: " + element.gameObject.name);
                diagram.label.CreateLabelObject(element);
                element.SetLabel();
            }
        }
    }

    public void LoadPoints(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonPoint jsonPoint in jsonDiagram.points)
        {
            Point point = diagram.CreatePoint(jsonPoint.position);
            point.percentage = jsonPoint.percentage;
            idToPointMap[jsonPoint.id] = point;

            point.isLabelVisible = jsonPoint.isLabelVisible;
            point.labelOverride = jsonPoint.labelOverride;
            diagram.elements.Add(point);
        }
    }

    public void LoadLines(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
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

            line.isLabelVisible = jsonLine.isLabelVisible;
            line.labelOverride = jsonLine.labelOverride;
            diagram.elements.Add(line);
        }
    }

    public void LoadCircles(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
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

            circle.isLabelVisible = jsonCircle.isLabelVisible;
            circle.labelOverride = jsonCircle.labelOverride;
            diagram.elements.Add(circle);
        }
    }

    public void LoadAngles(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonAngle jsonAngle in jsonDiagram.angles)
        {
            Angle angle = diagram.CreateAngle();
            angle.start = idToPointMap[jsonAngle.startID];
            angle.centre = idToPointMap[jsonAngle.centreID];
            angle.end = idToPointMap[jsonAngle.endID];

            angle.isLabelVisible = jsonAngle.isLabelVisible;
            angle.labelOverride = jsonAngle.labelOverride;
            diagram.elements.Add(angle);
            angle.transform.position = angle.centre.position;
        }
    }
}

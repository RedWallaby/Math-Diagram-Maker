using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static JsonDiagram;

public class LoadManager : MonoBehaviour
{
    public Diagram diagram;

    public Color unselectedColor;
    public Color selectedColor;

    public Transform content;
    public GameObject loadObjectPrefab;

    private LoadObject loadObject;
    private List<LoadObject> loadObjects;

    /// <summary>
    /// Initialises the load menu
    /// </summary>
    public void OpenLoadMenu()
    {
        DeselectLoadObject();
        LoadDiagramObjects();
    }

    /// <summary>
    /// Sets the currently selected <c>LoadObject</c> and updates its colour to indicate selection
    /// </summary>
    /// <param name="obj">The referenced <c>LoadObject</c></param>
    public void SelectLoadObject(LoadObject obj)
    {
        if (loadObject != null)
        {
            loadObject.mainBody.color = unselectedColor;
        }
        loadObject = obj;
        loadObject.mainBody.color = selectedColor;
    }

    /// <summary>
    /// Deletes the specified <c>LoadObject</c> from the scene and removes it from the list of load objects
    /// </summary>
    /// <param name="obj">The references <c>LoadObject</c></param>
    public void DeleteLoadObject(LoadObject obj)
    {
        string path = JsonManager.GetJsonFilePath(obj.diagram.name);
        File.Delete(path);
        Destroy(obj.gameObject);
        loadObjects.Remove(obj);
    }

    /// <summary>
    /// Resets the colour of the currently selected load object
    /// </summary>
    public void DeselectLoadObject()
    {
        foreach (LoadObject obj in loadObjects)
        {
            if (obj.mainBody.color == selectedColor)
            {
                obj.mainBody.color = unselectedColor;
                break;
            }
        }
        loadObject = null;
    }

    /// <summary>
    /// Loads all diagram objects from the JSON files in the working directory
    /// Displays these diagrams onto image objects in the scene
    /// </summary>
    public void LoadDiagramObjects()
    {
        string[] paths = JsonManager.GetFilePaths();

        foreach (string path in paths)
        {
            JsonDiagram jsonDiagram = GetJsonDiagram(path);
            if (CheckIfExists(jsonDiagram)) continue; // Skip if diagram already exists

            LoadFromDiagram(jsonDiagram);
            diagram.SetBoundsOnTextureCamera();

            GameObject newObj = Instantiate(loadObjectPrefab, content);
            newObj.transform.SetAsFirstSibling(); // Allows most recently loaded diagram to be on top
            LoadObject loadObject = newObj.GetComponent<LoadObject>();

            RenderTextureToMaterial(loadObject);
            loadObject.title.text = diagram.diagramName;
            loadObject.diagram = jsonDiagram;
            loadObject.loadManager = this;
            loadObjects.Add(loadObject);
        }
    }

    public void RenderTextureToMaterial(LoadObject loadObj)
    {
        RenderTexture texture = new(1024, 1024, 32);
        diagram.textureCamera.targetTexture = texture;
        diagram.textureCamera.Render();

        Material material = new(Shader.Find("UI/Default"))
        {
            mainTexture = texture
        };
        loadObj.image.material = material;
    }

    public bool CheckIfExists(JsonDiagram jsonDiagram)
    {
        foreach (LoadObject loadObj in loadObjects)
        {
            if (loadObj.diagram.name.Equals(jsonDiagram.name)) return true;
        }
        return false;
    }

    public JsonDiagram GetJsonDiagram(string path)
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<JsonDiagram>(json);
    }

    /// <summary>
    /// Loads the currently selected <c>LoadObject</c> into the main diagram
    /// </summary>
    /// <remarks>
    /// Destroys the <c>LoadObject</c> after loading to indicate that it must be reloaded
    /// </remarks>
    public void OpenLoadDiagram()
    {
        if (loadObject == null) return;
        LoadFromDiagram(loadObject.diagram);
        diagram.SetActive(true);
        gameObject.SetActive(false);
        loadObjects.Remove(loadObject);
        Destroy(loadObject.gameObject);
    }

    /// <summary>
    /// Loads a <c>JsonDiagram</c> into the main diagram
    /// </summary>
    /// <param name="jsonDiagram">The <c>JsonDiagram</c> to load</param>
    public void LoadFromDiagram(JsonDiagram jsonDiagram) // TODO (MAYBE) CREATE LOAD ELEMENT FUNCTION TO OVERIDE IN POINT, LINE, CIRCLE, ANGLE THEN CALL THAT HERE AND COMBINE ALL INTO ONE FOR LOOP (would be more modular and easier to read)
    {
        diagram.diagramName = jsonDiagram.name;

        diagram.ResetDiagram();

        Dictionary<int, Point> idToPointMap = new();
        LoadPoints(idToPointMap, jsonDiagram);
        LoadLines(idToPointMap, jsonDiagram);
        LoadCircles(idToPointMap, jsonDiagram);
        LoadAngles(idToPointMap, jsonDiagram);

        // Initialise labels for all elements
        foreach (Element element in diagram.elements)
        {
            if (element.isLabelVisible)
            {
                diagram.label.CreateLabelObject(element);
                element.SetLabel();
            }
        }
    }

    /// <summary>
    /// Loads all <c>Point</c> objects from a <c>JsonDiagram</c> into the main diagram
    /// </summary>
    /// <param name="idToPointMap">The mapping dictionary to fill</param>
    /// <param name="jsonDiagram">The <c>JsonDiagram</c> to load <c>Point</c> objects from</param>
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

    /// <summary>
    /// Loads all <c>Line</c> objects from a <c>JsonDiagram</c> into the main diagram
    /// </summary>
    /// <param name="idToPointMap">The mapping dictionary to get <c>Point</c> objects from</param>
    /// <param name="jsonDiagram">The <c>JsonDiagram</c> to load <c>Line</c> objects from</param>
    public void LoadLines(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonLine jsonLine in jsonDiagram.lines)
        {
            Line line = diagram.CreateLine();
            for (int i = 0; i < jsonLine.pointIDs.Length; i++)
            {
                Point point = idToPointMap[jsonLine.pointIDs[i]];
                line.points[i] = point;
                point.attachedElements.Add(line);
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

    /// <summary>
    /// Loads all <c>Circle</c> objects from a <c>JsonDiagram</c> into the main diagram
    /// </summary>
    /// <param name="idToPointMap">The mapping dictionary to get <c>Point</c> objects from</param>
    /// <param name="jsonDiagram">The <c>JsonDiagram</c> to load <c>Circle</c> objects from</param>
    public void LoadCircles(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonCircle jsonCircle in jsonDiagram.circles)
        {
            Point point = idToPointMap[jsonCircle.centreID];
            Circle circle = diagram.CreateCircle(point.position);
            circle.centre = point;
            point.attachedElements.Add(circle);
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

    /// <summary>
    /// Loads all <c>Angle</c> objects from a <c>JsonDiagram</c> into the main diagram
    /// </summary>
    /// <param name="idToPointMap">The mapping dictionary to get <c>Point</c> objects from</param>
    /// <param name="jsonDiagram">The <c>JsonDiagram</c> to load <c>Angle</c> objects from</param>
    public void LoadAngles(Dictionary<int, Point> idToPointMap, JsonDiagram jsonDiagram)
    {
        foreach (JsonAngle jsonAngle in jsonDiagram.angles)
        {
            Angle angle = diagram.CreateAngle();
            for (int i = 0; i < jsonAngle.pointIDs.Length; i++)
            {
                Point point = idToPointMap[jsonAngle.pointIDs[i]];
                angle.points[i] = point;
                point.attachedElements.Add(angle);
            }
            angle.isLabelVisible = jsonAngle.isLabelVisible;
            angle.labelOverride = jsonAngle.labelOverride;
            diagram.elements.Add(angle);
            angle.transform.position = angle.points[0].position;
            angle.GetAngleData();
            angle.DrawAngle();
            angle.DrawHitbox();
        }
    }
}

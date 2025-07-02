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
    private List<LoadObject> loadObjects = new();

    /// <summary>
    /// Initialises the load menu
    /// </summary>
    public void OpenLoadMenu()
    {
        float time = System.DateTime.Now.Millisecond;
        DeselectLoadObject();
        LoadDiagramObjects();
        Debug.Log("Load menu opened in " + (System.DateTime.Now.Millisecond - time) + "ms");
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
    public void LoadFromDiagram(JsonDiagram jsonDiagram)
    {
        diagram.diagramName = jsonDiagram.name;

        diagram.ResetDiagram();

        Dictionary<int, Point> idToPointMap = new();
        List<JsonElement> elements = new();
        elements.AddRange(jsonDiagram.points);
        elements.AddRange(jsonDiagram.lines);
        elements.AddRange(jsonDiagram.circles);
        elements.AddRange(jsonDiagram.angles);

        foreach (JsonElement element in elements)
        {
            element.LoadDiagramElement(diagram, idToPointMap);
        }
        foreach (Element element in diagram.elements)
        {
            if (element.isLabelVisible)
            {
                diagram.label.CreateLabelObject(element);
                element.SetLabel();
            }
        }
    }
}

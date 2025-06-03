using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadManager : MonoBehaviour
{
    public Diagram diagram;
    public DiagramSaver diagramSaver;

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
        diagramSaver.LoadFromDiagram(loadObject.diagram);
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
            LoadAllTextures();
        }
    }

    public void LoadAllTextures() // TODO RESET HIGHLIGHTED LOAD OBJECTS TO UNSELECTED COLOR ON LOAD MENU
    {
        string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string path in paths)
        {
            JsonDiagram jsonDiagram = diagramSaver.GetJsonDiagram(path);
            if (CheckIfExists(jsonDiagram)) continue; // Skip if diagram already exists

            diagramSaver.LoadJsonDiagram(path);
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
}

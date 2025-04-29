using UnityEngine;
using UnityEngine.EventSystems;

public class PointCreator : MonoBehaviour, IPointerClickHandler
{
    public GameObject pointPrefab;
    public GameObject linePrefab;
    //private GameObject currentLine;
    //private LineRenderer lineRenderer;
    public Point point;
    public bool placing;

    void Start()
    {
        //currentLine = Instantiate(linePrefab, transform);
        //lineRenderer = currentLine.GetComponent<LineRenderer>();
        //lineRenderer.positionCount = 0;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (placing) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        mousePosition.z = 0; // Set z to 0 for 2D
        CreatePoint(mousePosition);
        placing = true;
    }

    private void CreatePoint(Vector3 position)
    {
        GameObject pointObj = Instantiate(pointPrefab, position, Quaternion.identity);
        point = pointObj.GetComponent<Point>();
    }

    public void Update()
    {
        if (placing)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D
            point.gameObject.transform.position = mousePosition;
        }
    }
}

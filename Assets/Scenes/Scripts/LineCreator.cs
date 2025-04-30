using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineCreator : MonoBehaviour, IPointerClickHandler
{
    public GameObject diagram;
    public Line line;
    public GameObject linePrefab;
    public PlacingStage placing;
    public Point hoveringPoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Allows the line to lock to the nearest point or line (gives points priority)
    public void GetProminentFeature(ref Vector3 position)
    {
        hoveringPoint = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Point point))
            {
                position = point.position;
                hoveringPoint = point;
                return;
            }
        }
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Line line))
            {
                position = line.CalculateClosestPoint(position);
                return;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (placing != PlacingStage.None) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        mousePosition.z = 0; // Set z to 0 for 2D
        CreateLine(mousePosition);
        placing = PlacingStage.Point;
    }

    private void CreateLine(Vector3 position)
    {
        GameObject lineObj = Instantiate(linePrefab, position, Quaternion.identity, diagram.transform);
        line = lineObj.GetComponent<Line>();
    }

    public void Update()
    {
        if (placing == PlacingStage.Point)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D

            GetProminentFeature(ref mousePosition);
            line.gameObject.transform.position = mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                line.line.SetPosition(0, mousePosition);
                placing = PlacingStage.Line;
                if (!hoveringPoint)
                {
                    //create a point
                }
            }
        }
        else if (placing == PlacingStage.Line)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D

            GetProminentFeature(ref mousePosition);
            line.line.SetPosition(1, mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                placing = PlacingStage.None;
                if (!hoveringPoint)
                {
                    //create a point
                }

                line.AddComponent<ClickableLine>();
                line = null;
            }
        }
    }

    public enum PlacingStage
    {
        None,
        Point,
        Line
    }
}

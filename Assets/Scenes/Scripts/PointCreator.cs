using UnityEngine;
using UnityEngine.EventSystems;

public class PointCreator : MonoBehaviour, IPointerClickHandler
{
    public GameObject diagram;
    public GameObject pointPrefab;
    public Point point;
    public bool placing;

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
        GameObject pointObj = Instantiate(pointPrefab, position, Quaternion.identity, diagram.transform);
        point = pointObj.GetComponent<Point>();
    }

    public void Update()
    {
        if (placing)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D
            point.gameObject.transform.position = mousePosition;
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
            if (hit.collider != null && hit.collider.TryGetComponent(out Line line))
            {
                point.gameObject.transform.position = line.CalculateClosestPoint(mousePosition);
            }

            if (Input.GetMouseButtonDown(0))
            {
                placing = false;
                point = null;
            }
        }
    }
}

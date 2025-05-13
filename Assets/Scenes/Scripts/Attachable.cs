using UnityEngine;

public abstract class Attachable : MonoBehaviour
{
    public abstract Vector2 GetClosestPosition(Vector2 point);
    public abstract void AttachPoint(Point point);
    public abstract float CalculatePercentage(Vector2 point);
}

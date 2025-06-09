using System.Collections.Generic;
using UnityEngine;

public abstract class Attachable : Element
{
    public List<Point> attachedPoints = new();

    public abstract Vector2 GetClosestPosition(Vector2 point);
    public abstract float CalculatePercentage(Vector2 point);
    public void AttachPoint(Point point)
    {
        point.percentage = CalculatePercentage(point.position);
        point.semiAttachedLine = this;
        attachedPoints.Add(point);
    }
}

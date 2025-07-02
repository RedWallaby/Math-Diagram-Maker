using System.Collections.Generic;
using UnityEngine;
using static JsonDiagram;

public class JsonDiagram
{
    public string name;

    public List<JsonPoint> points = new();
    public List<JsonLine> lines = new();
    public List<JsonCircle> circles = new();
    public List<JsonAngle> angles = new();

    [System.Serializable]
    public abstract class JsonElement
    {
        public bool isLabelVisible;
        public string labelOverride;

        public abstract void LoadDiagramElement(Diagram diagram, Dictionary<int, Point> idToPointMap);
    }

    [System.Serializable]
    public class JsonPoint : JsonElement
    {
        public int id;
        public Vector2 position;
        public float percentage;
        public List<int> elementIDs = new();
        public int semiAttachedLineID;

        public JsonPoint(Point point, Dictionary<Element, int> elementToID)
        {
            id = elementToID[point];
            position = point.position;
            percentage = point.percentage;
            foreach (Element element in point.attachedElements)
            {
                elementIDs.Add(elementToID[element]);
            }
            if (point.semiAttachedLine != null)
            {
                semiAttachedLineID = elementToID[point.semiAttachedLine];
            }
            else
            {
                semiAttachedLineID = -1;
            }
            isLabelVisible = point.isLabelVisible;
            labelOverride = point.labelOverride;
        }

        public override void LoadDiagramElement(Diagram diagram, Dictionary<int, Point> idToPointMap)
        {
            Point point = diagram.CreatePoint(position);
            point.percentage = percentage;
            idToPointMap[id] = point;
            point.isLabelVisible = isLabelVisible;
            point.labelOverride = labelOverride;
            diagram.elements.Add(point);
        }
    }

    [System.Serializable]
    public class JsonLine : JsonElement
    {
        public int[] pointIDs = new int[2];
        public List<int> attachedPointIDs = new();

        public JsonLine(Line line, Dictionary<Element, int> elementToID)
        {
            for (int i = 0; i < line.points.Length; i++)
            {
                pointIDs[i] = elementToID[line.points[i]];
            }
            foreach (Point point in line.attachedPoints)
            {
                attachedPointIDs.Add(elementToID[point]);
            }
            isLabelVisible = line.isLabelVisible;
            labelOverride = line.labelOverride;
        }

        public override void LoadDiagramElement(Diagram diagram, Dictionary<int, Point> idToPointMap)
        {
            Line line = diagram.CreateLine();
            for (int i = 0; i < pointIDs.Length; i++)
            {
                Point point = idToPointMap[pointIDs[i]];
                line.points[i] = point;
                point.attachedElements.Add(line);
            }
            line.ForceUpdateLineRenderer();
            line.SetPosition();
            line.DrawLineHitbox();
            foreach (int pointID in attachedPointIDs)
            {
                line.AttachPoint(idToPointMap[pointID]);
            }
            line.isLabelVisible = isLabelVisible;
            line.labelOverride = labelOverride;
            diagram.elements.Add(line);
        }
    }

    [System.Serializable]
    public class JsonCircle : JsonElement
    {
        public int centreID;
        public List<int> attachedPointIDs = new();
        public float radius;

        public JsonCircle(Circle circle, Dictionary<Element, int> elementToID)
        {
            centreID = elementToID[circle.centre];
            foreach (Point point in circle.attachedPoints)
            {
                attachedPointIDs.Add(elementToID[point]);
            }
            radius = circle.radius;
            isLabelVisible = circle.isLabelVisible;
            labelOverride = circle.labelOverride;
        }

        public override void LoadDiagramElement(Diagram diagram, Dictionary<int, Point> idToPointMap)
        {
            Point centre = idToPointMap[centreID];
            Circle circle = diagram.CreateCircle(centre.position);
            circle.centre = centre;
            centre.attachedElements.Add(circle);
            foreach (int pointID in attachedPointIDs)
            {
                circle.AttachPoint(idToPointMap[pointID]);
            }
            circle.radius = radius;
            circle.DrawCircle();
            circle.isLabelVisible = isLabelVisible;
            circle.labelOverride = labelOverride;
            diagram.elements.Add(circle);
        }
    }

    [System.Serializable]
    public class JsonAngle : JsonElement
    {
        public int[] pointIDs = new int[3];

        public JsonAngle(Angle angle, Dictionary<Element, int> elementToID)
        {
            for (int i = 0; i < angle.points.Length; i++)
            {
                pointIDs[i] = elementToID[angle.points[i]];
            }
            isLabelVisible = angle.isLabelVisible;
            labelOverride = angle.labelOverride;
        }

        public override void LoadDiagramElement(Diagram diagram, Dictionary<int, Point> idToPointMap)
        {
            Angle angle = diagram.CreateAngle();
            for (int i = 0; i < pointIDs.Length; i++)
            {
                Point point = idToPointMap[pointIDs[i]];
                angle.points[i] = point;
                point.attachedElements.Add(angle);
            }
            angle.transform.position = angle.points[0].position;
            angle.GetAngleData();
            angle.DrawAngle();
            angle.DrawHitbox();
            angle.isLabelVisible = isLabelVisible;
            angle.labelOverride = labelOverride;
            diagram.elements.Add(angle);
        }
    }
}

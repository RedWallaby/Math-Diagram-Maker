using System.Collections.Generic;
using UnityEngine;

public class JsonDiagram
{
    public string name;

    public List<JsonPoint> points = new();
    public List<JsonLine> lines = new();
    public List<JsonCircle> circles = new();
    public List<JsonAngle> angles = new();


    [System.Serializable]
    public class JsonElement
    {
        public bool isLabelVisible;
        public string labelOverride;
    }

    [System.Serializable]
    public class JsonPoint : JsonElement
    {
        public int id;
        public Vector2 position;
        public float percentage;
        public List<int> attachedLineIDs = new();
        public List<int> circleIDs = new();
        public int semiAttachedLineID;

        public JsonPoint(Point point, Dictionary<Element, int> elementToID)
        {
            id = elementToID[point];
            position = point.position;
            percentage = point.percentage;
            foreach (Line line in point.attatchedLines)
            {
                attachedLineIDs.Add(elementToID[line]);
            }
            foreach (Circle circle in point.circles)
            {
                circleIDs.Add(elementToID[circle]);
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
    }

    [System.Serializable]
    public class JsonAngle : JsonElement
    {
        public int startID;
        public int centreID;
        public int endID;

        public JsonAngle(Angle angle, Dictionary<Element, int> elementToID)
        {
            startID = elementToID[angle.start];
            centreID = elementToID[angle.centre];
            endID = elementToID[angle.end];
            isLabelVisible = angle.isLabelVisible;
            labelOverride = angle.labelOverride;
        }
    }

}

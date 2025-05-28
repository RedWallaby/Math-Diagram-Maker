using System.Collections.Generic;
using UnityEngine;

public class JsonDiagram
{
    public List<JsonPoint> points = new();
    public List<JsonLine> lines = new();
    public List<JsonCircle> circles = new();
    public List<JsonAngle> angles = new();


    [System.Serializable]
    public class JsonPoint
    {
        public int id;
        public Vector2 position;
        public float percentage;
        public List<int> attachedLineIDs = new();
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
            if (point.semiAttachedLine != null)
            {
                semiAttachedLineID = elementToID[point.semiAttachedLine];
            }
            else
            {
                semiAttachedLineID = -1;
            }
        }
    }

    [System.Serializable]
    public class JsonLine
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
        }
    }

    [System.Serializable]
    public class JsonCircle
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
        }
    }

    [System.Serializable]
    public class JsonAngle
    {
        public int startID;
        public int centreID;
        public int endID;

        public JsonAngle(Angle angle, Dictionary<Element, int> elementToID)
        {
            startID = elementToID[angle.start];
            centreID = elementToID[angle.centre];
            endID = elementToID[angle.end];
        }
    }

}

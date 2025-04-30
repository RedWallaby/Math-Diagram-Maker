using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public Vector2 position => gameObject.transform.position;
    public List<Line> attatchedLines;

}

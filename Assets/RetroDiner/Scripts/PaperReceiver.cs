using UnityEngine;

public class PaperReceiver : MonoBehaviour
{
    public Transform drawingRoot;

    private void Awake()
    {
        if (drawingRoot == null)
        {
            GameObject root = new GameObject("DrawingRoot");
            root.transform.SetParent(transform, false);
            drawingRoot = root.transform;
        }
    }

    public LineRenderer StartStroke(Color color, float width)
    {
        GameObject strokeObj = new GameObject("Stroke");
        strokeObj.transform.SetParent(drawingRoot, false);

        LineRenderer lr = strokeObj.AddComponent<LineRenderer>();

        lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.startColor = color;
        lr.endColor = color;

        lr.startWidth = width;
        lr.endWidth = width;

        lr.useWorldSpace = false;
        lr.positionCount = 0;

        return lr;
    }

    public Vector3 ToLocalPoint(Vector3 worldPoint)
    {
        return drawingRoot.InverseTransformPoint(worldPoint);
    }
}
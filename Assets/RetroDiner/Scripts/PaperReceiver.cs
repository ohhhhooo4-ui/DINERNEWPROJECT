using UnityEngine;

public class PaperReceiver : MonoBehaviour
{
    [HideInInspector]
    public Transform drawingRoot;

    private void Awake()
    {
        if (drawingRoot == null)
        {
            Transform existing = transform.Find("DrawingRoot");

            if (existing != null)
            {
                drawingRoot = existing;
            }
            else
            {
                GameObject root = new GameObject("DrawingRoot");
                root.transform.SetParent(transform);
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = Quaternion.identity;
                root.transform.localScale = Vector3.one;

                drawingRoot = root.transform;
            }
        }
    }

    public LineRenderer CreateStroke(Color color, float width)
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

    public Vector3 WorldToPaperPoint(Vector3 worldPoint)
    {
        return drawingRoot.InverseTransformPoint(worldPoint);
    }
}
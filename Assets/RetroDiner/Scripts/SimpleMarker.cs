using UnityEngine;

public class SimpleMarker : MonoBehaviour
{
    public Transform tip;

    public float drawDistance = 0.02f;
    public float lineWidth = 0.002f;

    public LayerMask paperLayer;
    public Color inkColor = Color.blue;

    private PaperReceiver currentPaper;
    private LineRenderer currentLine;
    private bool isDrawing;

    void Update()
    {
        if (Physics.Raycast(
       tip.position,
       tip.up,
       out RaycastHit hit,
       drawDistance,
       paperLayer))

        {
            PaperReceiver paper = hit.collider.GetComponent<PaperReceiver>();

            if (paper != null)
            {
                Vector3 localPoint =
                    paper.WorldToPaperPoint(hit.point + hit.normal * 0.001f);

                if (!isDrawing || currentPaper != paper)
                {
                    currentPaper = paper;
                    currentLine = paper.CreateStroke(inkColor, lineWidth);
                    isDrawing = true;

                    currentLine.positionCount = 1;
                    currentLine.SetPosition(0, localPoint);
                }
                else
                {
                    int last = currentLine.positionCount - 1;

                    if (Vector3.Distance(
                        currentLine.GetPosition(last),
                        localPoint) > 0.001f)
                    {
                        currentLine.positionCount++;

                        currentLine.SetPosition(
                            currentLine.positionCount - 1,
                            localPoint);
                    }
                }
            }
        }
        else
        {
            isDrawing = false;
            currentPaper = null;
            currentLine = null;
        }
    }
}
using UnityEngine;

public class PenWriter : MonoBehaviour
{
    public Transform tip;
    public LayerMask paperLayer;

    public float drawDistance = 0.05f;
    public float lineWidth = 0.003f;

    public Color inkColor = Color.blue;

    private PaperReceiver currentPaper;
    private LineRenderer currentLine;

    void Update()
    {
        if (tip == null)
        {
            return;
        }

        Debug.DrawRay(
            tip.position,
            tip.up * drawDistance,
            Color.red);

        if (Physics.Raycast(
            tip.position,
            tip.up,
            out RaycastHit hit,
            drawDistance,
            paperLayer))
        {
            PaperReceiver paper =
                hit.collider.GetComponentInParent<PaperReceiver>();

            if (paper == null)
            {
                return;
            }

            Vector3 localPoint =
                paper.ToLocalPoint(
                    hit.point + hit.normal * 0.001f);

            if (currentPaper != paper || currentLine == null)
            {
                currentPaper = paper;
                currentLine =
                    paper.StartStroke(
                        inkColor,
                        lineWidth);

                currentLine.positionCount = 1;
                currentLine.SetPosition(0, localPoint);
            }
            else
            {
                int lastIndex = currentLine.positionCount - 1;

                if (Vector3.Distance(
                    currentLine.GetPosition(lastIndex),
                    localPoint) > 0.0005f)
                {
                    currentLine.positionCount++;

                    currentLine.SetPosition(
                        currentLine.positionCount - 1,
                        localPoint);
                }
            }
        }
        else
        {
            currentPaper = null;
            currentLine = null;
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SearchWaveView : MonoBehaviour
{
    [SerializeField] private float heightOffset = 0.05f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = Color.cyan;

    private const int SegmentCount = 64;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.loop = true;
        line.positionCount = SegmentCount;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.enabled = false;
    }

    public void Show(Vector3 center, float radius)
    {
        line.enabled = true;
        center.y += heightOffset;

        for (int i = 0; i < SegmentCount; i++)
        {
            float angle = i * Mathf.PI * 2f / SegmentCount;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle),
                0f,
                Mathf.Sin(angle)) * radius;

            line.SetPosition(i, center + offset);
        }
    }

    public void Hide()
    {
        line.enabled = false;
    }
}
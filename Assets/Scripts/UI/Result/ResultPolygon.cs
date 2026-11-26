using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/Result Polygon")]
public sealed class ResultPolygon : MaskableGraphic
{
    [Tooltip("Convex outline in normalized coordinates, with the origin at the top-left of the rect.")]
    [SerializeField] private Vector2[] points;

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (points == null || points.Length < 3)
            return;

        Rect rect = GetPixelAdjustedRect();
        foreach (Vector2 point in points)
        {
            mesh.AddVert(new Vector2(rect.xMin + point.x * rect.width,
                rect.yMax - point.y * rect.height), color, Vector2.zero);
        }

        for (int index = 1; index < points.Length - 1; index++)
            mesh.AddTriangle(0, index, index + 1);
    }
}

using UnityEngine;
using UnityEngine.UI;

// An independent outline keeps the silhouette clear without making the fill opaque.
[RequireComponent(typeof(CanvasRenderer))]
public class SearchSkillUIRing : MaskableGraphic
{
    public float Thickness { get; set; } = 2.5f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        Vector2 center = rect.center;
        float outer = Mathf.Min(rect.width, rect.height) * 0.5f;
        float inner = Mathf.Max(0f, outer - Thickness);
        const float feather = 0.65f;
        const int segments = 128;
        float[] radii = { outer, Mathf.Max(inner, outer - feather), inner + feather, inner };
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            for (int band = 0; band < 4; band++)
            {
                Color vertexColor = color;
                if (band == 0 || band == 3) vertexColor.a = 0f;
                vh.AddVert(center + direction * radii[band], vertexColor, Vector2.zero);
            }
            if (i == 0) continue;
            int start = (i - 1) * 4;
            for (int band = 0; band < 3; band++)
            {
                vh.AddTriangle(start + band, start + band + 4, start + band + 1);
                vh.AddTriangle(start + band + 1, start + band + 4, start + band + 5);
            }
        }
    }
}

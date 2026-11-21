using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TitleModeButtonGraphic : MaskableGraphic
{
    [SerializeField] private Color fillColor = new Color(0.08f, 0.18f, 0.32f, 0.48f);
    [SerializeField] private Color borderColor = new Color(1.0f, 1.0f, 1.0f, 0.8f);
    [SerializeField, Min(0.0f)] private float slantWidth = 24.0f;
    [SerializeField, Min(0.0f)] private float borderWidth = 2.0f;
    [SerializeField] private Color selectedFillColor = new Color(1.0f, 0.43f, 0.12f, 0.5f);
    [SerializeField] private Color selectedBorderColor = new Color(1.0f, 0.64f, 0.22f, 1.0f);
    [SerializeField, Min(0.0f)] private float selectedBorderWidth = 5.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float selectionProgress;
    [SerializeField, Min(0.0f)] private float glowWidth = 14.0f;
    [SerializeField, Range(0.0f, 1.0f)] private float glowOpacity = 0.18f;
    [SerializeField, Range(0.0f, 1.0f)] private float glowIntensity = 0.7f;
    private const int GlowRingCount = 5;

    public void SetSelectionVisual(float progress, float intensity)
    {
        float clampedProgress = Mathf.Clamp01(progress);
        float clampedIntensity = Mathf.Clamp01(intensity);
        if (Mathf.Approximately(selectionProgress, clampedProgress) &&
            Mathf.Approximately(glowIntensity, clampedIntensity))
            return;
        selectionProgress = clampedProgress;
        glowIntensity = clampedIntensity;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = GetPixelAdjustedRect();
        float glowStrength = selectionProgress * glowIntensity;
        if (glowStrength > 0.001f)
        {
            for (int ring = GlowRingCount; ring > 0; ring--)
            {
                float outerExpansion = glowWidth * ring / GlowRingCount;
                float innerExpansion = glowWidth * (ring - 1) / GlowRingCount;
                Color glowColor = selectedBorderColor;
                glowColor.a *= glowOpacity * glowStrength * (1.0f - (ring - 1.0f) / GlowRingCount);
                AddBorder(vertexHelper, ExpandRect(rect, outerExpansion), ExpandRect(rect, innerExpansion),
                    glowColor * color);
            }
        }
        float currentBorderWidth = Mathf.Lerp(borderWidth, selectedBorderWidth, selectionProgress);
        float inset = Mathf.Min(currentBorderWidth, Mathf.Min(rect.width, rect.height) * 0.5f);
        Rect innerRect = new Rect(rect.xMin + inset, rect.yMin + inset,
            Mathf.Max(0.0f, rect.width - inset * 2.0f), Mathf.Max(0.0f, rect.height - inset * 2.0f));

        // 外周と内側を重ねず、半透明の帯の中央に枠色が透けないようにする。
        Color currentFillColor = Color.Lerp(fillColor, selectedFillColor, selectionProgress);
        Color currentBorderColor = Color.Lerp(borderColor, selectedBorderColor, selectionProgress);
        Color fillHighlight = new Color(1, 1, 1, currentFillColor.a);
        Color borderHighlight = new Color(1, 1, 1, currentBorderColor.a);
        AddParallelogram(vertexHelper, innerRect, Color.Lerp(currentFillColor, fillHighlight, glowStrength * 0.07f) * color);
        AddBorder(vertexHelper, rect, innerRect, Color.Lerp(currentBorderColor, borderHighlight, glowStrength * 0.12f) * color);
    }

    private static Rect ExpandRect(Rect rect, float expansion)
    {
        return new Rect(rect.xMin - expansion, rect.yMin - expansion,
            rect.width + expansion * 2.0f, rect.height + expansion * 2.0f);
    }

    private void AddParallelogram(VertexHelper vertexHelper, Rect rect, Color tint)
    {
        int start = vertexHelper.currentVertCount;
        for (int index = 0; index < 4; index++)
            vertexHelper.AddVert(GetCorner(rect, index), tint, Vector2.zero);
        vertexHelper.AddTriangle(start, start + 1, start + 2);
        vertexHelper.AddTriangle(start, start + 2, start + 3);
    }

    private void AddBorder(VertexHelper vertexHelper, Rect outerRect, Rect innerRect, Color tint)
    {
        int start = vertexHelper.currentVertCount;
        for (int index = 0; index < 4; index++)
        {
            vertexHelper.AddVert(GetCorner(outerRect, index), tint, Vector2.zero);
            vertexHelper.AddVert(GetCorner(innerRect, index), tint, Vector2.zero);
        }
        for (int index = 0; index < 4; index++)
        {
            int outer = start + index * 2;
            int nextOuter = start + (index + 1) % 4 * 2;
            vertexHelper.AddTriangle(outer, nextOuter, outer + 1);
            vertexHelper.AddTriangle(nextOuter, nextOuter + 1, outer + 1);
        }
    }

    private Vector3 GetCorner(Rect rect, int index)
    {
        float slant = Mathf.Min(slantWidth, rect.width * 0.25f);
        switch (index)
        {
            case 0: return new Vector3(rect.xMin, rect.yMin);
            case 1: return new Vector3(rect.xMin + slant, rect.yMax);
            case 2: return new Vector3(rect.xMax, rect.yMax);
            default: return new Vector3(rect.xMax - slant, rect.yMin);
        }
    }
}

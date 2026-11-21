using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasRenderer))]
public sealed class TitleLogoSparkle : MaskableGraphic
{
    [SerializeField, Min(0.0f)] private float delay = 0.35f;
    [SerializeField, Min(0.01f)] private float duration = 0.9f;
    [SerializeField] private Color glowColor = new Color(0.65f, 1.0f, 0.3f, 0.45f);
    [SerializeField] private float rotationDegrees = 25.0f;

    private static readonly Vector2[] StarPoints =
    {
        new Vector2(0.0f, 0.5f), new Vector2(0.07f, 0.07f),
        new Vector2(0.5f, 0.0f), new Vector2(0.07f, -0.07f),
        new Vector2(0.0f, -0.5f), new Vector2(-0.07f, -0.07f),
        new Vector2(-0.5f, 0.0f), new Vector2(-0.07f, 0.07f)
    };

    public void Play()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        raycastTarget = false;
        StartCoroutine(AnimateSparkle());
    }

    private IEnumerator AnimateSparkle()
    {
        color = new Color(1, 1, 1, 0);
        rectTransform.localScale = Vector3.zero;
        float elapsed = 0.0f;
        while (elapsed < delay)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        elapsed = 0.0f;
        while (elapsed < duration)
        {
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            float brightness = progress < 0.14f ? progress / 0.14f :
                progress < 0.42f ? 1.0f : Mathf.Pow((1.0f - progress) / 0.58f, 1.2f);
            float scale = progress < 0.14f ? Mathf.Lerp(0.2f, 1.25f, progress / 0.14f) :
                progress < 0.42f ? 1.25f : Mathf.Lerp(1.25f, 0.6f, (progress - 0.42f) / 0.58f);
            color = new Color(1, 1, 1, brightness);
            rectTransform.localScale = Vector3.one * scale;
            rectTransform.localRotation = Quaternion.Euler(0, 0, rotationDegrees * progress);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
        gameObject.SetActive(false);
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Rect rect = GetPixelAdjustedRect();
        AddStar(vertexHelper, rect, 1.65f, glowColor * color);
        AddStar(vertexHelper, rect, 1.25f, new Color(0.85f, 1.0f, 0.65f, 0.35f) * color);
        AddStar(vertexHelper, rect, 1.0f, color);
    }

    private static void AddStar(VertexHelper vertexHelper, Rect rect, float scale, Color tint)
    {
        int start = vertexHelper.currentVertCount;
        vertexHelper.AddVert(rect.center, tint, Vector2.zero);
        foreach (Vector2 point in StarPoints)
        {
            Vector2 position = rect.center + Vector2.Scale(point, rect.size) * scale;
            vertexHelper.AddVert(position, tint, Vector2.zero);
        }
        for (int index = 0; index < StarPoints.Length; index++)
            vertexHelper.AddTriangle(start, start + index + 1, start + (index + 1) % StarPoints.Length + 1);
    }
}

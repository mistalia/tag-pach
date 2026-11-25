using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TitleEntranceAnimation : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Image revealImage;
    [SerializeField] private RectTransform logo;
    [SerializeField] private RectTransform[] artworkLayers = new RectTransform[0];
    [SerializeField] private CanvasGroup[] optionGroups = new CanvasGroup[0];
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private TitleLogoSparkle logoSparkle;

    [Header("Center Reveal")]
    [SerializeField] private Color coverColor = Color.white;
    [SerializeField, Min(0.0f)] private float holdDuration = 0.3f;
    [SerializeField, Min(0.01f)] private float revealDuration = 2.5f;
    [SerializeField, Range(0.001f, 0.5f)] private float featherWidth = 0.12f;

    [Header("Pop Out")]
    [SerializeField, Range(0.01f, 1.0f)] private float initialLogoScale = 0.18f;
    [SerializeField, Range(0.1f, 1.0f)] private float initialArtworkScale = 0.86f;
    [SerializeField, Min(0.0f)] private float logoDelay = 0.18f;
    [SerializeField, Min(0.01f)] private float logoDuration = 1.9f;
    [SerializeField, Range(0.0f, 3.0f)] private float logoOvershoot = 1.35f;
    [SerializeField, Min(0.01f)] private float optionsDuration = 0.7f;

    private static readonly int RevealProgressId = Shader.PropertyToID("_RevealProgress");
    private static readonly int AspectRatioId = Shader.PropertyToID("_AspectRatio");
    private static readonly int FeatherWidthId = Shader.PropertyToID("_FeatherWidth");

    private Material revealMaterial;
    private Vector2 logoPosition;
    private Vector3 logoScale;
    private Vector3[] artworkScales;
    private bool isInitialized;

    private void Awake()
    {
        if (revealImage == null || revealImage.material == null || logo == null || menuCanvasGroup == null)
        {
            Debug.LogError("[TitleEntranceAnimation] Entrance references are not configured.", this);
            if (revealImage != null)
                revealImage.gameObject.SetActive(false);
            enabled = false;
            return;
        }

        logoPosition = logo.anchoredPosition;
        logoScale = logo.localScale;
        artworkScales = new Vector3[artworkLayers.Length];
        for (int index = 0; index < artworkLayers.Length; index++)
        {
            if (artworkLayers[index] != null)
                artworkScales[index] = artworkLayers[index].localScale;
        }

        // マテリアルを共有する別シーンの表示を変えないよう、再生時だけ複製する。
        revealMaterial = new Material(revealImage.material);
        revealImage.material = revealMaterial;
        revealImage.color = coverColor;
        revealImage.gameObject.SetActive(true);
        revealImage.transform.SetAsLastSibling();
        menuCanvasGroup.alpha = 1.0f;
        menuCanvasGroup.interactable = false;
        isInitialized = true;
        ApplyFrame(0.0f);
    }

    public IEnumerator Play()
    {
        if (!isInitialized)
            yield break;

        float logoEnd = holdDuration + logoDelay + logoDuration;
        float optionsEnd = holdDuration + revealDuration * 0.8f + optionsDuration;
        float duration = Mathf.Max(holdDuration + revealDuration, Mathf.Max(logoEnd, optionsEnd));
        float elapsed = 0.0f;
        while (elapsed < duration)
        {
            ApplyFrame(elapsed);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
        RestoreLayout();
        if (logoSparkle != null)
            logoSparkle.Play();
    }

    private void ApplyFrame(float elapsed)
    {
        float revealProgress = Progress(elapsed, holdDuration, revealDuration);
        Rect overlayRect = revealImage.rectTransform.rect;
        revealMaterial.SetFloat(AspectRatioId, overlayRect.width / Mathf.Max(1.0f, overlayRect.height));
        revealMaterial.SetFloat(FeatherWidthId, featherWidth);
        revealMaterial.SetFloat(RevealProgressId, revealProgress * revealProgress);

        float logoProgress = Progress(elapsed, holdDuration + logoDelay, logoDuration);
        float popProgress = EaseOutBack(logoProgress);
        logo.localScale = logoScale * Mathf.LerpUnclamped(initialLogoScale, 1.0f, popProgress);
        logo.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, logoPosition, popProgress);

        float artworkProgress = Mathf.Lerp(initialArtworkScale, 1.0f, EaseOutCubic(revealProgress));
        for (int index = 0; index < artworkLayers.Length; index++)
        {
            if (artworkLayers[index] != null)
                artworkLayers[index].localScale = artworkScales[index] * artworkProgress;
        }

        float optionsProgress = Progress(elapsed, holdDuration + revealDuration * 0.8f, optionsDuration);
        foreach (CanvasGroup group in optionGroups)
        {
            if (group != null)
                group.alpha = optionsProgress;
        }
    }

    private float EaseOutBack(float progress)
    {
        float remaining = progress - 1.0f;
        return 1.0f + (logoOvershoot + 1.0f) * remaining * remaining * remaining +
            logoOvershoot * remaining * remaining;
    }

    private static float EaseOutCubic(float progress)
    {
        float remaining = 1.0f - progress;
        return 1.0f - remaining * remaining * remaining;
    }

    private static float Progress(float elapsed, float delay, float duration)
    {
        return Mathf.Clamp01((elapsed - delay) / Mathf.Max(0.01f, duration));
    }

    private void RestoreLayout()
    {
        if (logo != null)
        {
            logo.anchoredPosition = logoPosition;
            logo.localScale = logoScale;
        }
        for (int index = 0; index < artworkLayers.Length; index++)
        {
            if (artworkLayers[index] != null)
                artworkLayers[index].localScale = artworkScales[index];
        }
        foreach (CanvasGroup group in optionGroups)
        {
            if (group != null)
                group.alpha = 1.0f;
        }
        if (revealImage != null)
            revealImage.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (isInitialized)
            RestoreLayout();
    }

    private void OnDestroy()
    {
        if (revealMaterial != null)
            Destroy(revealMaterial);
    }
}

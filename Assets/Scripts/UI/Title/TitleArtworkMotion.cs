using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class TitleArtworkMotion : MonoBehaviour
{
    [SerializeField] private Vector2 movementAmplitude = new Vector2(5.0f, 12.0f);
    [SerializeField, Min(0.1f)] private float cycleDuration = 5.0f;
    [SerializeField] private float rotationAmplitude = 1.0f;
    [SerializeField] private float phaseOffset;
    [SerializeField] private Vector2 entranceOffset;
    [SerializeField, Min(0.0f)] private float entranceDuration = 0.8f;

    private RectTransform rectTransform;
    private Vector2 initialPosition;
    private Quaternion initialRotation;
    private float startedAt;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
        initialRotation = rectTransform.localRotation;
    }

    private void OnEnable()
    {
        startedAt = Time.unscaledTime;
    }

    private void Update()
    {
        float elapsed = Time.unscaledTime - startedAt;
        float phase = elapsed * Mathf.PI * 2.0f / Mathf.Max(0.1f, cycleDuration) + phaseOffset;
        float progress = entranceDuration > 0.0f ? Mathf.Clamp01(elapsed / entranceDuration) : 1.0f;
        float remaining = Mathf.Pow(1.0f - progress, 3.0f);
        Vector2 movement = new Vector2(Mathf.Sin(phase) * movementAmplitude.x,
            Mathf.Sin(phase * 0.8f) * movementAmplitude.y);

        rectTransform.anchoredPosition = initialPosition + movement + entranceOffset * remaining;
        rectTransform.localRotation = initialRotation * Quaternion.Euler(0.0f, 0.0f,
            Mathf.Sin(phase) * rotationAmplitude);
    }

    private void OnDisable()
    {
        if (rectTransform == null)
            return;

        rectTransform.anchoredPosition = initialPosition;
        rectTransform.localRotation = initialRotation;
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class TitleMenuButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler,
    IPointerEnterHandler
{
    [SerializeField, Range(1.0f, 1.2f)] private float selectedScale = 1.04f;
    [SerializeField, Min(0.1f)] private float glowCycleDuration = 2.4f;
    [SerializeField, Range(0.0f, 1.0f)] private float minimumGlowIntensity = 0.55f;

    private Vector3 initialScale;
    private Button button;
    private bool isSelected;
    private TitleModeButtonGraphic backgroundGraphic;

    private void Awake()
    {
        button = GetComponent<Button>();
        initialScale = transform.localScale;
        backgroundGraphic = GetComponentInChildren<TitleModeButtonGraphic>();
    }

    private void Update()
    {
        ApplySelectionVisual();
    }

    private void ApplySelectionVisual()
    {
        bool isHighlighted = isSelected && button.IsInteractable();
        transform.localScale = initialScale * (isHighlighted ? selectedScale : 1.0f);
        if (backgroundGraphic == null)
            return;

        float glowIntensity = 0.0f;
        if (isHighlighted)
        {
            float pulse = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2.0f / Mathf.Max(0.1f, glowCycleDuration)) + 1.0f) * 0.5f;
            glowIntensity = Mathf.Lerp(minimumGlowIntensity, 1.0f, pulse);
        }
        backgroundGraphic.SetSelectionVisual(isHighlighted ? 1.0f : 0.0f, glowIntensity);
    }

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        ApplySelectionVisual();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        ApplySelectionVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable() && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void OnDisable()
    {
        isSelected = false;
        if (button != null)
            transform.localScale = initialScale;
        if (backgroundGraphic != null)
            backgroundGraphic.SetSelectionVisual(0.0f, 0.0f);
    }
}

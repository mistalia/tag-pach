using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class SkillCooldownUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text iconText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Image cooldownFill;
    [SerializeField] private TMP_Text cooldownText;

    [Header("Ready Appearance")]
    [SerializeField] private bool useCustomReadyColor = true;
    [SerializeField] private Color customReadyColor = new Color(1f, 0.72f, 0.48f, 1f);
    [SerializeField, Range(0f, 1f)] private float teamColorSoftness = 0.3f;
    [SerializeField, Range(0f, 1f)] private float readyAlpha = 0.8f;
    [SerializeField] private Color readyIconColor = Color.white;

    [Header("Outline")]
    [SerializeField, Min(1.5f)] private float outlineThickness = 4f;
    [SerializeField] private Color readyOutlineColor = new Color(0.82f, 0.4f, 0.14f, 0.95f);
    [SerializeField, Min(0f)] private float readyBorderInset = 6f;
    [SerializeField] private Color cooldownOutlineColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("Cooldown Appearance")]
    [SerializeField] private Color cooldownColor = new Color(0.25f, 0.25f, 0.25f, 0.04f);
    [SerializeField] private Color meterColor = new Color(1f, 1f, 1f, 0.38f);
    [SerializeField] private Color cooldownTextColor = new Color(1f, 1f, 1f, 0.95f);
    [SerializeField] private Color blockedIconColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Recovery Flash")]
    [SerializeField, Range(0f, 1f)] private float recoveryFlashAlpha = 1f;
    [SerializeField, Min(0.01f)] private float flashDuration = 0.3f;

    private Sprite readyBackgroundSprite;
    private Sprite cooldownSprite;
    private Texture2D cooldownTexture;
    [SerializeField] private SearchSkillUIRing outline;
    private Player targetPlayer;
    private bool referencesReady;
    private bool hasPreviousState;
    private bool wasCoolingDown;
    private bool wasBlocked;

    public Player TargetPlayer => targetPlayer;
    public float RemainingCooldown { get; private set; }
    public float CooldownDuration { get; private set; }
    protected abstract Player ResolvePlayer();
    protected abstract bool TryReadCooldown(out float remaining, out float duration);
    protected virtual bool SkillIsAvailable => true;
    private bool appearanceDirty = true;
    private Color previousTeamColor;
    private Color readyColor;
    private Color flashColor;
    private float flashRemaining;
    private int displayedTenths = -1;

    protected virtual void Awake()
    {
        targetPlayer = ResolvePlayer();

        referencesReady = targetPlayer != null && background != null && (iconImage != null || iconText != null) &&
            cooldownOverlay != null && cooldownFill != null && cooldownText != null;

        if (!referencesReady)
        {
            Debug.LogError("SkillCooldownUI references are missing. Check the Inspector.", this);
            return;
        }

        readyBackgroundSprite = background.sprite;
        CreateCooldownSprite();
        cooldownFill.sprite = cooldownSprite;

        if (outline == null)
        {
            var outlineObject = new GameObject("SkillOutline", typeof(RectTransform), typeof(SearchSkillUIRing));
            outline = outlineObject.GetComponent<SearchSkillUIRing>();
            outline.raycastTarget = false;
            outline.rectTransform.SetParent(background.transform, false);
            outline.rectTransform.anchorMin = Vector2.zero;
            outline.rectTransform.anchorMax = Vector2.one;
            outline.rectTransform.offsetMin = Vector2.zero;
            outline.rectTransform.offsetMax = Vector2.zero;
        }

        // Keep the icon and timer above the fill.
        if (iconImage != null) iconImage.transform.SetAsLastSibling();
        else iconText.transform.SetAsLastSibling();
        cooldownText.transform.SetAsLastSibling();
    }

    protected virtual void OnEnable()
    {
        hasPreviousState = false;
        appearanceDirty = true;
        flashRemaining = 0f;
        displayedTenths = -1;
    }

    protected virtual void OnValidate()
    {
        // Apply Inspector changes on the next UI update, including during Play mode.
        appearanceDirty = true;
    }

    protected virtual void LateUpdate()
    {
        if (!referencesReady || targetPlayer == null ||
            !TryReadCooldown(out float sourceRemaining, out float sourceDuration))
            return;

        float remaining = Mathf.Max(0f, sourceRemaining);
        RemainingCooldown = remaining;
        CooldownDuration = Mathf.Max(0f, sourceDuration);
        bool isCoolingDown = remaining > 0f;
        bool isBlocked = !isCoolingDown && (!targetPlayer.isActiveAndEnabled || !SkillIsAvailable);
        bool stateChanged = !hasPreviousState || wasCoolingDown != isCoolingDown || wasBlocked != isBlocked;
        Color teamColor = targetPlayer.TeamColor;

        if (stateChanged)
        {
            // Only flash on recovery, never on the initial display.
            flashRemaining = hasPreviousState && wasCoolingDown && !isCoolingDown && !isBlocked ? flashDuration : 0f;
        }

        if (stateChanged || appearanceDirty || previousTeamColor != teamColor)
        {
            readyColor = useCustomReadyColor ? customReadyColor :
                Color.Lerp(teamColor, Color.white, teamColorSoftness);
            readyColor.a = readyAlpha;
            flashColor = Color.Lerp(readyColor, Color.white, 0.3f);
            flashColor.a = recoveryFlashAlpha;
            previousTeamColor = teamColor;
            ApplyAppearance(isCoolingDown, isBlocked);
            appearanceDirty = false;
        }

        if (isCoolingDown)
        {
            UpdateCooldown(remaining);
        }
        else if (flashRemaining > 0f)
        {
            // Animate color only during the short recovery flash.
            flashRemaining = Mathf.Max(0f, flashRemaining - Time.unscaledDeltaTime);
            float ratio = Mathf.Clamp01(flashRemaining / Mathf.Max(0.01f, flashDuration));
            background.color = Color.Lerp(readyColor, flashColor, ratio);
        }

        wasCoolingDown = isCoolingDown;
        wasBlocked = isBlocked;
        hasPreviousState = true;
    }

    private void ApplyAppearance(bool isCoolingDown, bool isBlocked)
    {
        bool subdued = isCoolingDown || isBlocked;
        background.sprite = subdued ? cooldownSprite : readyBackgroundSprite;
        background.color = subdued ? cooldownColor : readyColor;
        // Reinforce the sprite's existing rim rather than adding an outer halo.
        float inset = subdued ? 0f : readyBorderInset;
        outline.rectTransform.offsetMin = Vector2.one * inset;
        outline.rectTransform.offsetMax = -Vector2.one * inset;
        outline.Thickness = outlineThickness;
        outline.color = subdued ? cooldownOutlineColor : readyOutlineColor;
        outline.SetVerticesDirty();
        Graphic icon = iconImage != null ? (Graphic)iconImage : iconText;
        icon.color = isBlocked ? blockedIconColor : readyIconColor;
        icon.gameObject.SetActive(!isCoolingDown);
        cooldownOverlay.gameObject.SetActive(false);
        cooldownFill.color = meterColor;
        cooldownFill.gameObject.SetActive(isCoolingDown);
        cooldownText.color = cooldownTextColor;
        cooldownText.gameObject.SetActive(isCoolingDown);

        if (!isCoolingDown)
        {
            cooldownFill.fillAmount = 0f;
            cooldownText.text = string.Empty;
            displayedTenths = -1;
        }
    }

    private void CreateCooldownSprite()
    {
        // The built-in knob sprite includes a dark rim. Use a plain white disk
        // for cooldown fills so radial progress only applies the selected tint.
        const int size = 128;
        float radius = size * 0.5f;
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = new Vector2(x + 0.5f - radius, y + 0.5f - radius).magnitude;
                byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - distance) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }

        cooldownTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        cooldownTexture.name = "SearchCooldownDisk";
        cooldownTexture.hideFlags = HideFlags.DontSave;
        cooldownTexture.wrapMode = TextureWrapMode.Clamp;
        cooldownTexture.filterMode = FilterMode.Bilinear;
        cooldownTexture.SetPixels32(pixels);
        cooldownTexture.Apply(false, true);
        cooldownSprite = Sprite.Create(cooldownTexture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        cooldownSprite.name = "SearchCooldownDisk";
        cooldownSprite.hideFlags = HideFlags.DontSave;
    }

    protected virtual void OnDestroy()
    {
        if (cooldownSprite != null) Destroy(cooldownSprite);
        if (cooldownTexture != null) Destroy(cooldownTexture);
    }

    private void UpdateCooldown(float remaining)
    {
        float duration = CooldownDuration;
        float amount = duration > 0f ? Mathf.Clamp01(1f - remaining / duration) : 0f;
        if (cooldownFill.fillAmount != amount)
            cooldownFill.fillAmount = amount;

        // Update text only when the displayed tenth of a second changes.
        int tenths = Mathf.CeilToInt(remaining * 10f);
        if (displayedTenths != tenths)
        {
            cooldownText.text = (tenths / 10f).ToString("F1");
            displayedTenths = tenths;
        }
    }
}

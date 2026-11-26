using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public Player Owner { get; private set; }

    // 0 = 表示開始、1 = フェード完了
    public float CurrentProg { get; private set; }
    public bool IsTransitioning { get; private set; }

    [Header("リム表示")]
    [SerializeField, Min(0f)]
    private float revealedRimIntensity = 1f;

    [Header("時間（秒）")]
    [SerializeField, Min(0f)]
    private float holdDuration = 1f;

    [SerializeField, Min(0f)]
    private float fadeDuration = 3f;

    [Header("Renderer")]
    [SerializeField]
    private List<Renderer> meshRenderer = new();

    [SerializeField]
    private List<Renderer> holoMesh = new();

    [Header("ホログラム")]
    [SerializeField] private Material holoMaterial;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private sealed class RimTarget
    {
        public Renderer Renderer;
        public int ColorId;
        public int IntensityId;
        public MaterialPropertyBlock Block = new();
    }

    private readonly List<RimTarget> rimTargets = new();

    private readonly Dictionary<Renderer, Material[]>
        originalMaterials = new();

    private bool initialized;
    private bool holo;

    private Color revealedColor = Color.white;
    private float currentIntensity;
    private float elapsedTime;
    private float activeHoldDuration;
    private float activeFadeDuration;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        initialized = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Inspectorの設定を優先する
        if (holoMaterial == null)
        {
            holoMaterial =
                Resources.Load<Material>("Materials/HologramMaterial");
        }

        RegisterRimTargets();
        CacheOriginalMaterials();

        ResetColor();
    }

    private void RegisterRimTargets()
    {
        if (meshRenderer == null)
            return;

        var registered = new HashSet<Renderer>();

        foreach (Renderer target in meshRenderer)
        {
            if (target == null || !registered.Add(target))
                continue;

            Material material = target.sharedMaterial;

            if (material == null)
                continue;

            string colorName;
            string intensityName;

            // BaseMateria用
            if (material.HasProperty("_RimColor") &&
                material.HasProperty("_RimIntensity"))
            {
                colorName = "_RimColor";
                intensityName = "_RimIntensity";
            }
            // ToonBaseMaterial用
            else if (material.HasProperty("_Rim_Color") &&
                     material.HasProperty("_Rim_Intensity"))
            {
                colorName = "_Rim_Color";
                intensityName = "_Rim_Intensity";
            }
            else
            {
                Debug.LogWarning(
                    $"{target.name}のマテリアルにリム設定がありません。",
                    target);
                continue;
            }

            rimTargets.Add(new RimTarget
            {
                Renderer = target,
                ColorId = Shader.PropertyToID(colorName),
                IntensityId = Shader.PropertyToID(intensityName)
            });
        }
    }

    private void CacheOriginalMaterials()
    {
        if (holoMesh == null)
            return;

        foreach (Renderer target in holoMesh)
        {
            if (target == null ||
                originalMaterials.ContainsKey(target))
                continue;

            // Rendererごとに全スロットを保存
            originalMaterials.Add(target, target.sharedMaterials);
        }
    }

    public void SetOwner(Player player)
    {
        Initialize();

        Owner = player;
        ResetColor();

        if (Owner != null)
            RevealColor(Owner.TeamColor);
    }

    public void RevealColor(Color teamColor)
    {
        RevealColor(teamColor, holdDuration, fadeDuration);
    }

    public void RevealColor(
        Color teamColor,
        float visibleDuration,
        float returnDuration)
    {
        Initialize();

        revealedColor = teamColor;
        activeHoldDuration = Mathf.Max(0f, visibleDuration);
        activeFadeDuration = Mathf.Max(0f, returnDuration);

        elapsedTime = 0f;
        CurrentProg = 0f;
        IsTransitioning = true;

        ApplyRim(revealedRimIntensity);
        animator?.SetBool("bLightUp", true);

        if (activeHoldDuration == 0f && activeFadeDuration == 0f)
            CompleteReveal();
    }

    private void Update()
    {
        if (!IsTransitioning)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime < activeHoldDuration)
            return;

        CurrentProg = activeFadeDuration <= 0f
            ? 1f
            : Mathf.Clamp01(
                (elapsedTime - activeHoldDuration)
                / activeFadeDuration);

        // 色は固定し、Intensityだけを変える
        ApplyRim(Mathf.Lerp(
            revealedRimIntensity,
            0f,
            CurrentProg));

        if (CurrentProg >= 1f)
            CompleteReveal();
    }

    private void CompleteReveal()
    {
        CurrentProg = 1f;
        IsTransitioning = false;

        ApplyRim(0f);
        animator?.SetBool("bLightUp", false);
    }

    public void ResetColor()
    {
        Initialize();

        IsTransitioning = false;
        elapsedTime = 0f;
        CurrentProg = 0f;

        ApplyRim(0f);
        animator?.SetBool("bLightUp", false);
    }

    private void ApplyRim(float intensity)
    {
        currentIntensity = intensity;

        foreach (RimTarget target in rimTargets)
        {
            if (target.Renderer == null)
                continue;

            // ホログラム中は通常材質への設定を保留
            if (holo &&
                originalMaterials.ContainsKey(target.Renderer))
                continue;

            // 他のプロパティ設定を維持する
            target.Renderer.GetPropertyBlock(target.Block);

            target.Block.SetColor(
                target.ColorId, revealedColor);

            target.Block.SetFloat(
                target.IntensityId, intensity);

            target.Renderer.SetPropertyBlock(target.Block);
        }
    }

    public void SetHolo(bool active)
    {
        Initialize();

        if (holo == active)
            return;

        if (active && holoMaterial == null)
        {
            Debug.LogWarning(
                "ホログラム用マテリアルが未設定です。", this);
            return;
        }

        holo = active;

        foreach (var entry in originalMaterials)
        {
            Renderer target = entry.Key;

            if (target == null)
                continue;

            if (!active)
            {
                target.sharedMaterials = entry.Value;
                continue;
            }

            var materials = new Material[entry.Value.Length];

            for (int i = 0; i < materials.Length; i++)
                materials[i] = holoMaterial;

            target.sharedMaterials = materials;
        }

        if (!active)
            ApplyRim(currentIntensity);
    }

    private void OnTriggerEnter(Collider other)
    {
        SetHolo(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (FieldEventManager.Instance != null &&
            FieldEventManager.Instance.IsRotating)
            return;

        SetHolo(false);
    }
}
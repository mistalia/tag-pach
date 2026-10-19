
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 駒の持ち主と、配置後の色変化を管理する。
///
/// SetOwnerで持ち主を記録し、そのプレイヤーの色を表示する。
/// 設定時間の経過後、徐々に共通色へ戻る。
/// 色が戻っても持ち主の記録は維持する。
/// </summary>

public class Stone : MonoBehaviour
{
    // 既存の参照との互換性を保つため、名前は変更しない。
    public Player Owner { get; private set; }
    public Player GetOwner() { if (isVoid <= 0) return Owner; return null; }

    [Header("色の設定")]

    [Tooltip("配置前と色変化終了後の共通色。両チームとも同じ色に設定する。")]
    [SerializeField] private Color neutralColor = Color.gray;

    [Header("時間の設定（秒）")]

    [Tooltip("プレイヤーの色を保持する秒数。0なら、すぐに共通色へ戻り始める。")]
    [Min(0f)]
    [SerializeField] private float holdDuration = 1f;

    [Tooltip("共通色へ徐々に戻る秒数。0なら、保持時間の終了後に即座に戻る。")]
    [Min(0f)]
    [SerializeField] private float fadeDuration = 3f;

    [Header("色付け用メッシュの登録")]
    [SerializeField]private List<MeshRenderer> meshRenderer;
    [SerializeField] private List<MeshRenderer> holoMesh;

    [Header("Void化するターン")]
    [SerializeField] private const int minVoidTurn = 1;
    [SerializeField] private const int maxVoidTurn = 3;

    // この駒だけの表示を変更するために使用する。
    private MaterialPropertyBlock propertyBlock;
    private int colorPropertyId;

    private Material defaultMaterial;
    private Material holoMaterial;

    private Color revealedColor;
    public Color CurrentColor { get; private set; }
    private float elapsedTime;

    private float activeHoldDuration;
    private float activeFadeDuration;


    private bool isTransitioning;
    public bool IsTransitioning => isTransitioning;
    public float CurrentProg { get; private set; } = 0f;

    private bool isReady;

    public int isVoid{get;private set;} = 0;

    private void Awake()
    {
        InitializeColor();
        defaultMaterial = holoMesh[0].sharedMaterial;
        holoMaterial = Resources.Load<Material>("Materials/HologramMaterial");
    }

    /// <summary>
    /// 描画の準備を行い、初期状態を共通色にする。
    /// 準備済みなら再初期化しない。
    /// </summary>
    private bool InitializeColor()
    {
        if (isReady)
        {
            return true;
        }


        // 共有素材は確認にだけ使用し、直接変更しない。
        foreach (var renderer in meshRenderer)
        {
            if (renderer == null)
            {
                Debug.LogError("駒のMeshRendererが設定されていません。", this);
                return false;
            }
        }

        foreach (var renderer in meshRenderer)
        {


            Material material = renderer.sharedMaterial;

            if (material == null)
            {
                Debug.LogError("駒にマテリアルが設定されていません。", this);
                return false;
            }

            // 使用する素材に合わせて、色の設定項目を選ぶ。
            if (material.HasProperty("_BaseColor"))
            {
                colorPropertyId = Shader.PropertyToID("_BaseColor");
            }
            else if (material.HasProperty("_Color"))
            {
                colorPropertyId = Shader.PropertyToID("_Color");
            }
            else
            {
                Debug.LogError(
                    "駒のマテリアルに対応する色設定がありません。",
                    this);
                return false;
            }

            propertyBlock = new MaterialPropertyBlock();
            isReady = true;

            ApplyColor(neutralColor);
        }
        return true;
    }

    /// <summary>
    /// 配置処理から呼び出し、持ち主の記録と色変化を開始する。
    /// PlayerStoneColorはPlayerと同じGameObjectに付ける。
    ///
    /// 再度呼ぶと、色変化の時間を最初から数え直す。
    /// nullの場合は持ち主なし・共通色にする。
    /// </summary>
    public void SetOwner(Player player)
    {
        Owner = player;

        // 以前の色変化を終了する。
        isTransitioning = false;
        elapsedTime = 0f;

        // 色を表示できない場合も、持ち主の記録は維持する。
        if (!InitializeColor())
        {
            return;
        }

        ApplyColor(neutralColor);

        if (Owner == null)
        {
            return;
        }

        RevealColor(Owner.TeamColor);
    }

    /// <summary>
    /// 指定色を表示し、共通色へ戻るまでの計測を開始する。
    /// 持ち主は変更しない。再度呼ぶと時間がリセットされる。
    /// </summary>
    public void RevealColor(Color teamColor)
    {
        RevealColor(teamColor, holdDuration, fadeDuration);
    }

    public void RevealColor(
        Color teamColor,
        float visibleDuration,
        float returnDuration)
    {
        if (!InitializeColor())
        {
            return;
        }

        if(teamColor == null) {
            return;
        }

        activeHoldDuration = Mathf.Max(0f, visibleDuration);
        activeFadeDuration = Mathf.Max(0f, returnDuration);

        revealedColor = teamColor;
        elapsedTime = 0f;
        isTransitioning = true;

        ApplyColor(revealedColor);
    }

    private void Update()
    {
        if (!isTransitioning)
        {
            return;
        }

        // ゲーム内時間で計測する。
        // Time.timeScaleが0の間は色変化も止まる。
        elapsedTime += Time.deltaTime;

        float holdTime = activeHoldDuration;
        float fadeTime = activeFadeDuration;

        // 保持時間中は色を変えない。
        if (elapsedTime < holdTime)
        {
            return;
        }

        // 0＝プレイヤーの色、1＝共通色。
        // 変化時間が0なら、割り算せず完了扱いにする。
        CurrentProg = fadeTime <= 0f
            ? 1f
            : Mathf.Clamp01((elapsedTime - holdTime) / fadeTime);

        ApplyColor(Color.Lerp(revealedColor, neutralColor, CurrentProg));
        
        if (CurrentProg >= 1f)
        {
            isTransitioning = false;
        }
    }

    /// <summary>
    /// 共有素材を書き換えず、この駒だけに色を反映する。
    /// 同じGameObjectにRendererがあり、素材が1つの構成を想定。
    /// </summary>
    private void ApplyColor(Color color)
    {
        CurrentColor = color;

        foreach (var renderer in meshRenderer)
        {
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorPropertyId, color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }

    public void SetVoid()
    {
        Debug.Log("SetVoid");
        isVoid = Random.Range(minVoidTurn, maxVoidTurn);

        foreach (var renderer in holoMesh)
        {
            renderer.material.color = Color.black;
        }
    }

    public void DecreaseVoid()
    {
        if(isVoid > 0)
        {
            isVoid--;
            if(isVoid <= 0)
            {
                foreach (var renderer in holoMesh)
                {
                    renderer.material = defaultMaterial;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // ここで衝突時の処理を行う
            foreach (var renderer in holoMesh)
            {
                renderer.material = holoMaterial;
                if (isVoid > 0)
                {
                    renderer.material.color = Color.black;
                }
                else if(isTransitioning)
                {
                    renderer.material.color = Owner.TeamColor;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // ここで衝突終了時の処理を行う
            foreach(var renderer in holoMesh)
            {
                renderer.material = defaultMaterial;
                if(isVoid > 0)
                {
                    renderer.material.color = Color.black;
                }
            }
        }
    }

    private void OnDestroy()
    {
        
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ARScanController : MonoBehaviour
{
    public static ARScanController Instance { get; private set; }

    [Header("Scan Settings")]
    [SerializeField] private Transform scanCenterSource;
    [SerializeField] private float scanSpeed = 5f;
    [SerializeField] private float maxScanRadius = 30f;

    [Header("Overlay")]
    [SerializeField] private GameObject overlayRoot;

    [Header("Test")]
    [SerializeField] private bool testMode = false;

    // AROverlayRoot 配下の Renderer をまとめて保持
    private Renderer[] overlayRenderers;

    // Material を直接書き換えず、
    // RendererごとにShaderの値だけ変更するために使用
    private MaterialPropertyBlock propertyBlock;

    // スキャン開始時の位置を固定して保存
    private Vector3 fixedScanCenter;

    private float currentRadius = 0f;

    private bool isScanning = false;
    private bool isFinished = false;

    // Shader Graph 側の Reference 名
    private static readonly int ScanCenterID =
        Shader.PropertyToID("_ScanCenter");

    private static readonly int ScanRadiusID =
        Shader.PropertyToID("_ScanRadius");


    private void Awake()
    {
        // ARScanController が複数存在しないようにする
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("ARScanController already exists.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        propertyBlock = new MaterialPropertyBlock();

        // AROverlayRoot 以下にある Renderer をすべて取得
        if (overlayRoot != null)
        {
            overlayRenderers =
                overlayRoot.GetComponentsInChildren<Renderer>(true);
        }
        else
        {
            Debug.LogError("AROverlayRoot is not assigned.");
            overlayRenderers = new Renderer[0];
        }
    }


    private void Start()
    {
        currentRadius = 0f;

        // 開始前の初期位置を設定
        // 実際のスキャン開始時には、もう一度位置を取得する
        if (scanCenterSource != null)
        {
            fixedScanCenter = scanCenterSource.position;
            ApplyScanProperties();
        }
    }


    private void Update()
    {
        // ------------------------------
        // テストモード
        // ------------------------------
        // Inspectorで Test Mode をONにすると、
        // SpaceキーでTurnBattleStartStateを通さずに
        // スキャンを開始できる
        if (testMode &&
            Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnBattleStart();
        }

        // スキャン中でなければ何もしない
        if (!isScanning || isFinished)
            return;

        // ScanRadiusを時間経過で拡大
        currentRadius += scanSpeed * Time.deltaTime;

        // 最大値に到達
        if (currentRadius >= maxScanRadius)
        {
            currentRadius = maxScanRadius;

            // 最終値をShaderへ反映
            ApplyScanProperties();

            isScanning = false;
            isFinished = true;

            // Overlay削除処理へ
            StartCoroutine(FinishScan());

            return;
        }

        // 現在のRadiusをShaderへ反映
        ApplyScanProperties();
    }


    /// <summary>
    /// ゲーム開始時に呼ぶ関数。
    /// TurnBattleStartState から呼び出す想定。
    /// </summary>
    public void OnBattleStart()
    {
        // 二重開始防止
        if (isScanning || isFinished)
            return;

        if (scanCenterSource == null)
        {
            Debug.LogError("Scan Center Source is not assigned.");
            return;
        }

        if (overlayRoot == null)
        {
            Debug.LogError("AROverlayRoot is not assigned.");
            return;
        }

        // スキャン開始時点の位置を固定
        fixedScanCenter = scanCenterSource.position;

        // Radiusを最初から開始
        currentRadius = 0f;

        ApplyScanProperties();

        isScanning = true;

        Debug.Log($"AR Scan Start : Center = {fixedScanCenter}");
    }


    /// <summary>
    /// ScanCenterとScanRadiusを
    /// AROverlayRoot配下のRendererすべてへ反映する。
    /// </summary>
    private void ApplyScanProperties()
    {
        foreach (Renderer targetRenderer in overlayRenderers)
        {
            if (targetRenderer == null)
                continue;

            // そのRendererが現在持っているPropertyBlockを取得
            targetRenderer.GetPropertyBlock(propertyBlock);

            propertyBlock.SetVector(
                ScanCenterID,
                fixedScanCenter
            );

            propertyBlock.SetFloat(
                ScanRadiusID,
                currentRadius
            );

            // Shaderへ反映
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }


    /// <summary>
    /// スキャン完了後の後処理。
    /// Overlay群とARScanController自身を削除する。
    /// </summary>
    private IEnumerator FinishScan()
    {
        // 最終Radiusを1フレーム描画させてから削除
        yield return null;

        if (overlayRoot != null)
        {
            Destroy(overlayRoot);
        }

        // ARScanController自身も不要になるので削除
        Destroy(gameObject);
    }


    private void OnDestroy()
    {
        // Instanceが自分自身なら解除
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
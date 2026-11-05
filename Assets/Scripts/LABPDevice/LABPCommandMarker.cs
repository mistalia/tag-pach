using UnityEngine;

public class LABPCommandMarker : MonoBehaviour
{
    [Header("Mesh References")]
    private MeshFilter largeMeshFilter;
    private MeshFilter smallMeshFilter;

    [Header("Marker Shape")]
    [SerializeField, Min(0.01f)] private float markerWidth = 1.5f;
    [SerializeField, Min(0.01f)] private float markerLength = 0.6f;
    [SerializeField, Min(0.001f)] private float lineWidth = 0.15f;
    [SerializeField] private float yOffset = 0.01f;

    [Header("Marker Size")]
    [SerializeField, Min(0.01f)] private float largeScale = 1.0f;
    [SerializeField, Min(0.01f)] private float smallScale = 0.6f;

    [Header("Animation")]
    [SerializeField, Min(0f)] private float startDistance = 0.8f;
    [SerializeField, Min(0.01f)] private float moveDuration = 0.5f;

    private Mesh markerMesh;
    private float animationTimer;
    private bool isPlaying;

    // =========================================
    // 初期化
    // =========================================
    private void Awake()
    {
        // 子オブジェクトから自動取得
        Transform large = transform.Find("MarkerLarge");
        Transform small = transform.Find("MarkerSmall");

        largeMeshFilter = large?.GetComponent<MeshFilter>();
        smallMeshFilter = small?.GetComponent<MeshFilter>();

        if (largeMeshFilter == null || smallMeshFilter == null)
        {
            Debug.LogError("CommandMarkerのMeshFilterが見つかりません。", this);
            enabled = false;
            return;
        }

        // Mesh生成
        markerMesh = new Mesh();
        markerMesh.name = "LABP Command Marker Mesh";

        BuildMesh();

        // 同じMeshを共有
        largeMeshFilter.sharedMesh = markerMesh;
        smallMeshFilter.sharedMesh = markerMesh;

        SetupMarkers();

        gameObject.SetActive(false);
    }

    // =========================================
    // Mesh生成
    // =========================================
    private void BuildMesh()
    {
        if (markerMesh == null)
        {
            return;
        }

        markerMesh.Clear();

        float halfWidth = markerWidth * 0.5f;
        float halfLength = markerLength * 0.5f;

        float thickness = Mathf.Clamp(lineWidth, 0.001f, Mathf.Min(halfWidth, halfLength) * 0.9f);

        float innerWidth = halfWidth - thickness;
        float innerLength = halfLength - thickness;

        Vector3[] vertices = new Vector3[6];

        // 外側
        vertices[0] = new Vector3(-halfWidth, yOffset, -halfLength);

        vertices[1] = new Vector3(0f, yOffset, halfLength);

        vertices[2] = new Vector3(halfWidth, yOffset, -halfLength);

        // 内側
        vertices[3] = new Vector3(innerWidth, yOffset, -halfLength);

        vertices[4] = new Vector3(0f, yOffset, innerLength);

        vertices[5] = new Vector3(-innerWidth, yOffset, -halfLength);

        // 4三角形でV字を構成
        // 上向きの法線になるように設定
        int[] triangles =
        {
            0, 4, 1,
            0, 5, 4,

            1, 3, 2,
            1, 4, 3
        };

        Vector2[] uvs = new Vector2[6];

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / markerWidth + 0.5f, vertices[i].z / markerLength + 0.5f);
        }

        markerMesh.vertices = vertices;
        markerMesh.triangles = triangles;
        markerMesh.uv = uvs;

        markerMesh.RecalculateNormals();
        markerMesh.RecalculateBounds();
    }

    // =========================================
    // 大小マーカーの初期設定
    // =========================================
    private void SetupMarkers()
    {
        Transform large = largeMeshFilter.transform;
        Transform small = smallMeshFilter.transform;

        large.localPosition = Vector3.zero;
        large.localRotation = Quaternion.identity;
        large.localScale = Vector3.one * largeScale;

        small.localPosition = GetSmallStartPosition();
        small.localRotation = Quaternion.identity;
        small.localScale = Vector3.one * smallScale;
    }

    private Vector3 GetSmallStartPosition()
    {
        return new Vector3(0f,0f,-startDistance);
    }

    // =========================================
    // アニメーション
    // =========================================
    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (smallMeshFilter == null)
        {
            return;
        }

        animationTimer += Time.deltaTime;

        float t = animationTimer / moveDuration;

        // 到達したら初期位置へ戻す
        if (t >= 1f)
        {
            animationTimer = 0f;
            t = 0f;
        }

        // 小さい矢印を大きい矢印に近づける
        smallMeshFilter.transform.localPosition = Vector3.Lerp(GetSmallStartPosition(), Vector3.zero, t);
    }

    // =========================================
    // アニメーション開始
    // =========================================
    private void PlayAnimation()
    {
        animationTimer = 0f;
        isPlaying = true;

        smallMeshFilter.transform.localPosition = GetSmallStartPosition();
    }

    // =========================================
    // アニメーション停止
    // =========================================
    private void StopAnimation()
    {
        isPlaying = false;
        animationTimer = 0f;

        if (smallMeshFilter != null)
        {
            smallMeshFilter.transform.localPosition = GetSmallStartPosition();
        }
    }

    // =========================================
    // 表示
    // =========================================
    public void Show(Vector3 position, Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.position = position;

        transform.rotation = Quaternion.LookRotation(direction.normalized);

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            PlayAnimation();
        }
    }

    // =========================================
    // 非表示
    // =========================================
    public void Hide()
    {
        StopAnimation();
        gameObject.SetActive(false);
    }

    // =========================================
    // 終了処理
    // =========================================
    private void OnDestroy()
    {
        if (markerMesh != null)
        {
            Destroy(markerMesh);
        }
    }
}


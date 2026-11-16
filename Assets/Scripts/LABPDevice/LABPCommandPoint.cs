using UnityEngine;

public class LABP_CommandPoint : MonoBehaviour
{
    private Renderer[] pointRenderers;
    private SpriteRenderer[] wheelRenderers;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;

    private Vector3 spawnOffset = Vector3.zero;

    [Header("Wheel Animation")]
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatSpeed = 3.0f;
    [SerializeField] private float displayDuration = 3.0f;

    [Header("Wheel Transform")]
    [SerializeField] private Vector3 wheelOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Billboard")]
    [SerializeField] private bool useBillboard = true;
    [SerializeField] private Vector3 billboardRotationOffset = new Vector3(0f, 0f, 180f);
    
    [Header("Screen Offset")]
    [SerializeField] private float screenYOffset = 40.0f;
    private Camera targetCamera;

    [Header("Field")]
    private Transform fieldRoot;
    private Bounds fieldBounds;
    private bool hasFieldBounds;

    private GameObject repositionMesh;
    private Transform wheelFront;

    private Vector3 startPosition;
    private Vector3 wheelInitialPosition;

    private bool isSelecting;
    private bool isConfirmed;
    private float confirmedTimer;

    public bool IsSelecting => isSelecting;
    public bool IsConfirmed => isConfirmed;

    private void Awake()
    {
        Transform mesh = transform.Find("PointMesh");
        if (mesh != null)
        {
            repositionMesh = mesh.gameObject;
        }
        else
        {
            Debug.LogError("PointMesh is not found", this);
        }

        wheelFront = transform.Find("Wheel_front");
        if (wheelFront != null)
        {
            // Inspectorの配置ではなく、コード側で基準位置を決める
            wheelInitialPosition = wheelOffset;
            wheelFront.localPosition = wheelInitialPosition;
        }
        else
        {
            Debug.LogError("Wheel_front is not found", this);
        }

        if (repositionMesh != null)
        {
            pointRenderers =
                repositionMesh.GetComponentsInChildren<Renderer>(true);
        }

        if (wheelFront != null)
        {
            wheelRenderers =
                wheelFront.GetComponentsInChildren<SpriteRenderer>(true);
        }

        targetCamera = Camera.main;

        Hide();
    }

    private void Update()
    {
        if (!isConfirmed || wheelFront == null)
        {
            return;
        }

        UpdateWheelVisual();

        confirmedTimer -= Time.deltaTime;

        if (confirmedTimer <= 0.0f)
        {
            Hide();
        }
    }

    private void FindFieldRoot()
    {
        if (fieldRoot != null)
        {
            return;
        }

        FieldManager manager = FieldManager.Instance;
        if (manager == null)
        {
            manager = FindAnyObjectByType<FieldManager>();
        }

        if (manager == null)
        {
            Debug.LogError("FieldManagerが見つかりません。", this);
            return;
        }

        fieldRoot = manager.transform.Find("Generated Fields");

        if (fieldRoot == null)
        {
            Debug.LogError("Generated Fieldsが見つかりません。", this);
        }
    }

    private void UpdateFieldBounds()
    {
        hasFieldBounds = false;

        FindFieldRoot();

        if (fieldRoot == null)
        {
            return;
        }

        FieldTile[] tiles =
            fieldRoot.GetComponentsInChildren<FieldTile>();

        foreach (FieldTile tile in tiles)
        {
            if (tile == null)
            {
                continue;
            }

            Collider col = tile.GetComponent<Collider>();
            if (col == null)
            {
                continue;
            }

            if (!hasFieldBounds)
            {
                fieldBounds = col.bounds;
                hasFieldBounds = true;
            }
            else
            {
                fieldBounds.Encapsulate(col.bounds);
            }
        }
    }

    public void BeginSelection(Vector3 devicePosition)
    {
        UpdateFieldBounds();

        startPosition = devicePosition + spawnOffset;

        if (hasFieldBounds)
        {
            startPosition.x = Mathf.Clamp(
                startPosition.x,
                fieldBounds.min.x,
                fieldBounds.max.x);

            startPosition.z = Mathf.Clamp(
                startPosition.z,
                fieldBounds.min.z,
                fieldBounds.max.z);

            startPosition.y = fieldBounds.max.y;
        }

        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        isSelecting = true;
        isConfirmed = false;
        confirmedTimer = 0.0f;

        if (repositionMesh != null)
        {
            repositionMesh.SetActive(true);
        }

        if (wheelFront != null)
        {
            wheelFront.localPosition = wheelInitialPosition;
            wheelFront.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }

    public void MoveMarker(Vector2 input)
    {
        if (!isSelecting)
        {
            return;
        }

        Vector3 direction = new Vector3(
            input.x, 0.0f, input.y);

        direction = Vector3.ClampMagnitude(direction, 1.0f);

        Vector3 nextPosition =
            transform.position +
            direction * moveSpeed * Time.deltaTime;

        if (hasFieldBounds)
        {
            nextPosition.x = Mathf.Clamp(
                nextPosition.x,
                fieldBounds.min.x,
                fieldBounds.max.x);

            nextPosition.z = Mathf.Clamp(
                nextPosition.z,
                fieldBounds.min.z,
                fieldBounds.max.z);
        }

        transform.position = nextPosition;
    }

    public void Confirm()
    {
        if (!isSelecting)
        {
            return;
        }

        isSelecting = false;
        isConfirmed = true;
        confirmedTimer = displayDuration;

        if (wheelFront != null)
        {
            wheelFront.gameObject.SetActive(true);

            // 最初のフレームから位置を合わせる
            UpdateWheelVisual();
        }
    }

    public void Hide()
    {
        isSelecting = false;
        isConfirmed = false;
        confirmedTimer = 0.0f;

        if (wheelFront != null)
        {
            wheelFront.localPosition = wheelInitialPosition;
            wheelFront.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    public Vector3 GetTargetPosition()
    {
        return transform.position;
    }

    public void SetTeamColor(Color color)
    {
        if (pointRenderers != null)
        {
            foreach (Renderer renderer in pointRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_BaseColor"))
                    {
                        material.SetColor("_BaseColor", color);
                    }
                    else if (material.HasProperty("_Color"))
                    {
                        material.SetColor("_Color", color);
                    }
                }
            }
        }

        if (wheelRenderers != null)
        {
            foreach (SpriteRenderer renderer in wheelRenderers)
            {
                renderer.color = color;
            }
        }
    }

    private void UpdateWheelVisual()
    {
        if (wheelFront == null)
        {
            return;
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return;
        }

        Vector3 pointPosition = repositionMesh != null
            ? repositionMesh.transform.position
            : transform.position;

        // 円の画面座標を取得
        Vector3 screenPosition =
            targetCamera.WorldToScreenPoint(pointPosition);

        // 画面上で三角形を上にずらす
        screenPosition.y += screenYOffset;

        // 三角形の基準高さ
        float baseY =
            transform.position.y + wheelOffset.y;

        Ray ray =
            targetCamera.ScreenPointToRay(screenPosition);

        Plane plane = new Plane(
            Vector3.up,
            new Vector3(0f, baseY, 0f));

        Vector3 basePosition =
            pointPosition + Vector3.up * wheelOffset.y;

        if (plane.Raycast(ray, out float distance))
        {
            basePosition = ray.GetPoint(distance);
        }

        // 上下アニメーション
        float offset =
            Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        wheelFront.position =
            basePosition + Vector3.up * offset;

        // ビルボード
        if (useBillboard)
        {
            wheelFront.rotation =
                targetCamera.transform.rotation *
                Quaternion.Euler(billboardRotationOffset);
        }
    }
}


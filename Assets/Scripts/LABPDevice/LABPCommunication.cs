
using UnityEngine;
using UnityEngine.UI;

public class LABPCommunication : MonoBehaviour
{
    private LABPDevice device;

    private const int DirectionCount = 8;
    private GameObject bubbleRoot;
    [Header("Wheel Back Color")]
    [SerializeField] private Color backNormalColor = Color.white;
    [SerializeField] private Color backSelectedColor = Color.yellow;

    [Header("Wheel Front Color")]
    [SerializeField] private Color frontNormalColor = Color.white;
    [SerializeField] private Color frontSelectedColor = Color.yellow;

    private readonly Vector3[] backOriginalPositions = new Vector3[DirectionCount];
    private readonly Vector3[] backOriginalScales = new Vector3[DirectionCount];
    private readonly Vector3[] frontOriginalScales = new Vector3[DirectionCount];
    [SerializeField] private float selectedScale = 1.2f;

    [Header("Layout")]
    [SerializeField, Min(0f)] private float ringRadius = 100f;
    [SerializeField] private float arrowRadiusOffset = 0.3f;
    [SerializeField, Min(0f)] private float cameraOffset = 0.5f;
    [SerializeField] private float deadZone = 0.5f;

    private RectTransform directionPrefab;
    private readonly RectTransform[] directionBubbles = new RectTransform[DirectionCount];
    private static readonly string[] directionNames =
    {
        "Up",
        "UpRight",
        "Right",
        "DownRight",
        "Down",
        "DownLeft",
        "Left",
        "UpLeft"
    };

    [Header("Command Marker")]
    private LABPCommandMarker commandMarker;

    [SerializeField, Min(0f)]
    private float markerForwardOffset = 1.2f;


    // 確定方向はカメラ基準で計算
    private Camera targetCamera;

    private int selectedDirection = -1;
    private int confirmedDirection = -1;

    private bool isCommandOpen;
    private bool hasConfirmedDirection;

    private Quaternion confirmedWorldRotation;

    public bool IsCommandOpen => isCommandOpen;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        commandMarker = GetComponentInChildren<LABPCommandMarker>(true);

        // 吹き出しCanvasを取得
        Transform bubbleTransform = transform.Find("CommandBubbleCanvas");

        if (bubbleTransform == null)
        {
            Debug.LogError("CommandBubbleCanvas is not found", this);
            return;
        }

        bubbleRoot = bubbleTransform.gameObject;

        directionPrefab = bubbleTransform.Find("Up") as RectTransform;

        CreateDirectionBubbles();

        if (bubbleRoot != null)
        {
            bubbleRoot.SetActive(false);
        }

        device = GetComponent<LABPDevice>();
    }

    private void LateUpdate()
    {
        if (!hasConfirmedDirection)
        {
            return;
        }
    }

    // ========================================
    // 指示UIを開く
    // ========================================
    public void OpenCommand()
    {
        if (isCommandOpen || bubbleRoot == null)
        {
            return;
        }

        isCommandOpen = true;
        selectedDirection = -1;

        bubbleRoot.SetActive(true);

        UpdateBubbleSelection();
    }

    // ========================================
    // 方向選択
    // ========================================
    public void SelectDirection(Vector2 input)
    {
        if (!isCommandOpen)
        {
            return;
        }

        if (input.sqrMagnitude < deadZone * deadZone)
        {
            return;
        }

        // 上方向を0度とし、時計回りに計算
        float angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;

        angle = (angle + 360f) % 360f;

        int index = Mathf.RoundToInt(angle / 45f) % 8;

        if (selectedDirection == index)
        {
            return;
        }

        selectedDirection = index;

        UpdateBubbleSelection();
    }

    private void CreateDirectionBubbles()
    {
        if (directionPrefab == null)
        {
            Debug.LogError("Direction Prefab is not assigned.", this);
            return;
        }

        directionBubbles[0] = directionPrefab;

        // 残り7個を複製
        for (int i = 1; i < directionBubbles.Length; i++)
        {
            RectTransform clone = Instantiate(directionPrefab,directionPrefab.parent);

            clone.name = directionNames[i];

            directionBubbles[i] = clone;
        }

        // 最初に元のScaleを保存
        for (int i = 0; i < DirectionCount; i++)
        {
            Transform back = directionBubbles[i].Find("Wheel_back");

            Transform front = directionBubbles[i].Find("Wheel_front");

            if (back != null)
            {
                backOriginalScales[i] = back.localScale;
            }

            if (front != null)
            {
                frontOriginalScales[i] = front.localScale;
            }
        }

        UpdateBubblePositions();
        UpdateBubbleSelection();
    }

    // ========================================
    // 選択演出
    // ========================================
    private void UpdateBubbleSelection()
    {
        for (int i = 0; i < DirectionCount; i++)
        {
            RectTransform rect = directionBubbles[i];

            if (rect == null)
            {
                continue;
            }

            bool isSelected = i == selectedDirection;

            Transform wheelBack = rect.Find("Wheel_back");
            Transform wheelFront = rect.Find("Wheel_front");

            // リング背景
            if (wheelBack != null)
            {
                float scaleY = isSelected ? selectedScale : 1f;

                Vector3 scale = backOriginalScales[i];
                scale.y *= scaleY;

                wheelBack.localScale = scale;

                SpriteRenderer backRenderer = wheelBack.GetComponent<SpriteRenderer>();

                if (backRenderer != null)
                {
                    float height = backRenderer.sprite.bounds.size.y * backOriginalScales[i].y;

                    float offsetY = height * (scaleY - 1f) * 0.5f;

                    wheelBack.localPosition = backOriginalPositions[i] + new Vector3(0f, offsetY, 0f);

                    backRenderer.color = isSelected
                        ? backSelectedColor
                        : backNormalColor;

                    backRenderer.sortingOrder = 0;
                }
            }

            // 矢印
            if (wheelFront != null)
            {
                // 矢印の元サイズを維持
                wheelFront.localScale = frontOriginalScales[i];

                SpriteRenderer frontRenderer = wheelFront.GetComponent<SpriteRenderer>();

                if (frontRenderer != null)
                {
                    frontRenderer.color = isSelected
                        ? frontSelectedColor
                        : frontNormalColor;

                    // 背景より前に表示
                    frontRenderer.sortingOrder = 1;
                }
            }
        }
    }


    private void UpdateBubblePositions()
    {
        for (int i = 0; i < directionBubbles.Length; i++)
        {
            RectTransform rect = directionBubbles[i];

            if (rect == null)
            {
                continue;
            }

            // 親オブジェクトはCanvas中央に配置
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;

            // 上方向を0度として時計回り
            rect.localRotation = Quaternion.Euler(0f, 0f, -i * 45f);

            // 子オブジェクトを取得
            Transform wheelBack = rect.Find("Wheel_back");

            Transform wheelFront = rect.Find("Wheel_front");

            // リング背景
            if (wheelBack != null)
            {
                wheelBack.localPosition = new Vector3(0f, ringRadius, 0f);

                wheelBack.localRotation = Quaternion.identity;

                // 通常時の位置を保存
                backOriginalPositions[i] = wheelBack.localPosition;
            }

            // 矢印
            if (wheelFront != null)
            {
                wheelFront.localPosition = new Vector3(0f, ringRadius + arrowRadiusOffset, -0.1f);

                wheelFront.localRotation = Quaternion.identity;
            }
        }
    }


    // ========================================
    // 確定
    // ========================================
    public void ConfirmCommand()
    {
        if (!isCommandOpen || selectedDirection < 0)
        {
            return;
        }

        confirmedDirection = selectedDirection;

        Vector3 worldDirection = GetWorldDirection(confirmedDirection);

        if (worldDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        confirmedWorldRotation = Quaternion.LookRotation(worldDirection);

        // 吹き出しを閉じる
        CloseCommand();

        // Commandステートへ移行
        LABPDevice device = GetComponent<LABPDevice>();

        if (device != null)
        {
            device.ChangeState("command");
        }
    }

    // ========================================
    // 画面方向をワールド方向に変換
    // ========================================
    private Vector3 GetWorldDirection(int index)
    {
        float angle = index * 45f * Mathf.Deg2Rad;

        float x = Mathf.Sin(angle);
        float y = Mathf.Cos(angle);

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            return new Vector3(x, 0f, y).normalized;
        }

        Vector3 right = targetCamera.transform.right;
        Vector3 forward = targetCamera.transform.forward;

        right.y = 0f;
        forward.y = 0f;

        right.Normalize();
        forward.Normalize();

        Vector3 direction = right * x + forward * y;

        return direction.sqrMagnitude > 0.0001f
            ? direction.normalized
            : new Vector3(x, 0f, y).normalized;
    }

    // ========================================
    // キャンセル・閉じる
    // ========================================
    public void CloseCommand()
    {
        isCommandOpen = false;
        selectedDirection = -1;

        if (bubbleRoot != null)
        {
            bubbleRoot.SetActive(false);
        }
    }

    // ========================================
    // 確定矢印を消す
    // ========================================
    public void ShowCommandMarker()
    {
        if (commandMarker == null || confirmedDirection < 0)
        {
            return;
        }

        Vector3 direction = GetWorldDirection(confirmedDirection);

        // Deviceの少し前方
        Vector3 position = transform.position + direction * markerForwardOffset;

        Vector3 toCamera = (targetCamera.transform.position - position).normalized;
        position += toCamera * cameraOffset;

        commandMarker.Show(position, direction);
    }

    public void HideCommandMarker()
    {
        if (commandMarker != null)
        {
            commandMarker.Hide();
        }
    }

    public Vector3 GetConfirmedWorldDirection()
    {
        if (confirmedDirection < 0)
        {
            return Vector3.zero;
        }

        return GetWorldDirection(confirmedDirection);
    }

    public float GetCameraOffset()
    {
        return cameraOffset;
    }

    
}


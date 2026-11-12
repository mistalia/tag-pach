using UnityEngine;
using UnityEngine.UI;

// 対象UIのサイズに合わせてReady表示用の枠を自動生成する
[RequireComponent(typeof(RectTransform))]
public sealed class ControllerReadyFrame : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private ControllerFrameSettings settings;

    [Header("Controller")]
    [SerializeField, Range(1, ControllerSessionManager.MaxControllerCount)]
    private int controllerNumber = 1;

    [Header("Ready Source")]
    [SerializeField]
    private bool useSessionReadyState = true;

    private RectTransform targetRect;

    private Image topFrame;
    private Image bottomFrame;
    private Image leftFrame;
    private Image rightFrame;

    private ControllerSessionManager session;
    private bool isReady;

    private void Awake()
    {
        if (settings == null)
        {
            Debug.LogError("[ControllerReadyFrame] ControllerFrameSettingsが設定されていません。", this);
            enabled = false;
            return;
        }

        targetRect = GetComponent<RectTransform>();

        CreateFrames();
        RefreshFrame();
        SetReady(false);
    }

    private void Start()
    {
        if (!useSessionReadyState)
        {
            SetReady(false);
            return;
        }

        session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[ControllerReadyFrame] ControllerSessionManagerが存在しません。", this);
            return;
        }

        session.ControllerReadyChanged += HandleControllerReadyChanged;

        RefreshReadyState();
    }

    private void OnDestroy()
    {
        if (session != null)
            session.ControllerReadyChanged -= HandleControllerReadyChanged;
    }

    private void HandleControllerReadyChanged(int teamIndex, int changedControllerNumber, bool ready)
    {
        if (changedControllerNumber != controllerNumber)
            return;

        SetReady(ready);
    }

    private void RefreshReadyState()
    {
        ControllerRoleSelection selection = session?.GetControllerSelection(controllerNumber);

        SetReady(selection != null && selection.IsReady);
    }

    // Ready状態に応じて枠色を変更する
    public void SetReady(bool ready)
    {
        isReady = ready;

        RefreshColor();
    }

    // Settingsと現在のUIサイズから枠を更新する
    public void RefreshFrame()
    {
        if (settings == null || targetRect == null)
            return;

        RefreshLayout();
        RefreshColor();
    }

    private void CreateFrames()
    {
        topFrame    = CreateFrame("ReadyFrame_Top");
        bottomFrame = CreateFrame("ReadyFrame_Bottom");
        leftFrame   = CreateFrame("ReadyFrame_Left");
        rightFrame  = CreateFrame("ReadyFrame_Right");
    }

    private Image CreateFrame(string objectName)
    {
        GameObject frameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        frameObject.transform.SetParent(transform, false);

        Image image = frameObject.GetComponent<Image>();
        image.raycastTarget = false;

        return image;
    }

    private void RefreshLayout()
    {
        float thickness = settings.Thickness;
        float padding   = settings.Padding;

        SetHorizontalFrame(topFrame.rectTransform,      true,   thickness, padding);
        SetHorizontalFrame(bottomFrame.rectTransform,   false,  thickness, padding);
        SetVerticalFrame(leftFrame.rectTransform,       true,   thickness, padding);
        SetVerticalFrame(rightFrame.rectTransform,      false,  thickness, padding);
    }

    private void SetHorizontalFrame(RectTransform frame, bool isTop, float thickness, float padding)
    {
        frame.anchorMin         = new Vector2(0.0f, isTop ? 1.0f : 0.0f);
        frame.anchorMax         = new Vector2(1.0f, isTop ? 1.0f : 0.0f);
        frame.pivot             = new Vector2(0.5f, isTop ? 0.0f : 1.0f);
        frame.anchoredPosition  = new Vector2(0.0f, isTop ? padding : -padding);
        frame.sizeDelta         = new Vector2(padding * 2.0f, thickness);
    }

    private void SetVerticalFrame(RectTransform frame, bool isLeft, float thickness, float padding)
    {
        frame.anchorMin         = new Vector2(isLeft ? 0.0f : 1.0f, 0.0f);
        frame.anchorMax         = new Vector2(isLeft ? 0.0f : 1.0f, 1.0f);
        frame.pivot             = new Vector2(isLeft ? 1.0f : 0.0f, 0.5f);
        frame.anchoredPosition  = new Vector2(isLeft ? -padding : padding, 0.0f);
        frame.sizeDelta         = new Vector2(thickness, padding * 2.0f);
    }

    private void RefreshColor()
    {
        if (settings == null)
            return;

        Color color = isReady ? settings.ReadyColor : settings.WaitingColor;

        SetColor(topFrame, color);
        SetColor(bottomFrame, color);
        SetColor(leftFrame, color);
        SetColor(rightFrame, color);
    }

    private static void SetColor(Image image, Color color)
    {
        if (image != null)
            image.color = color;
    }
}
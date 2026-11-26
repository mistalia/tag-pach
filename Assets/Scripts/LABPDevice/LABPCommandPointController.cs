
using UnityEngine;

public class LABPCommandPointController : MonoBehaviour
{
    private LABPDevice device;
    private LABP_CommandPoint commandPoint;

    private bool isCommandOpen;

    public bool IsCommandOpen => isCommandOpen;

    private void Awake()
    {
        device = GetComponent<LABPDevice>();

        // 非アクティブの子も含めて取得
        commandPoint = GetComponentInChildren<LABP_CommandPoint>(true);

        if (commandPoint == null)
        {
            Debug.LogError("LABP_CommandPoint is not found", this);
            return;
        }

        // Deviceから独立させる
        commandPoint.transform.SetParent(null, true);
    }

    private void Start()
    {
        if (device == null || commandPoint == null)
        {
            return;
        }
    }

    // =====================================
    // コマンド開始
    // =====================================
    public void OpenCommand()
    {
        if (isCommandOpen || commandPoint == null)
        {
            return;
        }

        if (device != null && (device.IsCommanding() || device.IsPlacing))
        {
            return;
        }

        commandPoint.BeginSelection(transform.position);
        isCommandOpen = true;
    }

    // =====================================
    // 位置選択
    // =====================================
    public void SelectPosition(Vector2 input)
    {
        if (!isCommandOpen || commandPoint == null)
        {
            return;
        }

        commandPoint.MoveMarker(input);
    }

    // =====================================
    // 位置確定
    // =====================================
    public void ConfirmCommand()
    {
        if (!isCommandOpen || commandPoint == null)
        {
            return;
        }

        commandPoint.Confirm();

        isCommandOpen = false;

        // 確定後もマーカーを表示したままにする
    }

    // =====================================
    // キャンセル・非表示
    // =====================================
    public void CloseCommand()
    {
        isCommandOpen = false;

        if (commandPoint != null)
        {
            commandPoint.Hide();
        }
    }

    // =====================================
    // 確定位置を取得
    // =====================================
    public Vector3 GetTargetPosition()
    {
        if (commandPoint == null ||
            !commandPoint.IsConfirmed)
        {
            return Vector3.zero;
        }

        return commandPoint.GetTargetPosition();
    }

    public bool HasConfirmedPosition()
    {
        return commandPoint != null &&
               commandPoint.IsConfirmed;
    }
}


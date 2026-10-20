using System;
using UnityEngine;

[Serializable]
public sealed class InputState
{
    public bool Pressed { get; private set; }

    public bool Released { get; private set; }

    public bool Hold { get; private set; }

    public void SetPressed()
    {
        Pressed = true;
        Hold = true;
    }

    public void SetReleased()
    {
        Hold = false;
        Released = true;
    }

    public void ClearFrameFlags()
    {
        Pressed = false;
        Released = false;
    }

    public void ClearAllFlags()
    {
        Pressed = false;
        Released = false;
        Hold = false;
    }
}

// 操作担当と配置担当の入力を、PlayerとPlayerStateへ提供する共通の窓口
public sealed class PlayerInputReader : MonoBehaviour
{
    [Header("Input Readers")]

    [SerializeField]
    private OperationInputReader operationInputReader;

    [SerializeField]
    private PlacementInputReader placementInputReader;

    // Playerの移動は常に操作担当コントローラーの入力を使用する
    public Vector2 MoveValue =>
        operationInputReader != null
            ? operationInputReader.MoveValue
            : Vector2.zero;

    public InputState Attack =>
        operationInputReader?.Attack;

    public InputState Skill =>
        placementInputReader?.Skill;

    public InputState Place =>
        placementInputReader?.Place;

    public bool IsConfigured =>
        operationInputReader != null &&
        placementInputReader != null;

    private void Awake()
    {
        if (operationInputReader == null)
        {
            Debug.LogError(
                "[PlayerInputReader] " +
                "OperationInputReaderが設定されていません。",
                this
            );
        }

        if (placementInputReader == null)
        {
            Debug.LogError(
                "[PlayerInputReader] " +
                "PlacementInputReaderが設定されていません。",
                this
            );
        }
    }

    public void SetInputEnable(bool enable)
    {
        if (operationInputReader != null)
        {
            operationInputReader.SetInputEnable(enable);
        }

        if (placementInputReader != null)
        {
            placementInputReader.SetInputEnable(enable);
        }
    }
}
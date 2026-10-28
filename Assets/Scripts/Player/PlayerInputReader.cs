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

// Operation‚ÆPlacement‚Ì“ü—Í‚ðPlayer‚Ö’ñ‹Ÿ‚·‚é
public sealed class PlayerInputReader : MonoBehaviour
{
    [Header("Four Controllers Mode")]
    [SerializeField]
    private OperationInputReader operationInputReader;

    [SerializeField]
    private PlacementInputReader placementInputReader;

    [Header("Two Controllers Test Mode")]
    [SerializeField]
    private OperationInputReader testOperationInputReader;

    [SerializeField]
    private PlacementInputReader testPlacementInputReader;

    private OperationInputReader currentOperationInputReader;
    private PlacementInputReader currentPlacementInputReader;

    public Vector2 MoveValue => currentOperationInputReader != null ? currentOperationInputReader.MoveValue : Vector2.zero;

    public InputState Attack => currentOperationInputReader?.Attack;

    public InputState ForcePlace => currentOperationInputReader?.ForcePlace;

    public InputState Skill => currentPlacementInputReader?.Skill;

    public InputState Place => currentPlacementInputReader?.Place;

    private void Awake()
    {
        UseFourControllersMode();
    }

    public void UseFourControllersMode()
    {
        currentOperationInputReader = operationInputReader;
        currentPlacementInputReader = placementInputReader;
    }

    public void UseTwoControllersTestMode()
    {
        currentOperationInputReader = testOperationInputReader;
        currentPlacementInputReader = testPlacementInputReader;
    }

    public void SetInputEnable(bool enable)
    {
        if (currentOperationInputReader != null)
            currentOperationInputReader.SetInputEnable(enable);

        if (currentPlacementInputReader != null)
            currentPlacementInputReader.SetInputEnable(enable);
    }
}
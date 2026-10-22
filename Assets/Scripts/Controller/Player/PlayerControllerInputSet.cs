using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 1l‚ÌPlayer–{‘Ì‚ÆA–ðŠ„•Ê‚Ì“ü—ÍŽó‚¯Œû‚ð‚Ü‚Æ‚ß‚Ä•ÛŽ
[Serializable]
public sealed class PlayerControllerInputSet
{
    [SerializeField]
    private GameObject playerObject;

    [Header("Four Controllers Mode")]
    [SerializeField]
    private PlayerInput operationPlayerInput;

    [SerializeField]
    private PlayerInput placementPlayerInput;

    [Header("Two Controllers Test Mode")]
    [SerializeField]
    private PlayerInput testPlayerInput;

    [SerializeField]
    private PlayerInputReader playerInputReader;
    public PlayerInputReader PlayerInputReader => playerInputReader;

    public GameObject PlayerObject => playerObject;

    public PlayerInput OperationPlayerInput => operationPlayerInput;

    public PlayerInput PlacementPlayerInput => placementPlayerInput;

    public PlayerInput TestPlayerInput => testPlayerInput;

    public bool IsFourControllersConfigured => playerObject != null && playerInputReader != null && operationPlayerInput != null && placementPlayerInput != null;

    public bool IsTestModeConfigured => playerObject != null && playerInputReader != null && testPlayerInput != null;

    public void SetFourControllersInputActive(bool isActive)
    {
        SetPlayerInputActive(operationPlayerInput, isActive);
        SetPlayerInputActive(placementPlayerInput, isActive);
        SetPlayerInputActive(testPlayerInput, false);
    }

    public void SetTestInputActive(bool isActive)
    {
        SetPlayerInputActive(operationPlayerInput, false);
        SetPlayerInputActive(placementPlayerInput, false);
        SetPlayerInputActive(testPlayerInput, isActive);
    }

    private static void SetPlayerInputActive(PlayerInput playerInput, bool isActive)
    {
        if (playerInput != null)
            playerInput.gameObject.SetActive(isActive);
    }

    public void SetPlayerActive(bool isActive)
    {
        if (playerObject != null)
            playerObject.SetActive(isActive);
    }
}
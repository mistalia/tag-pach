using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 1l‚ÌPlayer–{‘Ì‚ÆA–ðŠ„•Ê‚Ì“ü—ÍŽó‚¯Œû‚ð‚Ü‚Æ‚ß‚Ä•ÛŽ
[Serializable]
public sealed class PlayerControllerInputSet
{
    [SerializeField]
    private GameObject playerObject;

    [SerializeField]
    private PlayerInput operationPlayerInput;

    [SerializeField]
    private PlayerInput placementPlayerInput;

    public GameObject PlayerObject => playerObject;

    public PlayerInput OperationPlayerInput => operationPlayerInput;

    public PlayerInput PlacementPlayerInput => placementPlayerInput;

    public bool IsConfigured => playerObject != null && operationPlayerInput != null && placementPlayerInput != null;

    public PlayerInput GetPlayerInput(ControllerRole role)
    {
        switch (role)
        {
            case ControllerRole.Operation:
                return operationPlayerInput;

            case ControllerRole.Placement:
                return placementPlayerInput;

            default:
                return null;
        }
    }

    public void SetPlayerActive(bool isActive)
    {
        if (playerObject != null)
            playerObject.SetActive(isActive);
    }
}
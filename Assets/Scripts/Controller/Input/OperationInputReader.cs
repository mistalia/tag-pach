using UnityEngine;
using UnityEngine.InputSystem;

// 操作担当コントローラーから配置入力を受け取る
[RequireComponent(typeof(PlayerInput))]
public sealed class OperationInputReader : MonoBehaviour
{
    public Vector2 MoveValue { get; private set; }

    public InputState Attack { get; } = new InputState();

    public InputState ForcePlace { get; } = new InputState();

    private PlayerInput playerInput;

    private InputAction moveAction;

    private InputAction attackAction;

    private InputAction forcePlaceAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "[OperationInputReader] " +
                "PlayerInputコンポーネントが見つかりません。",
                this
            );

            enabled = false;
            return;
        }

        moveAction = playerInput.actions.FindAction("Operation/Move", true);

        attackAction = playerInput.actions.FindAction("Operation/Attack", true);

        moveAction.performed += HandleMove;
        moveAction.canceled += HandleMoveCanceled;

        attackAction.performed += HandleAttack;
        attackAction.canceled += HandleAttackCanceled;

        forcePlaceAction = playerInput.actions.FindAction("Operation/ForcePlace", true);

        forcePlaceAction.performed += HandleForcePlace;
        forcePlaceAction.canceled += HandleForcePlaceCanceled;
    }

    private void OnDestroy()
    {
        if (moveAction != null)
        {
            moveAction.performed -= HandleMove;
            moveAction.canceled -= HandleMoveCanceled;
        }

        if (attackAction != null)
        {
            attackAction.performed -= HandleAttack;
            attackAction.canceled -= HandleAttackCanceled;
        }

        if (forcePlaceAction != null)
        {
            forcePlaceAction.performed -= HandleForcePlace;
            forcePlaceAction.canceled -= HandleForcePlaceCanceled;
        }
    }

    private void LateUpdate()
    {
        Attack.ClearFrameFlags();
        ForcePlace.ClearFrameFlags();
    }

    public void SetInputEnable(bool enable)
    {
        if (playerInput == null)
            return;

        if (enable)
        {
            playerInput.ActivateInput();
            return;
        }

        playerInput.DeactivateInput();

        MoveValue = Vector2.zero;
        Attack.ClearAllFlags();
        ForcePlace.ClearAllFlags();
    }

    private void HandleMove(InputAction.CallbackContext context)
    {
        MoveValue = context.ReadValue<Vector2>();
    }

    private void HandleMoveCanceled(InputAction.CallbackContext context)
    {
        MoveValue = Vector2.zero;
    }

    private void HandleAttack(InputAction.CallbackContext context)
    {
        Attack.SetPressed();
    }

    private void HandleAttackCanceled(InputAction.CallbackContext context)
    {
        Attack.SetReleased();
    }

    private void HandleForcePlace(InputAction.CallbackContext context)
    {
        ForcePlace.SetPressed();
    }

    private void HandleForcePlaceCanceled(InputAction.CallbackContext context)
    {
        ForcePlace.SetReleased();
    }
}
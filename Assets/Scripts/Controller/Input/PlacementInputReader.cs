using UnityEngine;
using UnityEngine.InputSystem;

// 配置担当コントローラーから配置入力を受け取る
[RequireComponent(typeof(PlayerInput))]
public sealed class PlacementInputReader : MonoBehaviour
{
    public InputState Place { get; } = new InputState();

    public InputState Skill { get; } = new InputState();

    private PlayerInput playerInput;

    private InputAction placeAction;

    private InputAction skillAction;

    //------------------------------------------------------------------------------
    public InputState Command { get; } = new InputState();

    public InputState Select { get; } = new InputState();

    private InputAction commandAction;
    private InputAction selectAction;
    //------------------------------------------------------------------------------


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError(
                "[PlacementInputReader] " +
                "PlayerInputコンポーネントが見つかりません。",
                this
            );

            enabled = false;
            return;
        }

        placeAction = playerInput.actions.FindAction("Placement/Place", true);

        skillAction = playerInput.actions.FindAction("Placement/Skill", true);

        placeAction.performed   += HandlePlace;
        placeAction.canceled    += HandlePlaceCanceled;

        skillAction.performed   += HandleSkill;
        skillAction.canceled    += HandleSkillCanceled;

        //------------------------------------------------------------------------------
        commandAction = playerInput.actions.FindAction("Placement/Command", true);

        selectAction = playerInput.actions.FindAction("Placement/Select", true);

        commandAction.performed += HandleCommand;
        commandAction.canceled += HandleCommandCanceled;

        selectAction.performed += HandleSelect;
        selectAction.canceled += HandleSelectCanceled;
        //------------------------------------------------------------------------------
    }

    private void OnDestroy()
    {
        if (placeAction != null)
        {
            placeAction.performed -= HandlePlace;
            placeAction.canceled -= HandlePlaceCanceled;
        }

        if (skillAction != null)
        {
            skillAction.performed -= HandleSkill;
            skillAction.canceled -= HandleSkillCanceled;
        }

        //------------------------------------------------------------------------------
        if (commandAction != null)
        {
            commandAction.performed -= HandleCommand;
            commandAction.canceled -= HandleCommandCanceled;
        }

        if (selectAction != null)
        {
            selectAction.performed -= HandleSelect;
            selectAction.canceled -= HandleSelectCanceled;
        }
        //------------------------------------------------------------------------------

    }

    private void LateUpdate()
    {
        Place.ClearFrameFlags();
        Skill.ClearFrameFlags();

        //------------------------------------------------------------------------------
        Command.ClearFrameFlags();
        Select.ClearFrameFlags();
        //------------------------------------------------------------------------------
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

        Place.ClearAllFlags();
        Skill.ClearAllFlags();

        //------------------------------------------------------------------------------
        Command.ClearAllFlags();
        Select.ClearAllFlags();
        //------------------------------------------------------------------------------

    }

    private void HandlePlace(InputAction.CallbackContext context)
    {
        Place.SetPressed();
    }

    private void HandlePlaceCanceled(InputAction.CallbackContext context)
    {
        Place.SetReleased();
    }

    private void HandleSkill(InputAction.CallbackContext context)
    {
        Skill.SetPressed();
    }

    private void HandleSkillCanceled(InputAction.CallbackContext context)
    {
        Skill.SetReleased();
    }

    //------------------------------------------------------------------------------
    private void HandleCommand(InputAction.CallbackContext context)
    {
        Command.SetPressed();
    }

    private void HandleCommandCanceled(InputAction.CallbackContext context)
    {
        Command.SetReleased();
    }

    private void HandleSelect(InputAction.CallbackContext context)
    {
        Select.SetPressed();
    }

    private void HandleSelectCanceled(InputAction.CallbackContext context)
    {
        Select.SetReleased();
    }
    //------------------------------------------------------------------------------
}
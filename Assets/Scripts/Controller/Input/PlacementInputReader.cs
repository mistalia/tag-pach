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
    }

    private void LateUpdate()
    {
        Place.ClearFrameFlags();
        Skill.ClearFrameFlags();
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
}
using UnityEngine;
using UnityEngine.InputSystem;

// 4台モードでTeam内の各ControllerからRole選択・OK・Cancel入力を受け取る
public sealed class ControllerRoleSelectionInput : MonoBehaviour
{
    private const float NavigateThreshold = 0.5f;
    private const float NavigateReleaseThreshold = 0.2f;

    [Header("Team")]
    [SerializeField, Range(0, ControllerSessionManager.TeamCount - 1)]
    private int teamIndex;

    [Header("Input Actions")]
    [SerializeField]
    private InputActionAsset inputActions;

    [SerializeField]
    private string navigateActionName = "ControllerSelection/Navigate";

    [SerializeField]
    private string submitActionName = "ControllerSelection/Submit";

    [SerializeField]
    private string cancelActionName = "ControllerSelection/Cancel";

    private InputActionAsset firstControllerActions;
    private InputActionAsset secondControllerActions;

    private InputAction firstNavigateAction;
    private InputAction firstSubmitAction;
    private InputAction firstCancelAction;

    private InputAction secondNavigateAction;
    private InputAction secondSubmitAction;
    private InputAction secondCancelAction;

    private bool isFirstNavigateLocked;
    private bool isSecondNavigateLocked;
    private bool isInitialized;

    private void Start()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[ControllerRoleSelectionInput] ControllerSessionManagerが存在しません。", this);
            return;
        }

        // BindSceneを直接起動した場合はController登録完了後に再初期化する
        session.ControllerRegistered += HandleControllerRegistered;

        TryInitialize();
    }

    private void Update()
    {
        UpdateNavigateLock(firstNavigateAction, ref isFirstNavigateLocked);
        UpdateNavigateLock(secondNavigateAction, ref isSecondNavigateLocked);
    }

    private void OnDestroy()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session != null)
            session.ControllerRegistered -= HandleControllerRegistered;

        ReleaseActions();
    }

    private void HandleControllerRegistered(int registeredTeamIndex, RegisteredController controller)
    {
        if (registeredTeamIndex != teamIndex)
            return;

        TryInitialize();
    }

    // Team内の2台が登録された時点で、それぞれ専用のInput Actionを準備する
    private void TryInitialize()
    {
        if (isInitialized)
            return;

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null || session.BindingMode != ControllerBindingMode.FourControllers)
            return;

        if (inputActions == null)
        {
            Debug.LogError("[ControllerRoleSelectionInput] Input Action Assetが設定されていません。", this);
            return;
        }

        ControllerTeam team = session.GetTeam(teamIndex);

        if (team == null || !team.HasBothControllers)
            return;

        CreateFirstControllerActions(team.FirstController);
        CreateSecondControllerActions(team.SecondController);

        isInitialized = true;
    }

    // Assetを複製し、First ControllerのDeviceだけを受け付けるように制限する
    private void CreateFirstControllerActions(RegisteredController controller)
    {
        firstControllerActions = Instantiate(inputActions);
        firstControllerActions.devices = new[] { controller.Device };

        firstNavigateAction = firstControllerActions.FindAction(navigateActionName, true);
        firstSubmitAction = firstControllerActions.FindAction(submitActionName, true);
        firstCancelAction = firstControllerActions.FindAction(cancelActionName, true);

        firstNavigateAction.performed += HandleFirstNavigate;
        firstSubmitAction.performed += HandleFirstSubmit;
        firstCancelAction.performed += HandleFirstCancel;

        firstControllerActions.Enable();
    }

    // Assetを複製し、Second ControllerのDeviceだけを受け付けるように制限する
    private void CreateSecondControllerActions(RegisteredController controller)
    {
        secondControllerActions = Instantiate(inputActions);
        secondControllerActions.devices = new[] { controller.Device };

        secondNavigateAction = secondControllerActions.FindAction(navigateActionName, true);
        secondSubmitAction = secondControllerActions.FindAction(submitActionName, true);
        secondCancelAction = secondControllerActions.FindAction(cancelActionName, true);

        secondNavigateAction.performed += HandleSecondNavigate;
        secondSubmitAction.performed += HandleSecondSubmit;
        secondCancelAction.performed += HandleSecondCancel;

        secondControllerActions.Enable();
    }

    private void HandleFirstNavigate(InputAction.CallbackContext context)
    {
        if (isFirstNavigateLocked)
            return;

        Vector2 navigation = context.ReadValue<Vector2>();

        if (Mathf.Abs(navigation.x) < NavigateThreshold)
            return;

        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);
        int controllerNumber = team?.FirstController?.ControllerNumber ?? -1;

        if (ControllerSessionManager.Instance?.ToggleControllerRole(controllerNumber, context.control.device) == true)
            isFirstNavigateLocked = true;
    }

    private void HandleSecondNavigate(InputAction.CallbackContext context)
    {
        if (isSecondNavigateLocked)
            return;

        Vector2 navigation = context.ReadValue<Vector2>();

        if (Mathf.Abs(navigation.x) < NavigateThreshold)
            return;

        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);
        int controllerNumber = team?.SecondController?.ControllerNumber ?? -1;

        if (ControllerSessionManager.Instance?.ToggleControllerRole(controllerNumber, context.control.device) == true)
            isSecondNavigateLocked = true;
    }

    private void HandleFirstSubmit(InputAction.CallbackContext context)
    {
        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);
        int controllerNumber = team?.FirstController?.ControllerNumber ?? -1;

        ControllerSessionManager.Instance?.SetControllerReady(controllerNumber, context.control.device);
    }

    private void HandleSecondSubmit(InputAction.CallbackContext context)
    {
        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);
        int controllerNumber = team?.SecondController?.ControllerNumber ?? -1;

        ControllerSessionManager.Instance?.SetControllerReady(controllerNumber, context.control.device);
    }

    private void HandleFirstCancel(InputAction.CallbackContext context)
    {
        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);
        int controllerNumber = team?.FirstController?.ControllerNumber ?? -1;

        ControllerSessionManager.Instance?.CancelControllerReady(controllerNumber, context.control.device);
    }

    private void HandleSecondCancel(InputAction.CallbackContext context)
    {
        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);
        int controllerNumber = team?.SecondController?.ControllerNumber ?? -1;

        ControllerSessionManager.Instance?.CancelControllerReady(controllerNumber, context.control.device);
    }

    // スティックを中央へ戻すまで、同じ入力による連続Role変更を防止する
    private static void UpdateNavigateLock(InputAction navigateAction, ref bool isLocked)
    {
        if (!isLocked || navigateAction == null)
            return;

        Vector2 navigation = navigateAction.ReadValue<Vector2>();

        if (Mathf.Abs(navigation.x) <= NavigateReleaseThreshold)
            isLocked = false;
    }

    // 複製したActionの購読を解除し、生成したAssetを破棄する
    private void ReleaseActions()
    {
        if (firstNavigateAction != null)
            firstNavigateAction.performed -= HandleFirstNavigate;

        if (firstSubmitAction != null)
            firstSubmitAction.performed -= HandleFirstSubmit;

        if (firstCancelAction != null)
            firstCancelAction.performed -= HandleFirstCancel;

        if (secondNavigateAction != null)
            secondNavigateAction.performed -= HandleSecondNavigate;

        if (secondSubmitAction != null)
            secondSubmitAction.performed -= HandleSecondSubmit;

        if (secondCancelAction != null)
            secondCancelAction.performed -= HandleSecondCancel;

        if (firstControllerActions != null)
        {
            firstControllerActions.Disable();
            Destroy(firstControllerActions);
        }

        if (secondControllerActions != null)
        {
            secondControllerActions.Disable();
            Destroy(secondControllerActions);
        }

        firstControllerActions = null;
        secondControllerActions = null;

        firstNavigateAction = null;
        firstSubmitAction = null;
        firstCancelAction = null;

        secondNavigateAction = null;
        secondSubmitAction = null;
        secondCancelAction = null;

        isFirstNavigateLocked = false;
        isSecondNavigateLocked = false;
        isInitialized = false;
    }
}
using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 1Team分のOK・Cancel・CHANGE・最終確認入力を管理する
public sealed class RematchReadyInput : MonoBehaviour
{
    // 1台分の複製したInput Actionと各Actionをまとめて保持する
    private sealed class ControllerActions
    {
        public InputActionAsset Asset { get; }
        public InputAction Submit { get; }
        public InputAction Cancel { get; }
        public InputAction Change { get; }
        public InputAction Proceed { get; }

        public ControllerActions(InputActionAsset asset, InputAction submit, InputAction cancel, InputAction change, InputAction proceed)
        {
            Asset = asset;
            Submit = submit;
            Cancel = cancel;
            Change = change;
            Proceed = proceed;
        }
    }

    [Header("Team")]
    [SerializeField, Range(0, ControllerSessionManager.TeamCount - 1)]
    private int teamIndex;

    [Header("Input Actions")]
    [SerializeField]
    private InputActionAsset inputActions;

    [SerializeField]
    private string submitActionName = "ControllerSelection/Submit";

    [SerializeField]
    private string cancelActionName = "ControllerSelection/Cancel";

    [SerializeField]
    private string changeActionName = "ControllerSelection/Change";

    [SerializeField]
    private string proceedActionName = "SceneFlow/Proceed";

    private ControllerActions firstControllerActions;
    private ControllerActions secondControllerActions;

    private bool isInitialized;

    private void Start()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[RematchReadyInput] ControllerSessionManagerが存在しません。", this);
            return;
        }

        // Controller登録がScene開始後の場合も、登録完了後に入力を初期化する
        session.ControllerRegistered += HandleControllerRegistered;

        TryInitialize();
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

    // Team内の2台が登録された時点で、それぞれの入力を初期化する
    private void TryInitialize()
    {
        if (isInitialized)
            return;

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null || session.BindingMode != ControllerBindingMode.FourControllers)
            return;

        if (inputActions == null)
        {
            Debug.LogError("[RematchReadyInput] Input Action Assetが設定されていません。", this);
            return;
        }

        ControllerTeam team = session.GetTeam(teamIndex);

        if (team == null || !team.HasBothControllers)
            return;

        firstControllerActions = CreateControllerActions(team.FirstController, HandleFirstSubmit, HandleFirstCancel, HandleFirstChange, HandleFirstProceed);
        secondControllerActions = CreateControllerActions(team.SecondController, HandleSecondSubmit, HandleSecondCancel, HandleSecondChange, HandleSecondProceed);

        isInitialized = true;
    }

    // Action Assetを複製し、指定されたControllerのDeviceだけを受け付けるように制限する
    private ControllerActions CreateControllerActions(RegisteredController controller, Action<InputAction.CallbackContext> submitHandler, Action<InputAction.CallbackContext> cancelHandler, Action<InputAction.CallbackContext> changeHandler, Action<InputAction.CallbackContext> proceedHandler)
    {
        InputActionAsset asset = Instantiate(inputActions);
        asset.devices = new[] { controller.Device };

        InputAction submitAction = asset.FindAction(submitActionName, true);
        InputAction cancelAction = asset.FindAction(cancelActionName, true);
        InputAction changeAction = asset.FindAction(changeActionName, true);
        InputAction proceedAction = asset.FindAction(proceedActionName, true);

        submitAction.performed += submitHandler;
        cancelAction.performed += cancelHandler;
        changeAction.performed += changeHandler;
        proceedAction.performed += proceedHandler;

        asset.Enable();

        return new ControllerActions(asset, submitAction, cancelAction, changeAction, proceedAction);
    }

    private void HandleFirstSubmit(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.SetPlayerReady(GetFirstControllerNumber(), context.control.device);
    }

    private void HandleFirstCancel(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.CancelPlayerReady(GetFirstControllerNumber(), context.control.device);
    }

    private void HandleFirstChange(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.ChangeTeamRoles(teamIndex, context.control.device);
    }

    private void HandleFirstProceed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.ProceedByButton(context.control.device);
    }

    private void HandleSecondSubmit(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.SetPlayerReady(GetSecondControllerNumber(), context.control.device);
    }

    private void HandleSecondCancel(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.CancelPlayerReady(GetSecondControllerNumber(), context.control.device);
    }

    private void HandleSecondChange(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.ChangeTeamRoles(teamIndex, context.control.device);
    }

    private void HandleSecondProceed(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        RematchReadyManager.Instance?.ProceedByButton(context.control.device);
    }

    private int GetFirstControllerNumber()
    {
        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);

        return team?.FirstController?.ControllerNumber ?? -1;
    }

    private int GetSecondControllerNumber()
    {
        ControllerTeam team = ControllerSessionManager.Instance?.GetTeam(teamIndex);

        return team?.SecondController?.ControllerNumber ?? -1;
    }

    // 複製したActionの購読を解除してAssetを破棄する
    private void ReleaseControllerActions(ControllerActions actions, Action<InputAction.CallbackContext> submitHandler, Action<InputAction.CallbackContext> cancelHandler, Action<InputAction.CallbackContext> changeHandler, Action<InputAction.CallbackContext> proceedHandler)
    {
        if (actions == null)
            return;

        actions.Submit.performed -= submitHandler;
        actions.Cancel.performed -= cancelHandler;
        actions.Change.performed -= changeHandler;
        actions.Proceed.performed -= proceedHandler;

        actions.Asset.Disable();
        Destroy(actions.Asset);
    }

    private void ReleaseActions()
    {
        ReleaseControllerActions(firstControllerActions, HandleFirstSubmit, HandleFirstCancel, HandleFirstChange, HandleFirstProceed);
        ReleaseControllerActions(secondControllerActions, HandleSecondSubmit, HandleSecondCancel, HandleSecondChange, HandleSecondProceed);

        firstControllerActions = null;
        secondControllerActions = null;
        isInitialized = false;
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

// TwoControllersTest‚ÅŠeController‚ÌReadyECancel“ü—Í‚ğŠÇ—‚·‚é
public sealed class TwoControllerReadyInput : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField]
    private InputActionAsset inputActions;

    [SerializeField]
    private string submitActionName = "ControllerSelection/Submit";

    [SerializeField]
    private string cancelActionName = "ControllerSelection/Cancel";

    private InputActionAsset team1Actions;
    private InputActionAsset team2Actions;

    private InputAction team1SubmitAction;
    private InputAction team1CancelAction;
    private InputAction team2SubmitAction;
    private InputAction team2CancelAction;

    private void Start()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[TwoControllerReadyInput] ControllerSessionManager‚ª‘¶İ‚µ‚Ü‚¹‚ñB", this);
            return;
        }

        if (session.BindingMode != ControllerBindingMode.TwoControllersTest)
            return;

        if (inputActions == null)
        {
            Debug.LogError("[TwoControllerReadyInput] Input Action Asset‚ªİ’è‚³‚ê‚Ä‚¢‚Ü‚¹‚ñB", this);
            return;
        }

        // BindScene‚ğ’¼Ú‹N“®‚µ‚½ê‡‚ÍController“o˜^Š®—¹Œã‚ÉÄ‰Šú‰»‚·‚é
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

    // “o˜^Ï‚İ‚É‚È‚Á‚½Controller‚©‚ç‡”Ô‚É“ü—Í‚ğ‰Šú‰»‚·‚é
    private void TryInitialize()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return;

        if (team1Actions == null)
        {
            RegisteredController controller1 = session.GetRegisteredController(1);

            if (controller1 != null)
                InitializeTeam1(session);
        }

        if (team2Actions == null)
        {
            RegisteredController controller2 = session.GetRegisteredController(2);

            if (controller2 != null)
                InitializeTeam2(session);
        }
    }

    private void HandleControllerRegistered(int teamIndex, RegisteredController controller)
    {
        TryInitialize();
    }

    // Controller 1‚ÌDevice‚¾‚¯‚ğó‚¯•t‚¯‚éInput Action‚ğì¬‚·‚é
    private void InitializeTeam1(ControllerSessionManager session)
    {
        RegisteredController controller = session.GetRegisteredController(1);

        if (controller == null)
            return;

        team1Actions = Instantiate(inputActions);
        team1Actions.devices = new[] { controller.Device };

        team1SubmitAction = team1Actions.FindAction(submitActionName, true);
        team1CancelAction = team1Actions.FindAction(cancelActionName, true);

        team1SubmitAction.performed += HandleTeam1Submit;
        team1CancelAction.performed += HandleTeam1Cancel;

        team1Actions.Enable();
    }

    // Controller 2‚ÌDevice‚¾‚¯‚ğó‚¯•t‚¯‚éInput Action‚ğì¬‚·‚é
    private void InitializeTeam2(ControllerSessionManager session)
    {
        RegisteredController controller = session.GetRegisteredController(2);

        if (controller == null)
            return;

        team2Actions = Instantiate(inputActions);
        team2Actions.devices = new[] { controller.Device };

        team2SubmitAction = team2Actions.FindAction(submitActionName, true);
        team2CancelAction = team2Actions.FindAction(cancelActionName, true);

        team2SubmitAction.performed += HandleTeam2Submit;
        team2CancelAction.performed += HandleTeam2Cancel;

        team2Actions.Enable();
    }

    private void HandleTeam1Submit(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        ControllerSessionManager.Instance?.SetControllerReady(1, context.control.device);
    }

    private void HandleTeam1Cancel(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        ControllerSessionManager.Instance?.CancelControllerReady(1, context.control.device);
    }

    private void HandleTeam2Submit(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        ControllerSessionManager.Instance?.SetControllerReady(2, context.control.device);
    }

    private void HandleTeam2Cancel(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        ControllerSessionManager.Instance?.CancelControllerReady(2, context.control.device);
    }

    // Action‚Ìw“Ç‚ğ‰ğœ‚µAController‚²‚Æ‚É•¡»‚µ‚½Asset‚ğ”jŠü‚·‚é
    private void ReleaseActions()
    {
        if (team1SubmitAction != null)
            team1SubmitAction.performed -= HandleTeam1Submit;

        if (team1CancelAction != null)
            team1CancelAction.performed -= HandleTeam1Cancel;

        if (team2SubmitAction != null)
            team2SubmitAction.performed -= HandleTeam2Submit;

        if (team2CancelAction != null)
            team2CancelAction.performed -= HandleTeam2Cancel;

        if (team1Actions != null)
        {
            team1Actions.Disable();
            Destroy(team1Actions);
        }

        if (team2Actions != null)
        {
            team2Actions.Disable();
            Destroy(team2Actions);
        }
    }
}
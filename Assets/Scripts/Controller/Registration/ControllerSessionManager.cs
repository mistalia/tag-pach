using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Controllerの自動登録・番号付与とTeam構成を管理する
public sealed class ControllerSessionManager : MonoBehaviour
{
    public static ControllerSessionManager Instance { get; private set; }

    public const int TeamCount              = 2;
    public const int ControllerCountPerTeam = 2;
    public const int MaxControllerCount     = TeamCount * ControllerCountPerTeam;

    // TODO: 正式なTitleSceneからモード指定されるようになったら、Inspector fallbackを削除してTitleScene指定へ一本化
    [Header("Binding Mode")]
    [SerializeField]
    private ControllerBindingMode bindingMode = ControllerBindingMode.FourControllers;

    private ControllerBindingMode? requestedBindingMode;

    public ControllerBindingMode BindingMode => requestedBindingMode ?? bindingMode;

    [Header("Registration Settings")]
    [SerializeField]
    private ControllerRegistrationSettings registrationSettings;

    private readonly ControllerTeam[] teams =
    {
        new ControllerTeam(),
        new ControllerTeam()
    };

    public event Action<int, RegisteredController>  ControllerRegistered;
    public event Action<int, int, ControllerRole>   ControllerRoleChanged;
    public event Action<int, int, bool>             ControllerReadyChanged;
    public event Action<int, bool, bool, bool>      TeamStatusChanged;
    public event Action                             RegistrationCleared;

    public int RegisteredControllerCount { get; private set; }

    public int RequiredControllerCount => BindingMode == ControllerBindingMode.TwoControllersTest ? TeamCount : MaxControllerCount;

    public bool IsFull => RegisteredControllerCount >= RequiredControllerCount;

    public bool AreAllTeamsComplete
    {
        get
        {
            if (BindingMode == ControllerBindingMode.TwoControllersTest)
            {
                return teams[0].FirstControllerSelection != null &&
                       teams[0].FirstControllerSelection.IsReady &&
                       teams[1].FirstControllerSelection != null &&
                       teams[1].FirstControllerSelection.IsReady;
            }

            return teams[0].CanDeploy && teams[1].CanDeploy;
        }
    }

    public bool UsesSplitKeyboard =>    BindingMode == ControllerBindingMode.TwoControllersTest && registrationSettings != null 
                                    &&  registrationSettings.KeyboardMode == KeyboardRegistrationMode.SplitDebug;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AutoRegisterConnectedDevices();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public int GetTeamIndexForController(int controllerNumber)
    {
        if (controllerNumber < 1 || controllerNumber > RequiredControllerCount)
            return -1;

        if (BindingMode == ControllerBindingMode.TwoControllersTest)
            return controllerNumber - 1;

        return (controllerNumber - 1) % TeamCount;
    }

    public ControllerRoleSelection GetControllerSelection(int controllerNumber)
    {
        int teamIndex = GetTeamIndexForController(controllerNumber);
        ControllerTeam team = GetTeam(teamIndex);

        return team?.GetSelection(controllerNumber);
    }

    public bool ToggleControllerRole(int controllerNumber, InputDevice requestingDevice)
    {
        int teamIndex = GetTeamIndexForController(controllerNumber);
        ControllerTeam team = GetTeam(teamIndex);
        ControllerRoleSelection selection = team?.GetSelection(controllerNumber);

        if (!CanControllerOperate(selection, requestingDevice))
            return false;

        if (!selection.ToggleRole())
            return false;

        RefreshTeamState(teamIndex);

        ControllerRoleChanged?.Invoke(teamIndex, controllerNumber, selection.SelectedRole);

        return true;
    }

    public bool SetControllerReady(int controllerNumber, InputDevice requestingDevice)
    {
        int teamIndex = GetTeamIndexForController(controllerNumber);
        ControllerTeam team = GetTeam(teamIndex);
        ControllerRoleSelection selection = team?.GetSelection(controllerNumber);

        if (!CanControllerOperate(selection, requestingDevice))
            return false;

        if (!selection.SetReady())
            return false;

        RefreshTeamState(teamIndex);

        ControllerReadyChanged?.Invoke(teamIndex, controllerNumber, true);

        return true;
    }

    public bool CancelControllerReady(int controllerNumber, InputDevice requestingDevice)
    {
        int teamIndex = GetTeamIndexForController(controllerNumber);
        ControllerTeam team = GetTeam(teamIndex);
        ControllerRoleSelection selection = team?.GetSelection(controllerNumber);

        if (!CanControllerOperate(selection, requestingDevice))
            return false;

        if (!selection.CancelReady())
            return false;

        RefreshTeamState(teamIndex);

        ControllerReadyChanged?.Invoke(teamIndex, controllerNumber, false);

        return true;
    }

    private void RefreshTeamState(int teamIndex)
    {
        ControllerTeam team = GetTeam(teamIndex);

        if (team == null)
            return;

        if (BindingMode == ControllerBindingMode.TwoControllersTest)
        {
            bool isReady = team.FirstControllerSelection != null && team.FirstControllerSelection.IsReady;

            TeamStatusChanged?.Invoke(teamIndex, isReady, true, isReady);

            return;
        }

        team.UpdateAssignments();

        TeamStatusChanged?.Invoke(teamIndex, team.AreAllReady, team.AreRolesValid, team.CanDeploy);
    }

    private static bool CanControllerOperate(ControllerRoleSelection selection, InputDevice requestingDevice)
    {
        if (selection == null || requestingDevice == null)
            return false;

        return selection.Controller.Device == requestingDevice;
    }

    // 現在接続されている対象DeviceへController番号を自動で割り当てる
    public void AutoRegisterConnectedDevices()
    {
        ClearRegistration();

        if (registrationSettings == null)
        {
            Debug.LogError("[ControllerSessionManager] ControllerRegistrationSettingsが設定されていません。", this);
            return;
        }

        if (UsesSplitKeyboard)
        {
            RegisterSplitKeyboard();
            return;
        }

        foreach (InputDevice device in InputSystem.devices)
        {
            if (IsFull)
                break;

            if (!IsRegistrationTarget(device))
                continue;

            RegisterDevice(device);
        }
    }

    private void RegisterSplitKeyboard()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            Debug.LogError("[ControllerSessionManager] Split Keyboard Debugに使用できるKeyboardがありません。", this);
            return;
        }

        RegisterSplitKeyboardController(0, keyboard);
        RegisterSplitKeyboardController(1, keyboard);
    }

    private void RegisterSplitKeyboardController(int teamIndex, Keyboard keyboard)
    {
        int controllerNumber = teamIndex + 1;
        RegisteredController controller = new RegisteredController(controllerNumber, keyboard);
        ControllerTeam team = teams[teamIndex];

        if (!team.TryAddController(controller))
            return;

        if (!team.ConfigureSingleControllerMode())
        {
            team.Reset();
            return;
        }

        RegisteredControllerCount++;

        ControllerRegistered?.Invoke(teamIndex, controller);
    }

    public ControllerTeam GetTeam(int teamIndex)
    {
        if (!IsValidTeamIndex(teamIndex))
            return null;

        return teams[teamIndex];
    }

    public RegisteredController GetRegisteredController(int controllerNumber)
    {
        if (controllerNumber < 1 || controllerNumber > RequiredControllerCount)
            return null;

        if (BindingMode == ControllerBindingMode.TwoControllersTest)
            return teams[controllerNumber - 1].FirstController;

        int teamIndex = (controllerNumber - 1) % TeamCount;

        ControllerTeam team = teams[teamIndex];

        if (controllerNumber <= TeamCount)
            return team.FirstController;

        return team.SecondController;
    }


    public InputDevice GetDevice(int teamIndex, ControllerRole role)
    {
        ControllerTeam team = GetTeam(teamIndex);

        if (team == null)
            return null;

        return team.GetDevice(role);
    }

    public bool IsRegistered(InputDevice device)
    {
        if (device == null)
            return false;

        for (int controllerNumber = 1; controllerNumber <= MaxControllerCount; controllerNumber++)
        {
            RegisteredController controller = GetRegisteredController(controllerNumber);

            if (controller?.Device == device)
                return true;
        }

        return false;
    }

    public void ClearRegistration()
    {
        foreach (ControllerTeam team in teams)
        {
            team.Reset();
        }

        RegisteredControllerCount = 0;

        RegistrationCleared?.Invoke();
    }

    private bool RegisterDevice(InputDevice device)
    {
        if (device == null || IsRegistered(device) || IsFull)
            return false;

        if (BindingMode == ControllerBindingMode.TwoControllersTest)
            return RegisterTwoControllersTestDevice(device);

        return RegisterFourControllersDevice(device);
    }

    private bool RegisterFourControllersDevice(InputDevice device)
    {
        int controllerNumber = RegisteredControllerCount + 1;
        int teamIndex = (controllerNumber - 1) % TeamCount;

        RegisteredController controller = new RegisteredController(controllerNumber, device);

        if (!teams[teamIndex].TryAddController(controller))
            return false;

        RegisteredControllerCount++;

        ControllerRegistered?.Invoke(teamIndex, controller);

        return true;
    }

    private bool RegisterTwoControllersTestDevice(InputDevice device)
    {
        int controllerNumber = RegisteredControllerCount + 1;
        int teamIndex = RegisteredControllerCount;

        RegisteredController controller = new RegisteredController(controllerNumber, device);
        ControllerTeam team = teams[teamIndex];

        if (!team.TryAddController(controller))
            return false;

        if (!team.ConfigureSingleControllerMode())
        {
            team.Reset();
            return false;
        }

        RegisteredControllerCount++;

        // 登録デバックログ箇所

        ControllerRegistered?.Invoke(teamIndex, controller);

        return true;
    }

    private bool IsRegistrationTarget(InputDevice device)
    {
        if (device == null || device is Mouse)
            return false;

        if (device is Keyboard)
            return registrationSettings.KeyboardMode == KeyboardRegistrationMode.SingleController;

        return device is Gamepad || device is Joystick;
    }

    private static bool IsValidTeamIndex(int teamIndex)
    {
        return teamIndex >= 0 && teamIndex < TeamCount;
    }

    // 再戦準備画面で確定したTeam内Roleを反映する
    public bool ApplyRematchRoles(int teamIndex, ControllerRole firstControllerRole, ControllerRole secondControllerRole)
    {
        ControllerTeam team = GetTeam(teamIndex);

        if (team == null)
            return false;

        if (!team.ApplyConfirmedRoles(firstControllerRole, secondControllerRole))
            return false;

        ControllerRoleChanged?.Invoke(teamIndex, team.FirstController.ControllerNumber, firstControllerRole);
        ControllerRoleChanged?.Invoke(teamIndex, team.SecondController.ControllerNumber, secondControllerRole);
        
        TeamStatusChanged?.Invoke(teamIndex, team.AreAllReady, team.AreRolesValid, team.CanDeploy);

        return true;
    }

    // TitleSceneなどからController構成を指定する
    public void SetBindingMode(ControllerBindingMode mode)
    {
        if (requestedBindingMode == mode)
            return;

        requestedBindingMode = mode;

        AutoRegisterConnectedDevices();
    }


    // 外部指定を解除し、Inspector設定へ戻す
    public void ClearRequestedBindingMode()
    {
        if (!requestedBindingMode.HasValue)
            return;

        requestedBindingMode = null;

        AutoRegisterConnectedDevices();
    }

    // 現在選択されているRoleを全TeamのAssignmentへ確定反映する
    public bool CommitCurrentRoleAssignments()
    {
        if (BindingMode == ControllerBindingMode.TwoControllersTest)
            return true;

        for (int teamIndex = 0; teamIndex < TeamCount; teamIndex++)
        {
            ControllerTeam team = teams[teamIndex];

            if (team == null || !team.CanDeploy)
                return false;

            if (!team.UpdateAssignments())
                return false;
        }

        return true;
    }

    public void ResetAllReadyStates()
    {
        for (int teamIndex = 0; teamIndex < TeamCount; teamIndex++)
        {
            ControllerTeam team = teams[teamIndex];

            if (team == null)
                continue;

            team.ResetReady();

            if (BindingMode == ControllerBindingMode.TwoControllersTest)
            {
                ControllerRoleSelection selection = team.FirstControllerSelection;

                if (selection != null)
                    ControllerReadyChanged?.Invoke(teamIndex, selection.Controller.ControllerNumber, false);

                TeamStatusChanged?.Invoke(teamIndex, false, true, false);

                continue;
            }

            if (team.FirstControllerSelection != null)
                ControllerReadyChanged?.Invoke(teamIndex, team.FirstController.ControllerNumber, false);

            if (team.SecondControllerSelection != null)
                ControllerReadyChanged?.Invoke(teamIndex, team.SecondController.ControllerNumber, false);

            TeamStatusChanged?.Invoke(teamIndex, false, team.AreRolesValid, false);
        }
    }
}
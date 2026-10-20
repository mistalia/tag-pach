using System;
using UnityEngine;
using UnityEngine.InputSystem;

// バインド画面で登録された入力デバイスを、コントローラーチームとして管理
public sealed class ControllerSessionManager : MonoBehaviour
{
    public static ControllerSessionManager Instance { get; private set; }

    // チーム数
    public const int TeamCount                  = 2;
    // 1チームのロール数
    public const int ControllerCountPerTeam     = 2;
    // 最大コントローラー数
    public const int MaxControllerCount         = TeamCount * ControllerCountPerTeam;

    [SerializeField]
    private ControllerBindingMode bindingMode   = ControllerBindingMode.FourControllers;

    private readonly ControllerTeam[] teams =
    {
        new ControllerTeam(),
        new ControllerTeam()
    };

    // Team・Role・Deviceの組み合わせを通知する
    public event Action<int, ControllerRole, InputDevice> AssignmentRegistered;

    public event Action RegistrationCleared;

    public ControllerBindingMode BindingMode => bindingMode;

    public int RegisteredControllerCount { get; private set; }

    // 現在のモードで必要な物理コントローラー数
    public int RequiredControllerCount => bindingMode == ControllerBindingMode.TwoControllersTest ? TeamCount : MaxControllerCount;

    public bool IsFull => RegisteredControllerCount >= RequiredControllerCount;

    // 両チームにOperationとPlacementが登録されているか
    public bool AreAllTeamsComplete => teams[0].IsComplete && teams[1].IsComplete;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // シーン遷移後も登録したチーム情報を保持
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool TryRegisterDevice(InputDevice device)
    {
        // 登録可能かどうか
        if (!CanRegisterDevice(device))
            return false;

        // 登録処理
        if (bindingMode == ControllerBindingMode.TwoControllersTest)
            return RegisterTestController(device);

        return RegisterFourControllerMode(device);
    }

    // 4台バージョン
    private bool RegisterFourControllerMode(InputDevice device)
    {
        int registrationIndex = RegisteredControllerCount;

        int teamIndex = registrationIndex / ControllerCountPerTeam;

        ControllerRole role = GetRoleFromRegistrationIndex(registrationIndex);

        ControllerTeam team = teams[teamIndex];

        if (!team.TryAssign(device, role))
        {
            Debug.LogError(
                $"[ControllerSessionManager] " +
                $"Team {teamIndex + 1}の{role}担当へ" +
                "デバイスを登録できませんでした。",
                this
            );

            return false;
        }

        RegisteredControllerCount++;

        NotifyAssignmentRegistered(teamIndex, role, device);

        return true;
    }

    // 2台バージョン
    private bool RegisterTestController(InputDevice device)
    {
        int teamIndex = RegisteredControllerCount;

        ControllerTeam team = teams[teamIndex];

        bool operationAssigned = team.TryAssign(device, ControllerRole.Operation);
        bool placementAssigned = team.TryAssign(device, ControllerRole.Placement);

        if (!operationAssigned || !placementAssigned)
        {
            Debug.LogError(
                $"[ControllerSessionManager] " +
                $"Team {teamIndex + 1}へ" +
                "テスト用デバイスを登録できませんでした。",
                this
            );

            return false;
        }

        RegisteredControllerCount++;

        // OperationとPlacementの両方を登録済みとして通知
        NotifyAssignmentRegistered(teamIndex, ControllerRole.Operation, device);
        NotifyAssignmentRegistered(teamIndex, ControllerRole.Placement, device);

        return true;
    }

    public bool IsRegistered(InputDevice device)
    {
        if (device == null)
            return false;

        for (int teamIndex = 0; teamIndex < teams.Length; teamIndex++)
        {
            ControllerTeam team =teams[teamIndex];

            if (team.GetDevice(ControllerRole.Operation) == device)
                return true;

            if (team.GetDevice(ControllerRole.Placement) == device)
                return true;
        }

        return false;
    }

    public ControllerTeam GetTeam(int teamIndex)
    {
        if (!IsValidTeamIndex(teamIndex))
            return null;

        return teams[teamIndex];
    }

    public InputDevice GetDevice(int teamIndex, ControllerRole role)
    {
        ControllerTeam team = GetTeam(teamIndex);

        if (team == null)
            return null;

        return team.GetDevice(role);
    }

    public void ClearRegistration()
    {
        if (RegisteredControllerCount == 0)
            return;

        for (int teamIndex = 0; teamIndex < teams.Length; teamIndex++)
        {
            teams[teamIndex] = new ControllerTeam();
        }

        RegisteredControllerCount = 0;

        RegistrationCleared?.Invoke();
    }

    private bool CanRegisterDevice(InputDevice device)
    {
        if (device == null)
        {
            Debug.LogWarning(
                "[ControllerSessionManager] " +
                "nullの入力デバイスは登録できません。",
                this
            );

            return false;
        }

        // 同じ物理コントローラーを複数のTeamへ登録しない
        if (IsRegistered(device))
            return false;

        if (IsFull)
            return false;

        return true;
    }

    private void NotifyAssignmentRegistered(int teamIndex, ControllerRole role, InputDevice device)
    {
        AssignmentRegistered?.Invoke(teamIndex, role, device);
    }

    private static ControllerRole GetRoleFromRegistrationIndex(int registrationIndex)
    {
        bool isOperationSlot = registrationIndex % ControllerCountPerTeam == 0;

        return isOperationSlot ? ControllerRole.Operation : ControllerRole.Placement;
    }

    private static bool IsValidTeamIndex(int teamIndex)
    {
        return teamIndex >= 0 && teamIndex < TeamCount;
    }
}
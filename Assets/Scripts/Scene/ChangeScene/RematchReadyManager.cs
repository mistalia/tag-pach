using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 再戦準備画面全体のRole変更・Ready状態・時間制限・最終確認を管理する
public sealed class RematchReadyManager : MonoBehaviour
{
    public static RematchReadyManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField]
    private RematchReadySettings settings;

    private readonly RematchTeamReadyState[] teamStates = new RematchTeamReadyState[ControllerSessionManager.TeamCount];

    public event Action<int, int, ControllerRole> RoleChanged;
    public event Action<int, int, bool> ReadyChanged;
    public event Action<int> TeamChanged;
    public event Action AllPlayersReady;
    public event Action<float> RemainingTimeChanged;
    public event Action TransitionRequested;
    public event Action<RematchReadyPhase> PhaseChanged;

    private float remainingTime;
    private float finalConfirmationRemainingTime;
    private bool isReadyPhaseActive;

    public float RemainingTime => remainingTime;
    public float FinalConfirmationRemainingTime => finalConfirmationRemainingTime;
    public RematchReadyPhase CurrentPhase { get; private set; } = RematchReadyPhase.WaitingForPlayers;
    public bool IsAllPlayersReady => teamStates[0] != null && teamStates[1] != null && teamStates[0].AreAllReady && teamStates[1].AreAllReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Viewより先にTeam状態を生成できるようAwake時点で初期化する
        Initialize();
    }

    private void Update()
    {
        switch (CurrentPhase)
        {
            case RematchReadyPhase.WaitingForPlayers:
                UpdateReadyPhase();
                break;

            case RematchReadyPhase.FinalConfirmation:
                UpdateFinalConfirmation();
                break;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ControllerSessionManagerの確定Roleを引き継いで再戦準備状態を作成する
    private void Initialize()
    {
        if (settings == null || !settings.IsValid)
        {
            Debug.LogError("[RematchReadyManager] RematchReadySettingsが正しく設定されていません。", this);
            enabled = false;
            return;
        }

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError("[RematchReadyManager] ControllerSessionManagerが存在しません。", this);
            enabled = false;
            return;
        }

        if (session.BindingMode != ControllerBindingMode.FourControllers)
        {
            Debug.LogWarning("[RematchReadyManager] 現在はFourControllersモードのみ再戦準備画面に対応しています。", this);
            enabled = false;
            return;
        }

        for (int teamIndex = 0; teamIndex < ControllerSessionManager.TeamCount; teamIndex++)
        {
            if (!TryCreateTeamState(session, teamIndex))
            {
                Debug.LogError($"[RematchReadyManager] Team {teamIndex + 1}の再戦準備状態を作成できませんでした。", this);
                enabled = false;
                return;
            }
        }

        remainingTime = settings.TimeLimit;
        isReadyPhaseActive = true;
        CurrentPhase = RematchReadyPhase.WaitingForPlayers;
    }

    // 準備中は残り時間を減らし、時間切れ時に未回答Playerを自動Readyにする
    private void UpdateReadyPhase()
    {
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0.0f, remainingTime);

        RemainingTimeChanged?.Invoke(remainingTime);

        if (remainingTime <= 0.0f)
            HandleTimeLimitReached();
    }

    // Automaticの場合のみ最終確認の表示時間終了後に遷移を要求する
    private void UpdateFinalConfirmation()
    {
        if (settings.TransitionMode != RematchTransitionMode.Automatic)
            return;

        finalConfirmationRemainingTime -= Time.deltaTime;

        if (finalConfirmationRemainingTime <= 0.0f)
            RequestTransition();
    }

    // 時間切れ時は未回答Playerのみ現在RoleのままReadyにする
    private void HandleTimeLimitReached()
    {
        if (!isReadyPhaseActive)
            return;

        for (int teamIndex = 0; teamIndex < teamStates.Length; teamIndex++)
        {
            RematchTeamReadyState teamState = teamStates[teamIndex];

            if (teamState == null)
                continue;

            ForcePlayerReady(teamIndex, teamState.FirstPlayerReadyState);
            ForcePlayerReady(teamIndex, teamState.SecondPlayerReadyState);
        }

        CheckAllPlayersReady();
    }

    private void ForcePlayerReady(int teamIndex, RematchPlayerReadyState readyState)
    {
        if (readyState == null || readyState.IsReady)
            return;

        readyState.ForceReady();
        ReadyChanged?.Invoke(teamIndex, readyState.ControllerNumber, true);
    }

    // 前回確定したAssignmentからControllerごとのRoleを復元する
    private bool TryCreateTeamState(ControllerSessionManager session, int teamIndex)
    {
        ControllerTeam team = session.GetTeam(teamIndex);

        if (team == null || team.FirstController == null || team.SecondController == null)
            return false;

        if (!TryGetCurrentRole(session, teamIndex, team.FirstController.Device, out ControllerRole firstRole))
            return false;

        if (!TryGetCurrentRole(session, teamIndex, team.SecondController.Device, out ControllerRole secondRole))
            return false;

        teamStates[teamIndex] = new RematchTeamReadyState(teamIndex, team.FirstController.ControllerNumber, team.SecondController.ControllerNumber, firstRole, secondRole);

        return true;
    }

    private bool TryGetCurrentRole(ControllerSessionManager session, int teamIndex, InputDevice device, out ControllerRole role)
    {
        role = ControllerRole.Operation;

        InputDevice operationDevice = session.GetDevice(teamIndex, ControllerRole.Operation);

        if (operationDevice == device)
        {
            role = ControllerRole.Operation;
            return true;
        }

        InputDevice placementDevice = session.GetDevice(teamIndex, ControllerRole.Placement);

        if (placementDevice == device)
        {
            role = ControllerRole.Placement;
            return true;
        }

        return false;
    }

    public RematchTeamReadyState GetTeamState(int teamIndex)
    {
        if (teamIndex < 0 || teamIndex >= teamStates.Length)
            return null;

        return teamStates[teamIndex];
    }

    public bool SetPlayerReady(int controllerNumber, InputDevice requestingDevice)
    {
        if (CurrentPhase != RematchReadyPhase.WaitingForPlayers)
            return false;

        if (!TryGetPlayerState(controllerNumber, requestingDevice, out int teamIndex, out RematchPlayerReadyState readyState))
            return false;

        if (!readyState.SetReady())
            return false;

        ReadyChanged?.Invoke(teamIndex, controllerNumber, true);
        CheckAllPlayersReady();

        return true;
    }

    // 最終確認中でも残り時間があればCancelして準備状態へ戻れる
    public bool CancelPlayerReady(int controllerNumber, InputDevice requestingDevice)
    {
        if (CurrentPhase == RematchReadyPhase.FinalConfirmation)
        {
            if (remainingTime <= 0.0f)
                return false;

            ReturnToWaitingForPlayers();
        }
        else if (CurrentPhase != RematchReadyPhase.WaitingForPlayers)
        {
            return false;
        }

        if (!TryGetPlayerState(controllerNumber, requestingDevice, out int teamIndex, out RematchPlayerReadyState readyState))
            return false;

        if (!readyState.CancelReady())
            return false;

        ReadyChanged?.Invoke(teamIndex, controllerNumber, false);

        return true;
    }

    // 最終確認を解除し、残っている準備時間から再開する
    private void ReturnToWaitingForPlayers()
    {
        if (CurrentPhase != RematchReadyPhase.FinalConfirmation)
            return;

        CurrentPhase = RematchReadyPhase.WaitingForPlayers;
        isReadyPhaseActive = true;
        finalConfirmationRemainingTime = 0.0f;

        PhaseChanged?.Invoke(CurrentPhase);
    }

    // Team内のどちらのControllerからでもRoleを交換でき、変更時はTeam内のReadyを解除する
    public bool ChangeTeamRoles(int teamIndex, InputDevice requestingDevice)
    {
        if (CurrentPhase != RematchReadyPhase.WaitingForPlayers)
            return false;

        RematchTeamReadyState teamState = GetTeamState(teamIndex);

        if (teamState == null || requestingDevice == null)
            return false;

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return false;

        RegisteredController firstController = session.GetRegisteredController(teamState.FirstControllerNumber);
        RegisteredController secondController = session.GetRegisteredController(teamState.SecondControllerNumber);
        bool isTeamController = firstController?.Device == requestingDevice || secondController?.Device == requestingDevice;

        if (!isTeamController)
            return false;

        teamState.ChangeRoles();

        RoleChanged?.Invoke(teamIndex, teamState.FirstControllerNumber, teamState.FirstControllerRole);
        RoleChanged?.Invoke(teamIndex, teamState.SecondControllerNumber, teamState.SecondControllerRole);
        ReadyChanged?.Invoke(teamIndex, teamState.FirstControllerNumber, false);
        ReadyChanged?.Invoke(teamIndex, teamState.SecondControllerNumber, false);
        TeamChanged?.Invoke(teamIndex);

        return true;
    }

    // Controller番号と実際に入力したDeviceが一致するPlayer状態だけを取得する
    private bool TryGetPlayerState(int controllerNumber, InputDevice requestingDevice, out int teamIndex, out RematchPlayerReadyState readyState)
    {
        teamIndex = -1;
        readyState = null;

        if (requestingDevice == null)
            return false;

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return false;

        RegisteredController controller = session.GetRegisteredController(controllerNumber);

        if (controller == null || controller.Device != requestingDevice)
            return false;

        teamIndex = session.GetTeamIndexForController(controllerNumber);

        RematchTeamReadyState teamState = GetTeamState(teamIndex);

        if (teamState == null)
            return false;

        readyState = teamState.GetReadyState(controllerNumber);

        return readyState != null;
    }

    private void CheckAllPlayersReady()
    {
        if (!IsAllPlayersReady)
            return;

        EnterFinalConfirmation();
    }

    // 全員Readyになった時点で通常操作を停止し、最終確認状態へ移行する
    private void EnterFinalConfirmation()
    {
        if (CurrentPhase != RematchReadyPhase.WaitingForPlayers)
            return;

        isReadyPhaseActive = false;
        CurrentPhase = RematchReadyPhase.FinalConfirmation;
        finalConfirmationRemainingTime = settings.AutomaticTransitionDelay;

        AllPlayersReady?.Invoke();
        PhaseChanged?.Invoke(CurrentPhase);
    }

    // Button方式では登録済みControllerの入力だけを最終遷移として受け付ける
    public bool ProceedByButton(InputDevice requestingDevice)
    {
        if (settings.TransitionMode != RematchTransitionMode.Button)
            return false;

        if (CurrentPhase != RematchReadyPhase.FinalConfirmation)
            return false;

        if (!IsRegisteredController(requestingDevice))
            return false;

        RequestTransition();

        return true;
    }

    private bool IsRegisteredController(InputDevice device)
    {
        if (device == null)
            return false;

        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return false;

        for (int controllerNumber = 1; controllerNumber <= ControllerSessionManager.MaxControllerCount; controllerNumber++)
        {
            RegisteredController controller = session.GetRegisteredController(controllerNumber);

            if (controller?.Device == device)
                return true;
        }

        return false;
    }

    // ChangeSceneで確定したRoleをSessionへ反映してからScene遷移を要求する
    private void RequestTransition()
    {
        if (CurrentPhase != RematchReadyPhase.FinalConfirmation)
            return;

        if (!ApplyConfirmedRoles())
        {
            Debug.LogError("[RematchReadyManager] 再戦用RoleをControllerSessionManagerへ反映できませんでした。", this);
            return;
        }

        CurrentPhase = RematchReadyPhase.Transitioning;

        PhaseChanged?.Invoke(CurrentPhase);
        TransitionRequested?.Invoke();
    }

    // 再戦準備中の仮Roleを全Teamの確定RoleとしてControllerSessionManagerへ反映する
    private bool ApplyConfirmedRoles()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
            return false;

        for (int teamIndex = 0; teamIndex < teamStates.Length; teamIndex++)
        {
            RematchTeamReadyState teamState = teamStates[teamIndex];

            if (teamState == null)
                return false;

            if (!session.ApplyRematchRoles(teamIndex, teamState.FirstControllerRole, teamState.SecondControllerRole))
                return false;
        }

        return true;
    }
}

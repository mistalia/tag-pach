using UnityEngine;
using UnityEngine.InputSystem;

// バインド画面で登録されたControllerTeamを役割別のPlayerInputへ割り当て
public sealed class PlayerSetup : MonoBehaviour
{
    [SerializeField]
    private string operationActionMapName       = "Operation";

    [SerializeField]
    private string placementActionMapName       = "Placement";

    [SerializeField]
    private string gamepadControlSchemeName     = "Gamepad";

    [SerializeField]
    private string joystickControlSchemeName    = "Joystick";

    [SerializeField]
    private string keyboardControlSchemeName    = "Keyboard&Mouse";

    [SerializeField]
    private PlayerControllerInputSet[] players = new PlayerControllerInputSet[ControllerSessionManager.TeamCount];

    private void Start()
    {
        SetupPlayers();
    }

    private void OnValidate()
    {
        if (players == null)
            return;

        if (players.Length != ControllerSessionManager.TeamCount)
        {
            Debug.LogWarning(
                $"[PlayerSetup] Playersには" +
                $"{ControllerSessionManager.TeamCount}人分の" +
                "入力設定を登録してください。",
                this
            );
        }
    }

    private void SetupPlayers()
    {
        ControllerSessionManager session = ControllerSessionManager.Instance;

        if (session == null)
        {
            Debug.LogError(
                "[PlayerSetup] " +
                "ControllerSessionManagerが存在しません。",
                this
            );

            DisableAllPlayers();
            return;
        }

        TeamPlayerAssignmentManager assignmentManager = TeamPlayerAssignmentManager.Instance;

        if (assignmentManager == null)
        {
            Debug.LogError(
                "[PlayerSetup] " +
                "TeamPlayerAssignmentManagerが存在しません。",
                this
            );

            DisableAllPlayers();
            return;
        }

        if (!session.AreAllTeamsComplete)
        {
            Debug.LogError(
                "[PlayerSetup] " +
                "ControllerTeamの登録が完了していません。",
                this
            );

            DisableAllPlayers();
            return;
        }

        if (players == null || players.Length != ControllerSessionManager.TeamCount)
        {
            Debug.LogError(
                "[PlayerSetup] " +
                "Playerの入力設定が不足しています。",
                this
            );

            DisableAllPlayers();
            return;
        }

        DisableAllPlayers();

        for (int teamIndex = 0; teamIndex < ControllerSessionManager.TeamCount; teamIndex++)
        {
            int playerIndex = assignmentManager.GetPlayerIndex(teamIndex);

            SetupPlayerForTeam(session, teamIndex, playerIndex);
        }
    }

    private void SetupPlayerForTeam(ControllerSessionManager session, int teamIndex, int playerIndex)
    {
        if (!IsValidPlayerIndex(playerIndex))
        {
            Debug.LogError(
                $"[PlayerSetup] " +
                $"Team {teamIndex + 1}に設定された" +
                $"Player番号が不正です: {playerIndex}",
                this
            );

            return;
        }

        PlayerControllerInputSet playerInputSet = players[playerIndex];

        if (playerInputSet == null)
        {
            Debug.LogError($"[PlayerSetup] Player {playerIndex + 1}の入力設定が存在しません。", this);
            return;
        }

        if (session.BindingMode == ControllerBindingMode.TwoControllersTest)
        {
            SetupTestModePlayer(session, playerInputSet, teamIndex, playerIndex);
            return;
        }

        SetupFourControllersPlayer(session, playerInputSet, teamIndex, playerIndex);
    }

    private void SetupFourControllersPlayer(ControllerSessionManager session, PlayerControllerInputSet playerInputSet, int teamIndex, int playerIndex)
    {
        if (!playerInputSet.IsFourControllersConfigured)
        {
            Debug.LogError($"[PlayerSetup] Player {playerIndex + 1}の4台モード用入力設定が完了していません。", this);
            return;
        }

        InputDevice operationDevice = session.GetDevice(teamIndex, ControllerRole.Operation);
        InputDevice placementDevice = session.GetDevice(teamIndex, ControllerRole.Placement);

        if (operationDevice == null || placementDevice == null)
        {
            Debug.LogError($"[PlayerSetup] Team {teamIndex + 1}のデバイス情報が不足しています。", this);
            return;
        }

        // Player本体を有効化する前に、使用する入力Objectを選択する
        playerInputSet.SetFourControllersInputActive(true);
        playerInputSet.SetPlayerActive(true);

        // Player本体が参照するReaderを通常モード用へ切り替える
        playerInputSet.PlayerInputReader.UseFourControllersMode();

        bool operationSucceeded = SetupRoleInput(playerInputSet.OperationPlayerInput, operationDevice, operationActionMapName, teamIndex, ControllerRole.Operation);
        bool placementSucceeded = SetupRoleInput(playerInputSet.PlacementPlayerInput, placementDevice, placementActionMapName, teamIndex, ControllerRole.Placement);

        if (operationSucceeded && placementSucceeded)
            return;

        playerInputSet.SetFourControllersInputActive(false);
        playerInputSet.SetPlayerActive(false);
    }

    private void SetupTestModePlayer(ControllerSessionManager session, PlayerControllerInputSet playerInputSet, int teamIndex, int playerIndex)
    {
        if (!playerInputSet.IsTestModeConfigured)
        {
            Debug.LogError($"[PlayerSetup] Player {playerIndex + 1}のテストモード用入力設定が完了していません。", this);
            return;
        }

        InputDevice device = session.GetDevice(teamIndex, ControllerRole.Operation);

        if (device == null)
        {
            Debug.LogError($"[PlayerSetup] Team {teamIndex + 1}のテストモード用デバイスが存在しません。", this);
            return;
        }

        string controlSchemeName = GetControlSchemeName(device);

        if (string.IsNullOrEmpty(controlSchemeName))
        {
            Debug.LogError($"[PlayerSetup] {device.displayName}に対応するControl Schemeを判定できません。", this);
            return;
        }

        // Player本体を有効化する前に、TestInputだけを有効化対象にする
        playerInputSet.SetTestInputActive(true);
        playerInputSet.SetPlayerActive(true);

        // Player本体が参照するReaderをTestInput側へ切り替える
        playerInputSet.PlayerInputReader.UseTwoControllersTestMode();

        PlayerInput playerInput = playerInputSet.TestPlayerInput;

        playerInput.enabled = true;
        playerInput.SwitchCurrentControlScheme(controlSchemeName, device);

        InputActionMap operationMap = playerInput.actions.FindActionMap(operationActionMapName, true);
        InputActionMap placementMap = playerInput.actions.FindActionMap(placementActionMapName, true);

        operationMap.Enable();
        placementMap.Enable();

        playerInput.ActivateInput();
    }

    private bool SetupRoleInput(PlayerInput playerInput, InputDevice device, string actionMapName, int teamIndex, ControllerRole role)
    {
        if (playerInput == null)
        {
            Debug.LogError(
                $"[PlayerSetup] " +
                $"Team {teamIndex + 1} / {role}の" +
                "PlayerInputが設定されていません。",
                this
            );

            return false;
        }

        if (device == null)
        {
            Debug.LogError(
                $"[PlayerSetup] " +
                $"Team {teamIndex + 1} / {role}の" +
                "デバイスが存在しません。",
                this
            );

            return false;
        }

        string controlSchemeName = GetControlSchemeName(device);

        if (string.IsNullOrEmpty(controlSchemeName))
        {
            Debug.LogError(
                $"[PlayerSetup] " +
                $"{device.displayName}に対応する" +
                "Control Schemeを判定できません。" +
                $"\nTeam: {teamIndex + 1}" +
                $"\nRole: {role}" +
                $"\nType: {device.GetType().Name}" +
                $"\nLayout: {device.layout}",
                playerInput
            );

            return false;
        }

        // PlayerInputを先に有効な状態にする
        playerInput.enabled = true;

        playerInput.ActivateInput();

        // その後で使用デバイスを設定する
        playerInput.SwitchCurrentControlScheme(controlSchemeName, device);

        // 担当に対応したActionMapへ切り替える
        playerInput.SwitchCurrentActionMap(actionMapName);

        return true;
    }

    private string GetControlSchemeName(InputDevice device)
    {
        if (device is Gamepad)
            return gamepadControlSchemeName;

        if (device is Joystick)
            return joystickControlSchemeName;

        if (device is Keyboard)
            return keyboardControlSchemeName;

        return null;
    }

    private bool IsValidPlayerIndex(int playerIndex)
    {
        return playerIndex >= 0 && playerIndex < players.Length;
    }

    private void DisableAllPlayers()
    {
        if (players == null)
            return;

        foreach (PlayerControllerInputSet playerInputSet in players)
        {
            if (playerInputSet == null)
                continue;

            playerInputSet.SetFourControllersInputActive(false);
            playerInputSet.SetTestInputActive(false);
            playerInputSet.SetPlayerActive(false);
        }
    }
}
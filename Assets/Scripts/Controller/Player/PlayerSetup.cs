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

        if (playerInputSet == null || !playerInputSet.IsConfigured)
        {
            Debug.LogError(
                $"[PlayerSetup] " +
                $"Player {playerIndex + 1}の入力設定が" +
                "完了していません。",
                this
            );

            return;
        }

        InputDevice operationDevice = session.GetDevice(teamIndex, ControllerRole.Operation);
        InputDevice placementDevice = session.GetDevice(teamIndex, ControllerRole.Placement);

        if (operationDevice == null || placementDevice == null)
        {
            Debug.LogError(
                $"[PlayerSetup] " +
                $"Team {teamIndex + 1}のデバイス情報が" +
                "不足しています。",
                this
            );

            return;
        }

        // Player本体を有効化してから、子のPlayerInputへデバイスを割り当てる
        playerInputSet.SetPlayerActive(true);

        bool operationSetupSucceeded = SetupRoleInput(playerInputSet.OperationPlayerInput, operationDevice, operationActionMapName, teamIndex, ControllerRole.Operation);
        bool placementSetupSucceeded = SetupRoleInput(playerInputSet.PlacementPlayerInput, placementDevice, placementActionMapName, teamIndex, ControllerRole.Placement);

        if (!operationSetupSucceeded || !placementSetupSucceeded)
        {
            playerInputSet.SetPlayerActive(false);
            return;
        }
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
            if (playerInputSet != null)
                playerInputSet.SetPlayerActive(false);
        }
    }
}
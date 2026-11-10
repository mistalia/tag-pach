using UnityEngine;

// 再戦準備画面における1Team分のRole・Ready表示を管理する
public sealed class RematchTeamView : MonoBehaviour
{
    [Header("Team")]
    [SerializeField, Range(0, ControllerSessionManager.TeamCount - 1)]
    private int teamIndex;

    [Header("First Controller")]
    [SerializeField]
    private GameObject firstOperationView;

    [SerializeField]
    private GameObject firstPlacementView;

    [SerializeField]
    private ControllerReadyFrame firstReadyFrame;

    [Header("Second Controller")]
    [SerializeField]
    private GameObject secondOperationView;

    [SerializeField]
    private GameObject secondPlacementView;

    [SerializeField]
    private ControllerReadyFrame secondReadyFrame;

    private RematchReadyManager manager;

    private void Start()
    {
        manager = RematchReadyManager.Instance;

        if (manager == null)
        {
            Debug.LogError("[RematchTeamView] RematchReadyManagerが存在しません。", this);
            enabled = false;
            return;
        }

        manager.RoleChanged += HandleRoleChanged;
        manager.ReadyChanged += HandleReadyChanged;
        manager.TeamChanged += HandleTeamChanged;

        Refresh();
    }

    private void OnDestroy()
    {
        if (manager == null)
            return;

        manager.RoleChanged -= HandleRoleChanged;
        manager.ReadyChanged -= HandleReadyChanged;
        manager.TeamChanged -= HandleTeamChanged;
    }

    // 現在保持されているTeam内のRole・Ready状態を表示へ反映する
    public void Refresh()
    {
        RematchTeamReadyState teamState = manager?.GetTeamState(teamIndex);

        if (teamState == null)
            return;

        ApplyFirstRole(teamState.FirstControllerRole);
        ApplySecondRole(teamState.SecondControllerRole);

        firstReadyFrame?.SetReady(teamState.FirstPlayerReadyState.IsReady);
        secondReadyFrame?.SetReady(teamState.SecondPlayerReadyState.IsReady);
    }

    // CHANGEなどでRoleが変更されたControllerだけ表示を更新する
    private void HandleRoleChanged(int changedTeamIndex, int controllerNumber, ControllerRole role)
    {
        if (changedTeamIndex != teamIndex)
            return;

        RematchTeamReadyState teamState = manager.GetTeamState(teamIndex);

        if (teamState == null)
            return;

        if (controllerNumber == teamState.FirstControllerNumber)
        {
            ApplyFirstRole(role);
            return;
        }

        if (controllerNumber == teamState.SecondControllerNumber)
            ApplySecondRole(role);
    }

    // 個別のReady変更を対応するController枠へ反映する
    private void HandleReadyChanged(int changedTeamIndex, int controllerNumber, bool isReady)
    {
        if (changedTeamIndex != teamIndex)
            return;

        RematchTeamReadyState teamState = manager.GetTeamState(teamIndex);

        if (teamState == null)
            return;

        if (controllerNumber == teamState.FirstControllerNumber)
        {
            firstReadyFrame?.SetReady(isReady);
            return;
        }

        if (controllerNumber == teamState.SecondControllerNumber)
            secondReadyFrame?.SetReady(isReady);
    }

    // Team単位の変更後はRoleとReadyをまとめて再同期する
    private void HandleTeamChanged(int changedTeamIndex)
    {
        if (changedTeamIndex != teamIndex)
            return;

        Refresh();
    }

    private void ApplyFirstRole(ControllerRole role)
    {
        bool isOperation = role == ControllerRole.Operation;

        SetActive(firstOperationView, isOperation);
        SetActive(firstPlacementView, !isOperation);
    }

    private void ApplySecondRole(ControllerRole role)
    {
        bool isOperation = role == ControllerRole.Operation;

        SetActive(secondOperationView, isOperation);
        SetActive(secondPlacementView, !isOperation);
    }

    private static void SetActive(GameObject target, bool isActive)
    {
        if (target != null)
            target.SetActive(isActive);
    }
}
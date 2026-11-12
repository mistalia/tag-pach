using UnityEngine;
using UnityEngine.Events;

// Team内の各Controllerが選択しているRoleを表示側へ反映する
public sealed class ControllerRoleSelectionView : MonoBehaviour
{
    [Header("Team")]
    [SerializeField, Range(0, ControllerSessionManager.TeamCount - 1)]
    private int teamIndex;

    [Header("First Controller Role Events")]
    [SerializeField]
    private UnityEvent onFirstControllerOperationSelected;

    [SerializeField]
    private UnityEvent onFirstControllerPlacementSelected;

    [Header("Second Controller Role Events")]
    [SerializeField]
    private UnityEvent onSecondControllerOperationSelected;

    [SerializeField]
    private UnityEvent onSecondControllerPlacementSelected;

    private ControllerSessionManager session;

    private void OnEnable()
    {
        Subscribe();
    }

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    // 現在保持されているRoleを表示へ反映する
    public void Refresh()
    {
        ControllerTeam team = session?.GetTeam(teamIndex);

        if (team == null)
            return;

        ApplyFirstControllerState(team.FirstControllerSelection);
        ApplySecondControllerState(team.SecondControllerSelection);
    }

    // SessionのRole変更とController登録を監視する
    private void Subscribe()
    {
        if (session != null)
            return;

        session = ControllerSessionManager.Instance;

        if (session == null)
            return;

        session.ControllerRoleChanged   += HandleControllerRoleChanged;
        session.ControllerRegistered    += HandleControllerRegistered;
    }

    private void Unsubscribe()
    {
        if (session == null)
            return;

        session.ControllerRoleChanged   -= HandleControllerRoleChanged;
        session.ControllerRegistered    -= HandleControllerRegistered;

        session = null;
    }

    // 変更されたControllerがTeam内のどちらかを判定して表示を更新する
    private void HandleControllerRoleChanged(int changedTeamIndex, int controllerNumber, ControllerRole role)
    {
        if (changedTeamIndex != teamIndex)
            return;

        ControllerTeam team = session?.GetTeam(teamIndex);

        if (team == null)
            return;

        if (team.FirstController?.ControllerNumber == controllerNumber)
        {
            ApplyFirstControllerRole(role);
            return;
        }

        if (team.SecondController?.ControllerNumber == controllerNumber)
            ApplySecondControllerRole(role);
    }

    // Controllerが後から登録された場合も現在状態を表示へ同期する
    private void HandleControllerRegistered(int registeredTeamIndex, RegisteredController controller)
    {
        if (registeredTeamIndex != teamIndex)
            return;

        Refresh();
    }

    private void ApplyFirstControllerState(ControllerRoleSelection selection)
    {
        if (selection == null)
            return;

        ApplyFirstControllerRole(selection.SelectedRole);
    }

    private void ApplySecondControllerState(ControllerRoleSelection selection)
    {
        if (selection == null)
            return;

        ApplySecondControllerRole(selection.SelectedRole);
    }

    private void ApplyFirstControllerRole(ControllerRole role)
    {
        if (role == ControllerRole.Operation)
        {
            onFirstControllerOperationSelected?.Invoke();
            return;
        }

        onFirstControllerPlacementSelected?.Invoke();
    }

    private void ApplySecondControllerRole(ControllerRole role)
    {
        if (role == ControllerRole.Operation)
        {
            onSecondControllerOperationSelected?.Invoke();
            return;
        }

        onSecondControllerPlacementSelected?.Invoke();
    }
}
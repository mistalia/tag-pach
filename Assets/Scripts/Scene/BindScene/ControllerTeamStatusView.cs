using UnityEngine;
using UnityEngine.Events;

// Team‘S‘Ì‚Ì€”õó‘Ô‚ğ•\¦‘¤‚Ö”½‰f‚·‚é
public sealed class ControllerTeamStatusView : MonoBehaviour
{
    [Header("Team")]
    [SerializeField, Range(0, ControllerSessionManager.TeamCount - 1)]
    private int teamIndex;

    [Header("Status Events")]
    [SerializeField]
    private UnityEvent onWaiting;

    [SerializeField]
    private UnityEvent onRoleConflict;

    [SerializeField]
    private UnityEvent onReady;

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

    // Œ»İ‚ÌTeamó‘Ô‚ğ•\¦‚Ö”½‰f‚·‚é
    public void Refresh()
    {
        ControllerTeam team = session?.GetTeam(teamIndex);

        if (team == null)
            return;

        ApplyStatus(team.AreAllReady, team.AreRolesValid, team.CanDeploy);
    }

    // Teamó‘Ô‚Ì•ÏX‚ÆController‚Ì“o˜^ó‘Ô‚ğŠÄ‹‚·‚é
    private void Subscribe()
    {
        if (session != null)
            return;

        session = ControllerSessionManager.Instance;

        if (session == null)
            return;

        session.TeamStatusChanged       += HandleTeamStatusChanged;
        session.ControllerRegistered    += HandleControllerRegistered;
        session.RegistrationCleared     += HandleRegistrationCleared;
    }

    private void Unsubscribe()
    {
        if (session == null)
            return;

        session.TeamStatusChanged       -= HandleTeamStatusChanged;
        session.ControllerRegistered    -= HandleControllerRegistered;
        session.RegistrationCleared     -= HandleRegistrationCleared;

        session = null;
    }

    private void HandleTeamStatusChanged(int changedTeamIndex, bool areAllReady, bool areRolesValid, bool canDeploy)
    {
        if (changedTeamIndex != teamIndex)
            return;

        ApplyStatus(areAllReady, areRolesValid, canDeploy);
    }

    // Controller‚ªŒã‚©‚ç“o˜^‚³‚ê‚½ê‡‚àŒ»İó‘Ô‚ğ•\¦‚Ö“¯Šú‚·‚é
    private void HandleControllerRegistered(int registeredTeamIndex, RegisteredController controller)
    {
        if (registeredTeamIndex != teamIndex)
            return;

        Refresh();
    }

    // Controller“o˜^‚ª‰ğœ‚³‚ê‚½ê‡‚Í€”õ’†•\¦‚Ö–ß‚·
    private void HandleRegistrationCleared()
    {
        onWaiting?.Invoke();
    }

    // Readyó‹µ‚ÆRoled•¡‚Ì—L–³‚©‚çTeam‚Ì•\¦ó‘Ô‚ğŒˆ’è‚·‚é
    private void ApplyStatus(bool areAllReady, bool areRolesValid, bool canDeploy)
    {
        if (!areAllReady)
        {
            onWaiting?.Invoke();
            return;
        }

        if (!areRolesValid)
        {
            onRoleConflict?.Invoke();
            return;
        }

        if (canDeploy)
            onReady?.Invoke();
    }
}
using UnityEngine;

// ControllerTeam‚ÆPlayer‚Ì‘Î‰ŠÖŒW‚ğŠÇ—
public sealed class TeamPlayerAssignmentManager : MonoBehaviour
{
    public static TeamPlayerAssignmentManager Instance { get; private set; }

    private readonly TeamPlayerAssignment[] assignments =
    {
        new TeamPlayerAssignment(0, 0),
        new TeamPlayerAssignment(1, 1)
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ƒV[ƒ“‚ğˆÚ“®‚µ‚Ä‚àTeam‚ÆPlayer‚Ì‘Î‰ŠÖŒW‚ğ•Û
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public int GetPlayerIndex(int teamIndex)
    {
        if (!IsValidTeamIndex(teamIndex))
            return -1;

        return assignments[teamIndex].PlayerIndex;
    }

    public int GetTeamIndex(int playerIndex)
    {
        for (int index = 0; index < assignments.Length; index++)
        {
            if (assignments[index].PlayerIndex == playerIndex)
                return assignments[index].TeamIndex;
        }

        return -1;
    }

    public void SwapPlayerAssignments()
    {
        int team1PlayerIndex = assignments[0].PlayerIndex;
        int team2PlayerIndex = assignments[1].PlayerIndex;

        assignments[0].AssignPlayer(team2PlayerIndex);
        assignments[1].AssignPlayer(team1PlayerIndex);
    }

    public void ResetAssignments()
    {
        assignments[0].AssignPlayer(0);
        assignments[1].AssignPlayer(1);
    }

    private bool IsValidTeamIndex(int teamIndex)
    {
        return teamIndex >= 0 && teamIndex < assignments.Length;
    }
}
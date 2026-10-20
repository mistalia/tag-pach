// ControllerTeam‚ÆPlayer‚Ì‘Î‰ŠÖŒW‚ğŠÇ—
public sealed class TeamPlayerAssignment
{
    public int TeamIndex    { get; }

    public int PlayerIndex  { get; private set; }

    public TeamPlayerAssignment(int teamIndex, int playerIndex)
    {
        TeamIndex   = teamIndex;
        PlayerIndex = playerIndex;
    }

    public void AssignPlayer(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }
}
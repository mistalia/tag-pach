// Äí€”õ‰æ–Ê‚É‚¨‚¯‚é1Team•ª‚ÌRole‚ÆReadyó‘Ô‚ğŠÇ—‚·‚é
public sealed class RematchTeamReadyState
{
    public int FirstControllerNumber { get; }
    public int SecondControllerNumber { get; }

    public ControllerRole FirstControllerRole { get; private set; }
    public ControllerRole SecondControllerRole { get; private set; }

    public RematchPlayerReadyState FirstPlayerReadyState { get; }
    public RematchPlayerReadyState SecondPlayerReadyState { get; }

    public bool AreAllReady => FirstPlayerReadyState.IsReady && SecondPlayerReadyState.IsReady;

    public RematchTeamReadyState(int teamIndex, int firstControllerNumber, int secondControllerNumber, ControllerRole firstControllerRole, ControllerRole secondControllerRole)
    {
        FirstControllerNumber = firstControllerNumber;
        SecondControllerNumber = secondControllerNumber;
        FirstControllerRole = firstControllerRole;
        SecondControllerRole = secondControllerRole;
        FirstPlayerReadyState = new RematchPlayerReadyState(firstControllerNumber);
        SecondPlayerReadyState = new RematchPlayerReadyState(secondControllerNumber);
    }

    // Team“à‚ÌOperation / Placement‚ğ“ü‚ê‘Ö‚¦AŒ»İ‚ÌReadyó‘Ô‚ğ‰ğœ‚·‚é
    public void ChangeRoles()
    {
        ControllerRole previousFirstRole = FirstControllerRole;

        FirstControllerRole = SecondControllerRole;
        SecondControllerRole = previousFirstRole;

        ResetReady();
    }

    // Team“à2l‚ÌReadyó‘Ô‚ğ‰ğœ‚·‚é
    public void ResetReady()
    {
        FirstPlayerReadyState.Reset();
        SecondPlayerReadyState.Reset();
    }

    public RematchPlayerReadyState GetReadyState(int controllerNumber)
    {
        if (controllerNumber == FirstControllerNumber)
            return FirstPlayerReadyState;

        if (controllerNumber == SecondControllerNumber)
            return SecondPlayerReadyState;

        return null;
    }
}
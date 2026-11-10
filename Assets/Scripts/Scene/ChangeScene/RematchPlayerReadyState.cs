// Äí€”õ‰æ–Ê‚É‚¨‚¯‚é1l•ª‚ÌOKó‘Ô‚ğŠÇ—‚·‚é
public sealed class RematchPlayerReadyState
{
    public int ControllerNumber { get; }

    public bool IsReady { get; private set; }

    public RematchPlayerReadyState(int controllerNumber)
    {
        ControllerNumber = controllerNumber;
        IsReady = false;
    }

    // ©•ª‚ÌŒ»İ‚Ì–ğŠ„‚ÅOK‚·‚é
    public bool SetReady()
    {
        if (IsReady)
            return false;

        IsReady = true;

        return true;
    }

    // ©•ª‚ÌOK‚ğæ‚èÁ‚·
    public bool CancelReady()
    {
        if (!IsReady)
            return false;

        IsReady = false;

        return true;
    }

    // ŠÔØ‚ê‚È‚Ç‚Å‹­§“I‚ÉOKó‘Ô‚É‚·‚é
    public void ForceReady()
    {
        IsReady = true;
    }

    // €”õó‘Ô‚ğ‰Šúó‘Ô‚Ö–ß‚·
    public void Reset()
    {
        IsReady = false;
    }
}
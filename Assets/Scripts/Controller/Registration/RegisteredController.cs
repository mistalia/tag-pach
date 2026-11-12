using UnityEngine.InputSystem;

// ©“®“o˜^‚³‚ê‚½Controller‚Ì”Ô†‚ÆInputDevice‚ğ•Û
public sealed class RegisteredController
{
    public int ControllerNumber { get; }

    public InputDevice Device   { get; }

    public RegisteredController(int controllerNumber, InputDevice device)
    {
        ControllerNumber    = controllerNumber;
        Device              = device;
    }
}

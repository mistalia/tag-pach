using UnityEngine.InputSystem;

// 1台の入力デバイスと、チーム内で担当する役割をまとめて保持
public sealed class ControllerAssignment
{
    public InputDevice      Device  { get; }

    public ControllerRole   Role    { get; }

    public ControllerAssignment(InputDevice device, ControllerRole role)
    {
        Device  = device;
        Role    = role;
    }
}
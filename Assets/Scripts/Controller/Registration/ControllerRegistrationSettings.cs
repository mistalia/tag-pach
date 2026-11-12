using UnityEngine;

// Controller‚ÌŽ©“®“o˜^ðŒ‚ð•ÛŽ
[CreateAssetMenu(fileName = "ControllerRegistrationSettings", menuName = "Controller/Registration Settings")]
public sealed class ControllerRegistrationSettings : ScriptableObject
{
    [Header("Keyboard")]
    [SerializeField]
    private KeyboardRegistrationMode keyboardMode = KeyboardRegistrationMode.Disabled;

    public KeyboardRegistrationMode KeyboardMode => keyboardMode;
}
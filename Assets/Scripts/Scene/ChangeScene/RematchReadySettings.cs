using UnityEngine;

// Äí€”õ‰æ–Ê‚ÌŽžŠÔE‘JˆÚ•û–@‚ðÝ’è‚·‚é
[CreateAssetMenu(fileName = "RematchReadySettings", menuName = "Rematch/Ready Settings")]
public sealed class RematchReadySettings : ScriptableObject
{
    [Header("Ready")]
    [SerializeField, Min(0.1f)]
    private float timeLimit = 15.0f;

    [Header("Final Confirmation")]
    [SerializeField]
    private RematchTransitionMode transitionMode = RematchTransitionMode.Automatic;

    [SerializeField, Min(0.0f)]
    private float automaticTransitionDelay = 2.0f;

    public float TimeLimit => timeLimit;
    public RematchTransitionMode TransitionMode => transitionMode;
    public float AutomaticTransitionDelay => automaticTransitionDelay;
    public bool IsValid => IsPositiveFinite(timeLimit) && IsNonNegativeFinite(automaticTransitionDelay);

    private static bool IsPositiveFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0.0f;
    }

    private static bool IsNonNegativeFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0.0f;
    }
}
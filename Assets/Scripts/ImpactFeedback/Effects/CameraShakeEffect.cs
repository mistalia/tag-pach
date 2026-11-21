using UnityEngine;

// Camera Shake‚ğÀs‚·‚éEffect
[CreateAssetMenu(fileName = "CameraShakeEffect", menuName = "Impact Feedback/Effects/Camera Shake")]
public sealed class CameraShakeEffect : ImpactEffect
{
    [SerializeField, Min(0.0f)]
    private float duration = 0.15f;

    [SerializeField, Min(0.0f)]
    private float strength = 0.1f;

    public override void Play(GameObject target)
    {
        FieldCamera.Instance?.CameraShake(duration, strength);
    }
}
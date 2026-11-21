using UnityEngine;

// ƒQ[ƒ€‘S‘Ì‚ÖHitStop‚ğ—v‹
[CreateAssetMenu(fileName = "GlobalHitStopEffect", menuName = "Impact Feedback/Effects/Global HitStop")]
public sealed class GlobalHitStopEffect : ImpactEffect
{
    [SerializeField, Min(0.0f)]
    private float duration = 0.08f;

    public override void Play(GameObject target)
    {
        GlobalTimeEffectController.Instance?.RequestHitStop(duration);
    }
}
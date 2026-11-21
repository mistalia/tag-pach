using UnityEngine;

// ƒQ[ƒ€‘S‘Ì‚ÖSlow‚ğ—v‹
[CreateAssetMenu(fileName = "GlobalSlowEffect", menuName = "Impact Feedback/Effects/Global Slow")]
public sealed class GlobalSlowEffect : ImpactEffect
{
    [SerializeField, Range(0.01f, 0.99f)]
    private float timeScale = 0.5f;

    [SerializeField, Min(0.0f)]
    private float duration = 0.3f;

    public override void Play(GameObject target)
    {
        GlobalTimeEffectController.Instance?.RequestSlow(timeScale, duration);
    }
}
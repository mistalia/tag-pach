using UnityEngine;

// 対象キャラクターへHit Stopを要求
[CreateAssetMenu(fileName = "CharacterHitStopEffect", menuName = "Impact Feedback/Effects/Character HitStop")]
public sealed class CharacterHitStopEffect : ImpactEffect
{
    [SerializeField, Min(0.0f)]
    private float duration = 0.08f;

    public override void Play(GameObject target)
    {
        if (target == null)
            return;

        ICharacterTimeTarget timeTarget = target.GetComponent<ICharacterTimeTarget>();
        timeTarget?.RequestStop(duration);
    }
}
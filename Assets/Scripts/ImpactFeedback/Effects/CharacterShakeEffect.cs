using UnityEngine;

// 対象キャラクターへShakeを要求
[CreateAssetMenu(fileName = "CharacterShakeEffect", menuName = "Impact Feedback/Effects/Character Shake")]
public sealed class CharacterShakeEffect : ImpactEffect
{
    [SerializeField, Min(0.0f)]
    private float duration = 0.1f;

    [SerializeField]
    private CharacterShakeMode mode = CharacterShakeMode.Constant;

    [SerializeField, Min(0.0f)]
    private float positionStrength = 0.05f;

    [SerializeField, Min(0.0f)]
    private float rotationStrength = 3.0f;

    [SerializeField, Min(0.0f)]
    private float frequency = 25.0f;

    public override void Play(GameObject target)
    {
        if (target == null)
            return;

        ICharacterShakeTarget shakeTarget = target.GetComponent<ICharacterShakeTarget>();

        if (shakeTarget == null)
            return;

        CharacterShakeRequest request = new CharacterShakeRequest(duration, mode, positionStrength, rotationStrength, frequency);
        shakeTarget.RequestShake(request);
    }
}
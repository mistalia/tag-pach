using UnityEngine;

// 対象キャラクターへFlashを要求
[CreateAssetMenu(fileName = "CharacterFlashEffect", menuName = "Impact Feedback/Effects/Character Flash")]
public sealed class CharacterFlashEffect : ImpactEffect
{
    [SerializeField]
    private Material flashMaterial;

    [SerializeField, Min(1)]
    private int flashCount = 2;

    [SerializeField, Min(0.0f)]
    private float visibleDuration = 0.035f;

    [SerializeField, Min(0.0f)]
    private float hiddenDuration = 0.025f;

    public override void Play(GameObject target)
    {
        if (target == null || flashMaterial == null)
            return;

        ICharacterFlashTarget flashTarget = target.GetComponent<ICharacterFlashTarget>();

        if (flashTarget == null)
            return;

        CharacterFlashRequest request = new CharacterFlashRequest(flashMaterial, flashCount, visibleDuration, hiddenDuration);
        flashTarget.RequestFlash(request);
    }
}
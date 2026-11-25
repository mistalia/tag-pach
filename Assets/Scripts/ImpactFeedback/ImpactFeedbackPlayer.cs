using UnityEngine;

// ProfileÇ…ìoò^Ç≥ÇÍÇΩImpact EffectÇèáî‘Ç…é¿çsÇ∑ÇÈ
public sealed class ImpactFeedbackPlayer : MonoBehaviour
{
    public static ImpactFeedbackPlayer Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Play(ImpactFeedbackProfile profile, GameObject target = null)
    {
        if (profile == null)
            return;

        foreach (ImpactEffect effect in profile.Effects)
        {
            if (effect != null)
                effect.Play(target);
        }
    }
}
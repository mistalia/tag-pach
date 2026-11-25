using UnityEngine;

// Search-specific references stay on this component; appearance is shared.
public class SearchSkillUI : SkillCooldownUI
{
    [Header("Search")]
    [SerializeField] private SearchSkill searchSkill;

    protected override Player ResolvePlayer() =>
        searchSkill != null ? searchSkill.GetComponent<Player>() : null;

    protected override bool TryReadCooldown(out float remaining, out float duration)
    {
        remaining = searchSkill != null ? searchSkill.RemainingCooldown : 0f;
        duration = searchSkill != null ? searchSkill.CooldownTime : 0f;
        return searchSkill != null;
    }
}

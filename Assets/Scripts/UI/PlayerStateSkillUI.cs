using UnityEngine;

// Reads the player's runtime state, never the shared ScriptableObject asset.
public class PlayerStateSkillUI : SkillCooldownUI
{
    public enum SkillType { Smash, ForcePlace }

    [Header("Player Skill")]
    [SerializeField] private Player player;
    [SerializeField] private SkillType skillType;

    private PlayerStateSmash smash;
    private PlayerStateForcePlace forcePlace;

    protected override Player ResolvePlayer() => player;

    protected override bool SkillIsAvailable =>
        skillType != SkillType.ForcePlace ||
        (TurnManager.Instance != null &&
         TurnManager.Instance.GetCurrentStateType() == TurnStateType.Playing &&
         TurnManager.Instance.GetDefendPlayer() == player);

    protected override bool TryReadCooldown(out float remaining, out float duration)
    {
        remaining = duration = 0f;
        if (player == null) return false;

        if (skillType == SkillType.Smash)
        {
            if (smash == null && player.TryGetRuntimeState("smash", out var state))
                smash = state as PlayerStateSmash;
            if (smash == null) return false;
            remaining = smash.RemainingCooldown;
            duration = smash.CooldownTime;
        }
        else
        {
            if (forcePlace == null && player.TryGetRuntimeState("forcePlace", out var state))
                forcePlace = state as PlayerStateForcePlace;
            if (forcePlace == null) return false;
            remaining = forcePlace.RemainingCooldown;
            duration = forcePlace.CooldownTime;
        }
        return true;
    }
}

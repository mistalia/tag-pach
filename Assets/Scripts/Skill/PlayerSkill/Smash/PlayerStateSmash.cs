using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStateSmash",
    menuName = "PlayerState/Smash")]
public class PlayerStateSmash : PlayerState
{
    private const float MinDirectionSqrMagnitude = 0.0001f;

    [Header("スキル設定")]
    [SerializeField]
    private SmashSettings settings;

    private float nextUseTime;
    private int lastUseFrame = -1;

    public float RemainingCooldown =>
        Mathf.Max(0f, nextUseTime - Time.time);

    public bool CanUse(Player player)
    {
        return player != null &&
               player.isActiveAndEnabled &&
               !player.IsRepositioning &&
               settings != null &&
               settings.IsValid &&
               Time.timeScale > 0f &&
               Time.time >= nextUseTime &&
               lastUseFrame != Time.frameCount &&
               TryGetOpponent(player, out _);
    }

    public override void EnterState(
        Player owner,
        PlayerInputReader input)
    {
        base.EnterState(owner, input);

        if (CanUse(owner) &&
            TryGetOpponent(owner, out var opponent))
        {
            // 空振りでも消費し、ステートを抜けても再使用時刻を保持する。
            nextUseTime = Time.time + settings.CooldownTime;
            lastUseFrame = Time.frameCount;

            Vector3 direction =
                opponent.transform.position - owner.transform.position;

            direction.y = 0f;

            if (direction.magnitude <= settings.Range)
            {
                // 同じ位置に重なった場合は、自分の正面へ押し戻す。
                if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                {
                    direction = owner.transform.forward;
                    direction.y = 0f;

                    if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                    {
                        direction = Vector3.forward;
                    }
                }

                opponent.AddKnockback(
                    direction,
                    settings.KnockbackDistance,
                    settings.KnockbackSpeed,
                    settings.StunDuration);
            }
        }

        owner.ChangeState("idle");
    }

    private bool TryGetOpponent(
        Player player,
        out Player opponent)
    {
        opponent = null;

        Player[] players =
            Object.FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player candidate in players)
        {
            if (!candidate.isActiveAndEnabled ||
                candidate == player ||
                candidate.gameObject.scene != player.gameObject.scene)
            {
                continue;
            }

            // 1対1の前提が崩れた場合は、検索順で相手を決めない。
            if (opponent != null)
            {
                Debug.LogWarning(
                    "スマッシュの対象が複数います。" +
                    "同じシーンの有効な対戦プレイヤーを2体にしてください。",
                    player);

                opponent = null;
                return false;
            }

            opponent = candidate;
        }

        return opponent != null;
    }
}
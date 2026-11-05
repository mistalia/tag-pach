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

    [Header("攻撃範囲表示")]
    [SerializeField, Min(0.01f)]
    private float rangeDisplayDuration = 0.2f;

    public float RemainingCooldown =>
        Mathf.Max(0f, nextUseTime - Time.time);

    public bool CanUse(Player player)
    {
        return player != null &&
               player.isActiveAndEnabled &&
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

        if (CanUse(owner))
        {
            // 空振りでも消費し、ステートを抜けても再使用時刻を保持する。
            nextUseTime = Time.time + settings.CooldownTime;
            lastUseFrame = Time.frameCount;

            // 攻撃範囲を表示
            ShowAttackRange(owner);

            ExecuteHit();

        }

        owner.ChangeState("idle");
    }

    private bool TryGetOpponent(
        Player player,
        out Player opponent)
    {
        opponent = null;

        Player[] players =
            Object.FindObjectsByType<Player>();

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

    private void ShowAttackRange(Player owner)
    {
        SC_SmashRangeSector sector = owner.GetComponentInChildren<SC_SmashRangeSector>(true);

        if (sector == null)
        {
            Debug.LogWarning("SmashRangeSectorが見つかりません");
            return;
        }

        sector.gameObject.SetActive(true);

        sector.Init(
            owner.transform,
            settings.Range,
            settings.AttackAngle,
            rangeDisplayDuration
        );
    }

    // 攻撃判定を実行する
    public void ExecuteHit()
    {
        if (owner == null || settings == null)
            return;

        if (!TryGetOpponent(owner, out var opponent))
            return;

        Vector3 direction = opponent.transform.position - owner.transform.position;

        direction.y = 0f;

        Vector3 forward = owner.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < MinDirectionSqrMagnitude)
            forward = Vector3.forward;

        forward.Normalize();

        // 距離判定
        if (direction.magnitude > settings.Range)
            return;

        if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
            direction = forward;

        // 角度判定
        float angle = Vector3.Angle(forward, direction);

        if (angle > settings.AttackAngle * 0.5f)
            return;

        // ノックバック
        opponent.AddKnockback(
            direction,
            settings.KnockbackDistance,
            settings.KnockbackSpeed,
            settings.StunDuration
        );
    }
}
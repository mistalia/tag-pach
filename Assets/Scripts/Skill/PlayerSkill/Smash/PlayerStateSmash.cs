using System.Collections.Generic;
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

    public float CooldownTime => settings != null ? settings.CooldownTime : 0f;

    public float RemainingCooldown =>
        Mathf.Max(0f, nextUseTime - Time.time);

    public bool CanUse(Player player)
    {
        // 共通条件
        if (player == null ||
            !player.isActiveAndEnabled ||
            settings == null ||
            !settings.IsValid ||
            Time.timeScale <= 0f ||
            Time.time < nextUseTime ||
            lastUseFrame == Time.frameCount)
        {
            return false;
        }

        // チュートリアル
        if (TutorialManager.Instance != null)
        {
            return true;
        }

        // 通常戦闘
        return TryGetOpponent(player, out _);
    }

    public override void EnterState(Player owner, PlayerInputReader input, Animator animator)
    {
        base.EnterState(owner, input, animator);

        // クールタイム設定
        nextUseTime = Time.time + settings.CooldownTime;
        lastUseFrame = Time.frameCount;

        // 攻撃範囲を表示
        ShowAttackRange(owner);

        // 攻撃実行
        if (TutorialManager.Instance != null)
        {
            ExecuteAttackDummy();
        }
        else
        {
            ExecuteHit();
        }

        owner.ChangeState("idle");
        animator?.SetTrigger("tSmash");
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

    private bool IsInAttackRange(Transform target)
    {
        if (owner == null || target == null)
        {
            return false;
        }

        Vector3 direction =
            target.position - owner.transform.position;

        direction.y = 0f;

        Vector3 forward = owner.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < MinDirectionSqrMagnitude)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();

        // 距離判定
        if (direction.sqrMagnitude > settings.Range * settings.Range)
        {
            return false;
        }

        // 角度判定
        if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
        {
            direction = forward;
        }

        float angle = Vector3.Angle(forward, direction);

        return angle <= settings.AttackAngle * 0.5f;
    }

    // 攻撃判定を実行する
    private void ExecuteHit()
    {
        if (!TryGetOpponent(owner, out var opponent))
        {
            return;
        }

        if (!IsInAttackRange(opponent.transform))
        {
            return;
        }

        Vector3 direction =
            opponent.transform.position - owner.transform.position;

        direction.y = 0f;

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
            settings.StunDuration
        );
    }


    // リュートリアル用

    private void ExecuteAttackDummy()
    {
        Dummy_Player[] dummies = Object.FindObjectsByType<Dummy_Player>();

        foreach (var dummy in dummies)
        {
            if (dummy == null ||
                !dummy.isActiveAndEnabled ||
                !IsInAttackRange(dummy.transform))
            {
                continue;
            }

            dummy.HitAttack(owner);
        }
    }
}
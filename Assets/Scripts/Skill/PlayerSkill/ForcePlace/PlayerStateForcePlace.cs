using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStateForcePlace",
    menuName = "PlayerState/ForcePlace")]
public class PlayerStateForcePlace : PlayerState
{
    private enum ForcePlaceState
    {
        Charge,
        Activate,
        End
    }

    private ForcePlaceState currentPhase;
    private float phaseTimer;

    [Header("スキル設定")]
    [SerializeField]
    private ForcePlaceSettings settings;

    private SC_ForcePlaceRangeCircle rangeCircle;

    private float nextUseTime;
    private int lastUseFrame = -1;
    private int activeTurn;
    private bool resolvingHit;
    private bool skillActive;
    private GameObject skillVisual;

    public float RemainingCooldown => 
        Mathf.Max(0f, nextUseTime - Time.time);

    public bool CanUse(Player player)
    {
        TurnManager turn = TurnManager.Instance;

        return player != null &&
            player.isActiveAndEnabled &&
            settings != null &&
            settings.IsValid &&
            Time.timeScale > 0f &&
            Time.time >= nextUseTime &&
            lastUseFrame != Time.frameCount &&
            turn != null &&
            turn.IsBattleStarted() &&
            !turn.IsBattlePaused() &&
            turn.GetCurrentStateType() == TurnStateType.Playing &&
            turn.GetDefendPlayer() == player &&
            turn.GetAttackPlayer() != null;

    }

    public override void EnterState(Player owner,PlayerInputReader input)
    {
        base.EnterState(owner, input);
        Debug.Log("強制配置ステートに入った", owner);

        if (!CanUse(owner))
        {
            owner.ChangeState("idle");
            return;
        }

        activeTurn = TurnManager.Instance.GetTurnCount();

        // ステート終了後も再使用時刻を保持する。
        nextUseTime = Time.time + settings.CooldownTime;
        lastUseFrame = Time.frameCount;

        resolvingHit = false;
        skillActive = true;

        // チャージ開始
        currentPhase = ForcePlaceState.Charge;
        phaseTimer = settings.ChargeDuration;

        Transform visual = owner.transform.Find("ForcePlaceVisual");

        skillVisual = visual != null ? visual.gameObject : null;

        if (skillVisual != null)
        {
            skillVisual.SetActive(true);
        }

        // 円形の攻撃範囲を取得
        rangeCircle = owner.GetComponentInChildren<SC_ForcePlaceRangeCircle>(true);

        if (rangeCircle != null)
        {
            rangeCircle.gameObject.SetActive(true);

            rangeCircle.Init(settings.HitRadius, settings.HitForwardOffset);
        }
    }

    public override void UpdateState()
    {
        if (!skillActive)
        {
            return;
        }

        TurnManager turn = TurnManager.Instance;

        if (!IsActiveTurn(turn))
        {
            owner.ChangeState("idle");
            return;
        }

        if (Time.timeScale <= 0f || turn.IsBattlePaused())
        {
            return;
        }

        switch (currentPhase)
        {
            case ForcePlaceState.Charge:
                UpdateCharge();
                break;

            case ForcePlaceState.Activate:
                ActivateSkill();
                break;

            case ForcePlaceState.End:
                UpdateEnd();
                break;
        }
    }

    private void UpdateCharge()
    {
        phaseTimer -= Time.deltaTime;

        if (phaseTimer > 0f)
        {
            return;
        }

        currentPhase = ForcePlaceState.Activate;

        Debug.Log("ForcePlace：Activate", owner);

        // チャージ終了後、直ちに攻撃判定
        ActivateSkill();
    }

    private void ActivateSkill()
    {
        if (!skillActive ||
            currentPhase != ForcePlaceState.Activate)
        {
            return;
        }

        // 攻撃判定を1回だけ実行
        ExecuteHit();

        // 命中・空振りにかかわらずENDへ移行
        currentPhase = ForcePlaceState.End;
        phaseTimer = settings.EndDuration;

        Debug.Log("ForcePlace：End開始", owner);
    }

    private void UpdateEnd()
    {
        phaseTimer -= Time.deltaTime;

        if (phaseTimer > 0f)
        {
            return;
        }

        owner.ChangeState("idle");
    }

    public override void ExitState()
    {
        skillActive = false;
        resolvingHit = false;
        phaseTimer = 0f;

        if (skillVisual != null)
        {
            skillVisual.SetActive(false);
        }

        if (rangeCircle != null)
        {
            rangeCircle.gameObject.SetActive(false);
        }
    }

    public bool TryHit(Player opponent)
    {
        if (!skillActive ||
            currentPhase != ForcePlaceState.Activate ||
            resolvingHit ||
            opponent == null ||
            opponent == owner ||
            !opponent.isActiveAndEnabled ||
            !IsInHitRange(opponent))
        {
            return false;
        }

        TurnManager turn = TurnManager.Instance;

        if (!IsActiveTurn(turn) ||
            turn.IsBattlePaused() ||
            Time.timeScale <= 0f ||
            turn.GetAttackPlayer() != opponent)
        {
            return false;
        }

        resolvingHit = true;

        try
        {
            if (!opponent.TryForcePlaceStone())
            {
                return false;
            }

            Debug.Log("ForcePlace命中：相手の駒を強制配置",owner);

            return true;
        }
        finally
        {
            resolvingHit = false;
        }
    }

    private bool IsActiveTurn(TurnManager turn)
    {
        return turn != null &&
               turn.IsBattleStarted() &&
               turn.GetCurrentStateType() == TurnStateType.Playing &&
               turn.GetTurnCount() == activeTurn &&
               turn.GetDefendPlayer() == owner;
    }

    private bool IsInHitRange(Player opponent)
    {
        if (opponent == null || owner == null)
        {
            return false;
        }

        Vector3 forward = owner.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        forward.Normalize();

        // 円の中心
        Vector3 center = owner.transform.position + forward * settings.HitForwardOffset;

        Vector3 direction = opponent.transform.position - center;

        // 水平方向の距離で判定
        direction.y = 0f;

        return direction.sqrMagnitude <= settings.HitRadius * settings.HitRadius;
    }

    // 攻撃判定を実行
    public void ExecuteHit()
    {
        if (!skillActive ||
            currentPhase != ForcePlaceState.Activate ||
            owner == null ||
            settings == null)
        {
            return;
        }

        TurnManager turn = TurnManager.Instance;

        if (!IsActiveTurn(turn) ||
            turn.IsBattlePaused() ||
            Time.timeScale <= 0f)
        {
            return;
        }

        Player opponent = turn.GetAttackPlayer();

        if (opponent == null)
        {
            return;
        }

        TryHit(opponent);
    }
}
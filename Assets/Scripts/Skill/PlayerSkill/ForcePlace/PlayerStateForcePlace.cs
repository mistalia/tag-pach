using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStateForcePlace",
    menuName = "PlayerState/ForcePlace")]
public class PlayerStateForcePlace : PlayerState
{
    [Header("スキル設定")]
    [SerializeField]
    private ForcePlaceSettings settings;

    [Header("移動")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 10f;

    private Rigidbody body;
    private float remainingDuration;
    private float nextUseTime;
    private int lastUseFrame = -1;
    private int activeTurn;
    private bool resolvingHit;

    private GameObject skillVisual;


    public float RemainingDuration => remainingDuration;

    public float RemainingCooldown =>
        Mathf.Max(0f, nextUseTime - Time.time);

    public bool CanUse(Player player)
{
    TurnManager turn = TurnManager.Instance;

    return player != null &&
           player.isActiveAndEnabled &&
           !player.IsRepositioning &&
           player.TryGetComponent<Rigidbody>(out _) &&
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

    public override void EnterState(
        Player owner,
        PlayerInputReader input)
    {
        base.EnterState(owner, input);
        Debug.Log("強制配置ステートに入った", owner);

        //if (!CanUse(owner) || input == null || !input.IsConfigured)
        //{
        //    owner.ChangeState("idle");
        //    return;
        //}

        body = owner.GetComponent<Rigidbody>();

        remainingDuration = settings.ActiveDuration;
        activeTurn = TurnManager.Instance.GetTurnCount();

        // ステート終了後も再使用時刻を保持する。
        nextUseTime = Time.time + settings.CooldownTime;
        lastUseFrame = Time.frameCount;

        Debug.Log("強制配置スキル発動", owner);


        Transform visual = owner.transform.Find("ForcePlaceVisual");

        skillVisual = visual != null ? visual.gameObject : null;

        if (skillVisual != null)
        {
            skillVisual.SetActive(true);
        }
    }

    public override void UpdateState()
    {
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

        remainingDuration = Mathf.Max(
            0f, remainingDuration - Time.deltaTime);

        if (remainingDuration <= 0f)
        {
            owner.ChangeState("idle");
        }
    }

    public override void FixedUpdateState()
    {
        TurnManager turn = TurnManager.Instance;

        if (body == null || input == null ||
            remainingDuration <= 0f ||
            !IsActiveTurn(turn) ||
            Time.timeScale <= 0f || turn.IsBattlePaused())
        {
            return;
        }

        Vector2 moveInput = input.MoveValue;
        Vector3 direction = new Vector3(
            moveInput.x, 0f, moveInput.y);

        body.MovePosition(
            body.position +
            direction * moveSpeed * Time.fixedDeltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            body.MoveRotation(
                Quaternion.Slerp(
                    body.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime));
        }
    }

    public override void ExitState()
    {
        if (skillVisual != null)
        {
            skillVisual.SetActive(false);
        }

        remainingDuration = 0f;

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
        }
    }


    public bool TryHit(Player opponent)
    {
        if (resolvingHit ||
            remainingDuration <= 0f ||
            opponent == null ||
            opponent == owner ||
            !opponent.isActiveAndEnabled)
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
            // 前回Player.csに追加した関数を、相手側で呼ぶ。
            if (!opponent.TryForcePlaceStone())
            {
                return false;
            }

            // 配置できたら、スキルの効果を終了する。
            remainingDuration = 0f;

            Debug.Log("接触成功：相手の駒を強制配置", owner);
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
}
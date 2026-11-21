using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStateIdle", menuName = "PlayerState/Idle")]
public class PlayerStateIdle : PlayerState
{
    public override void EnterState(Player owner, PlayerInputReader input)
    {
        base.EnterState(owner, input);
    }

    public override void UpdateState()
    {
        base.UpdateState();

        // バトル開始演出中は移動禁止
        if (owner.IsBattleStart())
        {
            return;
        }

        // 入力があれば移動状態に遷移
        if (input.MoveValue != Vector2.zero)
        {
            owner.ChangeState("move");
        }
    }

    public override void FixedUpdateState()
    {
        base.FixedUpdateState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }
}

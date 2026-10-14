using UnityEngine;

public class TurnStartState : TurnBaseState
{
    public override void Enter(TurnManager manager)
    {
        manager.ResetTurnTimer();
        TurnUI.Instance.SetColor(manager.GetAttackPlayer().TeamColor);
    }


    public override void UpdateState(TurnManager manager)
    {
        manager.ChangeState(TurnStateType.Playing);
    }


    public override void Exit(TurnManager manager)
    {

    }
}

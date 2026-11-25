using UnityEngine;

public class TurnStartState : TurnBaseState
{
    public override void Enter(TurnManager manager)
    {
        manager.ResetTurnTimer();
        TurnUI.Instance.SetColor(manager.GetAttackPlayer().TeamColor);
        FieldManager.Instance.SetFrameColor(manager.GetAttackPlayer().TeamColor);
    }


    public override void UpdateState(TurnManager manager)
    {
        FieldEventManager eventManager = FieldEventManager.Instance;

        if (eventManager != null && eventManager.HasTurnEvent(manager.GetTurnCount()))
        {
            manager.ChangeState(TurnStateType.FieldEvent);
            return;
        }

        manager.ChangeState(TurnStateType.Playing);
    }


    public override void Exit(TurnManager manager)
    {

    }
}

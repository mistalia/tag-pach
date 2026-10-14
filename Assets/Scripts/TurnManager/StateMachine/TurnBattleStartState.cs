using UnityEngine;

public class TurnBattleStartState : TurnBaseState
{
    public override void Enter(TurnManager manager)
    {
        ARScanController.Instance?.OnBattleStart();
    }


    public override void UpdateState(TurnManager manager)
    {
        manager.ChangeState(TurnStateType.TurnStart);
    }


    public override void Exit(TurnManager manager)
    {

    }
}


public enum TurnStateType
{
    BattleStart,
    TurnStart,
    FieldEvent,
    Playing,
    TurnEnd,
    Result,
    Reposition
}

public abstract class TurnBaseState
{

    // State開始時
    public abstract void Enter(TurnManager manager);

    // State更新
    public abstract void UpdateState(TurnManager manager);

    // State終了時
    public abstract void Exit(TurnManager manager);
}

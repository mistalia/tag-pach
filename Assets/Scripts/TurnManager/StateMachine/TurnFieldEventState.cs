using UnityEngine;

public class TurnFieldEventState : TurnBaseState
{
    private bool eventStarted;

    public override void Enter(TurnManager manager)
    {
        eventStarted = false;

        FieldEventManager eventManager = FieldEventManager.Instance;

        if (eventManager == null)
        {
            return;
        }

        // 特殊イベントを開始
        eventStarted = eventManager.StartTurnEvent(manager.GetTurnCount());

        manager.GetAttackPlayer().SetCanPlaceStone(false);
    }

    public override void UpdateState(TurnManager manager)
    {
        FieldEventManager eventManager = FieldEventManager.Instance;

        // イベントが開始できなければ通常ターンへ
        if (!eventStarted || eventManager == null)
        {
            manager.ChangeState(TurnStateType.Playing);
            return;
        }

        // 演出が終了するまで待機
        if (eventManager.IsEventPlaying)
        {
            return;
        }

        // 回転後の勝利判定
        int matchCount = MatchStorage.Instance.GetMatchCount();
        if (matchCount > 0)
        {
            MatchStorage.Instance.SortMatchData();
        }
        for (int i = 0; i < matchCount; i++)
        {
            MatchData matchData = MatchStorage.Instance.PopMatchData();
            int num = matchData.Num;
            num = Mathf.Clamp(num, 0, 6);
            switch (num)
            {
                //6以上
                case 6:
                    // 6ライン以上
                    Debug.Log($"6ライン以上を検知(所有者:{matchData.Owner}): 起点位置:{matchData.Coordinate} / ライン方向{matchData.Direction}]");
                    manager.SetBattleWinner(matchData.Owner);
                    manager.ChangeState(TurnStateType.Result);
                    return;

                case 5:
                    // 5ライン
                    Debug.Log($"5ラインを検知(所有者:{matchData.Owner}): 起点位置:{matchData.Coordinate} / ライン方向{matchData.Direction}]");
                    manager.SetBattleWinner(matchData.Owner);
                    manager.ChangeState(TurnStateType.Result);
                    return;

                case 4:
                    // 4ライン
                    Debug.Log($"4ラインを検知(所有者:{matchData.Owner}): 起点位置:{matchData.Coordinate} / ライン方向{matchData.Direction}]");
                    break;

                case 3:
                    // 3ライン
                    Debug.Log($"3ラインを検知(所有者:{matchData.Owner}): 起点位置:{matchData.Coordinate} / ライン方向{matchData.Direction}]");
                    break;
            }
        }
        MatchStorage.Instance.ClearMatchData();

        manager.ChangeState(TurnStateType.Playing);
    }

    public override void Exit(TurnManager manager)
    {
        manager.GetAttackPlayer().SetCanPlaceStone(true);
    }
}

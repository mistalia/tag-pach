using UnityEngine;
using UnityEngine.SceneManagement;

public class TurnEndState : TurnBaseState
{
 
    public override void Enter(TurnManager manager)
    {
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

                    if(RandomRemoveEvent.Instance != null)
                    {
                        RandomRemoveEvent.Instance.IncreaseEventProbability();
                        RandomRemoveEvent.Instance.ExecuteRandomRemoveEvent();
                    }
                    break;

                case 3:
                    // 3ライン
                    Debug.Log($"3ラインを検知(所有者:{matchData.Owner}): 起点位置:{matchData.Coordinate} / ライン方向{matchData.Direction}]");
                    break;
            }
        }
        MatchStorage.Instance.ClearMatchData();

        var fieldManager = FieldManager.Instance;
        var capacity = fieldManager.GetFieldTileCount();

        // ランダム削除イベントの実行条件をチェック
        if (fieldManager.GetStoneCount() >= capacity * 0.9f)
        {
            // 石の数がフィールドの90％以上の場合、ランダム削除イベントを実行
            if(RandomRemoveEvent.Instance != null)
            {
                RandomRemoveEvent.Instance.ExecuteSaturationRemoveEvent();
            }
        }

        if(fieldManager.IsRemoveing)
        {
            WarningEvent.Instance.SetWarningEventActive(true);
        }
    }


    public override void UpdateState(TurnManager manager)
    {
        if(FieldManager.Instance.IsRemoveing)
        {
            return;
        }

        manager.ProceedNextTurn();
        
    }


    public override void Exit(TurnManager manager)
    {
        if(WarningEvent.Instance != null)
            WarningEvent.Instance.SetWarningEventActive(false);
    }
}

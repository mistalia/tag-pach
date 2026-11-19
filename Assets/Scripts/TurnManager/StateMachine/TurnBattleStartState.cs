using UnityEngine;

public class TurnBattleStartState : TurnBaseState
{
    private GameStartUI gameStartUI;
    private bool isUIFinished;

    public override void Enter(TurnManager manager)
    {
        isUIFinished = false;

        ARScanController.Instance?.OnBattleStart();

        // UI取得
        gameStartUI = Object.FindAnyObjectByType<GameStartUI>();

        if (gameStartUI == null)
        {
            Debug.LogWarning("GameStartUIが見つかりません。");

            isUIFinished = true;
            return;
        }

        // UI表示開始
        gameStartUI.Show(() => { isUIFinished = true; });
    }


    public override void UpdateState(TurnManager manager)
    {
        // UI表示中は待機
        if (!isUIFinished)
        {
            return;
        }

        manager.ChangeState(TurnStateType.TurnStart);
    }


    public override void Exit(TurnManager manager)
    {
        gameStartUI?.Hide();
        gameStartUI = null;
    }
}

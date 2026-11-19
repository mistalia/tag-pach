using UnityEngine;

public class TurnResultState : TurnBaseState
{
    private bool player1Ready;
    private bool player2Ready;

    private bool resultFinished;
    private bool isUIFinished;

    private Player winner;
    private GameEndUI gameEndUI;

    public override void Enter(TurnManager manager)
    {
        player1Ready = false;
        player2Ready = false;
        resultFinished = false;

        winner = manager.GetBattleWinner();

        if (winner == null)
        {
            Debug.LogWarning("TurnResultState : Battle Winner が設定されていません。");

            return;
        }

        BattleManager.Instance.SetBattleResult(winner);

        manager.GetPlayer1().StartResult();
        manager.GetPlayer2().StartResult();

        // UI取得
        gameEndUI = Object.FindAnyObjectByType<GameEndUI>();

        if (gameEndUI == null)
        {
            Debug.LogWarning("GameEndUIが見つかりません。");

            isUIFinished = true;
            return;
        }

        // ② 更新された勝利数を取得
        int player1Wins = BattleManager.Instance.GetPlayer1Wins();
        int player2Wins = BattleManager.Instance.GetPlayer2Wins();

        // ③ 勝者名を取得
        string winnerName = manager.GetPlayerName(winner);

        // 結果UI表示
        gameEndUI.Show(winner,manager.GetPlayer1(),manager.GetPlayer2(),player1Wins,player2Wins,() =>{isUIFinished = true;});
    }


    public override void UpdateState(TurnManager manager)
    {        
        FieldManager.Instance.HighlightAllStoneColor();

        // 両Playerが確認完了
        if (player1Ready && player2Ready)
        {
            FinishResult();
        }
    }


    public override void Exit(TurnManager manager)
    {
        gameEndUI?.Hide();
        gameEndUI = null;
    }

    // ========================================
    // Playerから確認通知
    // ========================================

    public void RequestEnd(Player player, TurnManager manager)
    {
        if (resultFinished)
        {
            return;
        }

        if (player == manager.GetPlayer1())
        {
            player1Ready = true;
        }
        else if (player == manager.GetPlayer2())
        {
            player2Ready = true;
        }
    }

    // ========================================
    // Result終了
    // ========================================
    private void FinishResult()
    {
        if (resultFinished)
        {
            return;
        }

        resultFinished = true;

        // UIを閉じる
        gameEndUI?.Hide();

        BattleManager.Instance.OnBattleFinished();
    }


}

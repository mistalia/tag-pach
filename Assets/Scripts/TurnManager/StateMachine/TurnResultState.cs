using UnityEngine;

public class TurnResultState : TurnBaseState
{
    private bool player1Ready;
    private bool player2Ready;

    private bool resultFinished;

    private Player winner;

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

        BattleManager.Instance.OnBattleFinished();
    }


}

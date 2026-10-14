using UnityEngine;

public class TurnRepositionState : TurnBaseState
{

    private bool player1Ready;
    private bool player2Ready;

    // 一人目（Defend）が降りたか
    private bool firstPlayerExited;

    // 一人目が降りてからの経過時間
    private float secondPlayerTimer;


    public override void Enter(TurnManager manager)
    {

        player1Ready = false;
        player2Ready = false;

        firstPlayerExited = false;

        secondPlayerTimer = 0.0f;

        // 最大待ち時間
        manager.SetTurnTimer(manager.GetRepositionTimeLimit());

        manager.GetPlayer1().StartReposition();
        manager.GetPlayer2().StartReposition();

        TurnUI.Instance.SetColor(Color.purple);
    }

    public override void UpdateState(TurnManager manager)
    {
        // ========================================
        // まだ一人目が降りていない
        // ========================================
        if (!firstPlayerExited)
        {
            manager.UpdateTurnTimer();

            // 両PlayerがReady
            bool bothReady = player1Ready && player2Ready;

            // 時間切れ
            bool timeUp = manager.GetTurnTimer() <= 0.0f;

            // ========================================
            // 両方Ready または 時間切れ
            // ========================================
            if (bothReady || timeUp)
            {
                FirstPlayerExit(manager);
            }

            return;
        }

        // ========================================
        // 一人目が降りた後
        // ========================================
        secondPlayerTimer -= Time.deltaTime;

        // Debug表示用
        manager.SetTurnTimer(Mathf.Max(secondPlayerTimer, 0.0f));

        if (secondPlayerTimer <= 0.0f)
        {
            SecondPlayerExit(manager);
        }
    }

    // PlayerからReady通知
    public void RequestEnd(Player player, TurnManager manager)
    {
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
    // 一人目
    // ========================================
    private void FirstPlayerExit(TurnManager manager)
    {
        if (firstPlayerExited)
        {
            return;
        }

        firstPlayerExited = true;

        // 一人目 = Defend Player
        manager.GetDefendPlayer().EndReposition();

        // 二人目までのDelay開始
        secondPlayerTimer = manager.GetSecondPlayerExitDelay();

        // Delayが0なら即終了
        if (secondPlayerTimer <= 0.0f)
        {
            SecondPlayerExit(manager);
        }
    }

    // ========================================
    // 二人目
    // ========================================
    private void SecondPlayerExit(TurnManager manager)
    {
        // 二人目 = Attack Player
        manager.GetAttackPlayer().EndReposition();

        manager.ChangeState(TurnStateType.TurnStart);
    }

    public override void Exit(TurnManager manager)
    {
    }
}

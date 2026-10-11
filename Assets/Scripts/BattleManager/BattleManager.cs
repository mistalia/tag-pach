using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // ========================================
    // Battle Settings
    // ========================================

    [Header("Battle Settings")]

    [Tooltip("勝利に必要な本数")]
    [SerializeField]
    private int requiredWins = 2;

    // ========================================
    // Battle Data
    // ========================================

    // 現在何本目のBattleか
    private int battleCount = 1;

    // Player1の勝利数
    private int player1Wins = 0;

    // Player2の勝利数
    private int player2Wins = 0;

    // 前回のBattle勝者
    private Player previousWinner;

    // 前回のBattle敗者
    private Player previousLoser;

    // 3本勝負全体の勝者
    private Player battleWinner;

    // ========================================
    // Unity
    // ========================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ========================================
    // Battle Result
    // ========================================
    public void SetBattleResult(Player winner)
    {
        if (winner == null)
        {
            Debug.LogWarning("BattleManager : winner が null です。");

            return;
        }

        TurnManager turnManager = TurnManager.Instance;

        Player player1 = turnManager.GetPlayer1();

        Player player2 = turnManager.GetPlayer2();

        // ========================================
        // 勝者 / 敗者を決定
        // ========================================

        if (winner == player1)
        {
            player1Wins++;

            previousWinner = player1;
            previousLoser = player2;
        }
        else if (winner == player2)
        {
            player2Wins++;

            previousWinner = player2;
            previousLoser = player1;
        }
        else
        {
            Debug.LogWarning("BattleManager : " + "TurnManagerに登録されていないPlayerです。");

            return;
        }

        // ========================================
        // 最終勝者判定
        // ========================================

        battleWinner = null;

        if (player1Wins >= requiredWins)
        {
            battleWinner = player1;
        }
        else if (player2Wins >= requiredWins)
        {
            battleWinner = player2;
        }

        // ========================================
        // 勝利演出
        // ========================================

        if (battleWinner != null)
        {
            //最終勝利演出

        }
        else
        {
            //ターン勝利演出

        }
    }

    public void OnBattleFinished()
    {
        // ========================================
        // 3本勝負終了判定
        // ========================================

        if (battleWinner != null)
        {
            FinishBattle();
            return;
        }

        // ========================================
        // 次Battle
        // ========================================

        PrepareNextBattle();
    }

    // ========================================
    // Next Battle
    // ========================================
    private void PrepareNextBattle()
    {
        battleCount++;

        // 前Battleのデータをリセット
        MatchStorage.Instance.ClearMatchData();
        FieldManager.Instance.ResetField();

        TurnManager turnManager = TurnManager.Instance;
        turnManager.GetPlayer1().ResetBattle();
        turnManager.GetPlayer2().ResetBattle();

        // 次Battle開始
        turnManager.StartNextBattle(previousLoser);
    }

    // ========================================
    // Finish
    // ========================================
    private void FinishBattle()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager != null)
        {
            turnManager.EndBattle();
        }

        SceneManager.LoadScene("ResultScene");
    }

    // ========================================
    // Reset
    // ========================================
    public void ResetBattle()
    {
        battleCount = 1;

        player1Wins = 0;
        player2Wins = 0;

        previousWinner = null;
        previousLoser = null;

        battleWinner = null;
    }

    // ========================================
    // Getter
    // ========================================
    public int GetBattleCount()
    {
        return battleCount;
    }

    public int GetPlayer1Wins()
    {
        return player1Wins;
    }

    public int GetPlayer2Wins()
    {
        return player2Wins;
    }

    public Player GetPreviousWinner()
    {
        return previousWinner;
    }

    public Player GetPreviousLoser()
    {
        return previousLoser;
    }

    public Player GetBattleWinner()
    {
        return battleWinner;
    }

#if UNITY_EDITOR

    private void OnGUI()
    {
        GUI.color = Color.black;

        GUI.Label(new Rect(320, 10, 300, 30), $"BATTLE : {battleCount}");

        GUI.Label(new Rect(320, 40, 300, 30), $"P1 WINS : {player1Wins}");

        GUI.Label(new Rect(320, 70, 300, 30), $"P2 WINS : {player2Wins}");

        GUI.Label(new Rect(320, 100, 300, 30),$"REQUIRED WINS : {requiredWins}");

        GUI.color = Color.white;
    }

#endif
}

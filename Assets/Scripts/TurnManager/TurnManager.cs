using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // ========================================
    // Turn Settings
    // ========================================
    [Header("Turn Settings")]

    // 1ターンの制限時間
    [Tooltip("1ターンの制限時間")]
    [SerializeField]
    private float turnTimeLimit = 20.0f;

    // 現在のターン数
    private int turnCount = 0;

    // ========================================
    // Player
    // ========================================

    // Player1の参照
    private Player player1;

    // Player2の参照
    private Player player2;

    // 現在Attack側になっているPlayer
    private Player currentAttackPlayer;

    private Player battleWinner;

    // ========================================
    // Turn
    // ========================================

    // 現在のターン残り時間
    private float turnTimer;

    // Battleが開始されているか
    private bool isBattleStarted = false;

    // Battleが一時停止中か
    private bool isBattlePaused = false;

    // 先攻Playerが決定済みかfalseの場合はBattle開始時にランダムで決定する
    private bool hasFirstPlayer = false;

    // ========================================
    // State Machine
    // ========================================

    // 現在のTurn State
    private TurnBaseState currentState;

    // 現在のTurn State種類
    private TurnStateType currentStateType;

    // Battle開始State
    private TurnBattleStartState battleStartState;

    // Turn開始State
    private TurnStartState turnStartState;

    // Player操作中State
    private TurnPlayingState playingState;

    // Turn終了State
    private TurnEndState turnEndState;

    // Battle結果State
    private TurnResultState resultState;

    private TurnRepositionState repositionState;

    [Header("Reposition Settings")]
    [Tooltip("両PlayerがReadyにならなかった場合に、一人目が自動でRepositionを終了するまでの時間")]
    [SerializeField] private float repositionTimeLimit = 10.0f;

    [Tooltip("一人目がRepositionを終了してから、二人目が終了するまでの時間")]
    [SerializeField] private float secondPlayerExitDelay = 2.0f;

    [Tooltip("何ターンごとにRepositionを行うか")]
    [SerializeField] private int repositionTurnInterval = 10;

    private void Awake()
    {
        // Singleton重複防止
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Turn State生成
        battleStartState = new TurnBattleStartState();
        turnStartState = new TurnStartState();
        playingState = new TurnPlayingState();
        turnEndState = new TurnEndState();
        resultState = new TurnResultState();
        repositionState = new TurnRepositionState();
    }

    private void Update()
    {
        // Battle中ではない
        if (!isBattleStarted)
        {
            return;
        }

        // Battle一時停止中
        if (isBattlePaused)
        {
            return;
        }

        // 現在Stateを更新
        currentState?.UpdateState(this);
    }

    // ========================================
    // Player Register
    // ========================================

    // PlayerをTurnManagerへ登録する
    public void RegisterPlayer(Player player)
    {
        if (player == null)
        {
            Debug.LogWarning("TurnManager : 登録Playerがnullです。");

            return;
        }

        // Player1が未登録ならPlayer1へ
        if (player1 == null)
        {
            player1 = player;

            player1.SetRepositionColor(player1.TeamColor);

            CheckBattleReady();

            return;
        }

        // Player2が未登録ならPlayer2へ
        if (player2 == null)
        {
            player2 = player;

            player2.SetRepositionColor(player2.TeamColor);

            CheckBattleReady();

            return;
        }

        // すでに同じPlayerが登録済みの場合
        if (player1 == player || player2 == player)
        {
            return;
        }

        Debug.LogWarning("TurnManager : Playerはすでに2人登録されています。");
    }

    // Playerの登録を解除する
    public void UnregisterPlayer(Player player)
    {
        if (player == null)
        {
            return;
        }

        if (player1 == player)
        {
            player1 = null;

            return;
        }

        if (player2 == player)
        {
            player2 = null;
        }
    }

    // ========================================
    // Battle
    // ========================================

    // Playerが2人揃ったか確認する
    private void CheckBattleReady()
    {
        // Battle開始済み
        if (isBattleStarted)
        {
            return;
        }

        // Playerが揃っていない
        if (player1 == null || player2 == null)
        {
            return;
        }

        StartBattle();
    }

    // Battle開始
    private void StartBattle()
    {
        if (isBattleStarted)
        {
            return;
        }

        // 先攻が外部指定されていなければランダムで決定
        if (!hasFirstPlayer)
        {
            DecideRandomFirstPlayer();
        }

        turnCount = 1;

        isBattleStarted = true;
        isBattlePaused = false;
        hasFirstPlayer = false;

        // BattleStart演出Stateへ
        ChangeState(TurnStateType.BattleStart);
    }

    // Battle終了
    public void EndBattle()
    {
        isBattleStarted = false;
        isBattlePaused = false;

        turnTimer = 0.0f;
    }

    // Battleを一時停止する
    public void PauseBattle()
    {
        if (!isBattleStarted)
        {
            return;
        }

        isBattlePaused = true;
    }

    // Battleの一時停止を解除する
    public void ResumeBattle()
    {
        if (!isBattleStarted)
        {
            return;
        }

        isBattlePaused = false;
    }

    // Battleをリセットする
    public void ResetBattle()
    {
        isBattleStarted = false;
        isBattlePaused = false;

        turnTimer = 0.0f;
        turnCount = 0;

        currentAttackPlayer = null;

        hasFirstPlayer = false;

        battleWinner = null;

        currentState = null;
    }

    public void StartNextBattle(Player firstPlayer)
    {
        // ========================================
        // 前Battle終了
        // ========================================

        ResetBattle();

        // ========================================
        // 先攻設定
        // ========================================

        if (firstPlayer != player1 && firstPlayer != player2) 
        {
            Debug.LogWarning("TurnManager : " + "先攻Playerが不正です。");

            return;
        }

        SetFirstPlayer(firstPlayer);

        // ========================================
        // 次Battle開始
        // ========================================

        StartBattle();
    }

    // ========================================
    // First Player
    // ========================================

    // ランダムで先攻Playerを決定する
    private void DecideRandomFirstPlayer()
    {
        currentAttackPlayer =
            Random.Range(0, 2) == 0
                ? player1
                : player2;

        hasFirstPlayer = true;
    }

    // 外部から先攻Playerを指定するBattle開始前のみ使用可能
    public void SetFirstPlayer(Player player)
    {
        if (isBattleStarted)
        {
            Debug.LogWarning("Battle開始後は先攻Playerを変更できません。");

            return;
        }

        // 登録されているPlayerのみ指定可能
        if (player != player1 && player != player2)
        {
            Debug.LogWarning("TurnManagerに登録されていないPlayerです。");

            return;
        }

        currentAttackPlayer = player;

        hasFirstPlayer = true;
    }

    // 先攻指定を解除する次回Battle開始時はランダムになる
    public void ClearFirstPlayer()
    {
        if (isBattleStarted)
        {
            return;
        }

        currentAttackPlayer = null;

        hasFirstPlayer = false;
    }

    // ========================================
    // State Machine
    // ========================================

    // TurnManagerのStateを変更する
    public void ChangeState(TurnStateType stateType)
    {
        // 現在State終了
        currentState?.Exit(this);

        // 次Stateを設定
        switch (stateType)
        {
            case TurnStateType.BattleStart:
                currentState = battleStartState;
                break;

            case TurnStateType.TurnStart:
                currentState = turnStartState;
                break;

            case TurnStateType.Playing:
                currentState = playingState;
                break;

            case TurnStateType.TurnEnd:
                currentState = turnEndState;
                break;

            case TurnStateType.Result:
                currentState = resultState;
                break;
                
            case TurnStateType.Reposition:
                currentState = repositionState;
                break;
        }

        currentStateType = stateType;

        // 次State開始
        currentState?.Enter(this);
    }

    // ========================================
    // Turn
    // ========================================

    // Turnタイマーを初期化する
    public void ResetTurnTimer()
    {
        turnTimer = turnTimeLimit;
    }

    // Turnタイマーを更新する
    public void UpdateTurnTimer()
    {
        turnTimer -= Time.deltaTime;

        if (turnTimer < 0.0f)
        {
            turnTimer = 0.0f;
        }

        TurnUI.Instance.SetTimeText(((int)turnTimer).ToString());
    }

    // Attack側Playerが行動を完了した時に呼ぶ
    public void ConsumeTurn(Player player)
    {
        if (!isBattleStarted)
        {
            return;
        }

        // Playing State以外ではTurn終了しない
        if (currentStateType != TurnStateType.Playing)
        {
            return;
        }

        // Attack側以外からの要求は無視
        if (player != currentAttackPlayer)
        {
            return;
        }

        ChangeState(TurnStateType.TurnEnd);
    }

    // 次のTurnへ進める
    public void NextTurn()
    {
        // TurnManager側のAttack Player交代
        ChangeAttackPlayer();

        // Turn数を増やす
        turnCount++;
    }

    // Attack切り替え
    private void ChangeAttackPlayer()
    {
        if (currentAttackPlayer == player1)
        {
            currentAttackPlayer = player2;
        }
        else
        {
            currentAttackPlayer = player1;
        }
    }

    // Repositionが必要な場合はRepositionへ遷移する
    public void ProceedNextTurn()
    {
        // Attack Player交代 + Turn数加算
        NextTurn();

        // Repositionを行うTurn
        if (IsRepositionTurn())
        {
            ChangeState(TurnStateType.Reposition);
            return;
        }

        // 通常のTurn開始
        ChangeState(TurnStateType.TurnStart);
    }

    public void RequestEndReposition(Player player)
    {
        if (currentStateType != TurnStateType.Reposition)
        {
            return;
        }

        repositionState.RequestEnd(player, this);
    }

    public void RequestEndResult(Player player)
    {
        if (currentStateType != TurnStateType.Result)
        {
            return;
        }

        resultState.RequestEnd(player, this);
    }

    // ========================================
    // Getter / Check
    // ========================================

    // 現在のTurn Stateを取得する
    public TurnStateType GetCurrentStateType()
    {
        return currentStateType;
    }

    // 現在のTurn残り時間を取得する
    public float GetTurnTimer()
    {
        return turnTimer;
    }

    // Turnタイマーを指定時間に設定する
    public void SetTurnTimer(float time)
    {
        turnTimer = time;
    }

    // 現在のターン数を取得する
    public int GetTurnCount()
    {
        return turnCount;
    }

    public float GetRepositionTimeLimit()
    {
        return repositionTimeLimit;
    }

    public float GetSecondPlayerExitDelay()
    {
        return secondPlayerExitDelay;
    }

    // 現在のターンがRepositionを行うターンか確認する
    public bool IsRepositionTurn()
    {
        if (repositionTurnInterval <= 0)
        {
            return false;
        }

        return (turnCount - 1) % repositionTurnInterval == 0;
    }

    // Battle中か
    public bool IsBattleStarted()
    {
        return isBattleStarted;
    }

    // Battleが一時停止中か
    public bool IsBattlePaused()
    {
        return isBattlePaused;
    }

    // 指定PlayerがAttack側か
    public bool IsAttackPlayer(Player player)
    {
        if (!isBattleStarted)
        {
            return false;
        }

        if (currentStateType != TurnStateType.Playing)
        {
            return false;
        }

        return currentAttackPlayer == player;
    }

    // Player1を取得する
    public Player GetPlayer1()
    {
        return player1;
    }

    // Player2を取得する
    public Player GetPlayer2()
    {
        return player2;
    }

    // 現在Attack側のPlayerを取得する
    public Player GetAttackPlayer()
    {
        return currentAttackPlayer;
    }

    // 現在Defend側のPlayerを取得する
    public Player GetDefendPlayer()
    {
        if (currentAttackPlayer == player1)
        {
            return player2;
        }
        if (currentAttackPlayer == player2)
        {
            return player1;
        }
        return null;
    }

    // Debug表示用Player名を取得する
    public string GetPlayerName(Player player)
    {
        if (player == player1)
        {
            return "Player1";
        }

        if (player == player2)
        {
            return "Player2";
        }

        return "None";
    }

    public void SetBattleWinner(Player player)
    {
        battleWinner = player;
    }

    public Player GetBattleWinner()
    {
        return battleWinner;
    }

#if UNITY_EDITOR

    private void OnGUI()
    {
        GUI.color = Color.black;

        float timeLimit = 0.0f;

        switch (currentStateType)
        {
            case TurnStateType.Playing:
                timeLimit = turnTimeLimit;
                break;

            case TurnStateType.Reposition:
                timeLimit = repositionTimeLimit;
                break;
        }

        GUI.Label(new Rect(10, 10, 300, 30), $"TURN : {turnCount}");
        GUI.Label(new Rect(10, 40, 300, 30), $"TIME : {turnTimer:F1} / {timeLimit:F1}");
        GUI.Label(new Rect(10, 70, 300, 30), $"ATTACK : {GetPlayerName(currentAttackPlayer)}");
        GUI.Label(new Rect(10, 100, 300, 30), $"STATE : {currentStateType}");

        GUI.color = Color.white;
    }

#endif
}

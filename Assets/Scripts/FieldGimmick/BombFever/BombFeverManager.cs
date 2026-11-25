using UnityEngine;

// Bomb Feverの開始・進行を管理する
public sealed class BombFeverManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private BombFeverSettings settings;

    [Header("References")]
    [SerializeField]
    private BombSpawner bombSpawner;

    private float nextFeverTimer;
    private int nextFeverTurn;
    private int lastObservedTurn;
    private float feverDuration;
    private float feverRemainingTime;
    private float nextDropTimer;
    private bool wasBattleStarted;
    private bool isFeverActive;
    private bool isBattlePrepared;
    private TurnManager observedTurnManager;

    private void Start()
    {
        if (bombSpawner.Initialize())
            return;

        Debug.LogError("[BombFeverManager] BombSpawnerの初期化に失敗しました。", this);
        enabled = false;
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager == null)
            return;

        if (observedTurnManager != turnManager)
        {
            HandleTurnManagerChanged(turnManager);
            return;
        }

        UpdateBattleState(turnManager);

        if (!CanUpdateFever(turnManager))
            return;

        if (!isBattlePrepared)
        {
            TryPrepareForBattle(turnManager.GetTurnCount());
            return;
        }

        if (isFeverActive)
        {
            UpdateFever(turnManager.GetTurnCount());
            return;
        }

        UpdateFeverTrigger(turnManager);
    }

    private bool CanUpdateFever(TurnManager turnManager)
    {
        if (!turnManager.IsBattleStarted())
            return false;

        if (turnManager.IsBattlePaused())
            return false;

        if (Time.timeScale <= 0.0f)
            return false;

        return turnManager.GetCurrentStateType() == TurnStateType.Playing;
    }

    private void HandleTurnManagerChanged(TurnManager turnManager)
    {
        observedTurnManager = turnManager;
        ResetBattle();
        wasBattleStarted = false;
        lastObservedTurn = 0;
    }

    // Battleの開始・終了とターン数のリセットを検出する
    private void UpdateBattleState(TurnManager turnManager)
    {
        bool isBattleStarted = turnManager.IsBattleStarted();
        int currentTurn = turnManager.GetTurnCount();
        bool battleStartedNow = isBattleStarted && !wasBattleStarted;
        bool turnCountReset = isBattleStarted && lastObservedTurn > 0 && currentTurn < lastObservedTurn;

        if (battleStartedNow || turnCountReset)
        {
            ResetBattle();
        }
        else if (!isBattleStarted && wasBattleStarted)
        {
            ResetBattle();
        }

        lastObservedTurn = currentTurn;
        wasBattleStarted = isBattleStarted;
    }

    // Fieldが生成されるまで次フレーム以降も取得を試みる
    private void TryPrepareForBattle(int currentTurn)
    {
        if (!bombSpawner.RefreshField())
            return;

        ResetFeverState();
        ScheduleNextFever(currentTurn);
        isBattlePrepared = true;
    }

    private void UpdateFeverTrigger(TurnManager turnManager)
    {
        switch (settings.TriggerMode)
        {
            case BombFeverTriggerMode.ElapsedTime:
                UpdateTimeTrigger();
                break;

            case BombFeverTriggerMode.TurnCount:
                UpdateTurnTrigger(turnManager.GetTurnCount());
                break;
        }
    }

    private void ScheduleNextFever(int currentTurn)
    {
        switch (settings.TriggerMode)
        {
            case BombFeverTriggerMode.ElapsedTime:
                nextFeverTimer = settings.GetRandomStartIntervalSeconds();
                break;

            case BombFeverTriggerMode.TurnCount:
                nextFeverTurn = currentTurn + settings.GetRandomStartIntervalTurns();
                break;
        }
    }

    private void UpdateTimeTrigger()
    {
        nextFeverTimer -= Time.deltaTime;

        if (nextFeverTimer <= 0.0f)
            StartFever();
    }

    private void UpdateTurnTrigger(int currentTurn)
    {
        if (currentTurn >= nextFeverTurn)
            StartFever();
    }

    private void StartFever()
    {
        if (isFeverActive)
            return;

        isFeverActive = true;
        feverDuration = settings.GetRandomFeverDuration();
        feverRemainingTime = feverDuration;
        nextDropTimer = 0.0f;
    }

    private void UpdateFever(int currentTurn)
    {
        feverRemainingTime -= Time.deltaTime;
        nextDropTimer -= Time.deltaTime;

        if (nextDropTimer <= 0.0f)
        {
            bombSpawner.TrySpawnBomb();
            nextDropTimer = GetNextDropInterval();
        }

        if (feverRemainingTime <= 0.0f)
            FinishFever(currentTurn);
    }

    private float GetNextDropInterval()
    {
        float baseInterval = settings.GetRandomDropInterval();
        float progress = GetFeverProgress();
        float intervalScale = Mathf.Lerp(settings.StartDropIntervalScale, settings.EndDropIntervalScale, progress);
        return baseInterval * intervalScale;
    }

    private float GetFeverProgress()
    {
        if (feverDuration <= 0.0f)
            return 1.0f;

        return Mathf.Clamp01(1.0f - feverRemainingTime / feverDuration);
    }

    private void FinishFever(int currentTurn)
    {
        ResetFeverState();
        ScheduleNextFever(currentTurn);
    }

    private void ResetBattle()
    {
        ResetFeverState();
        bombSpawner.ResetBattle();
        nextFeverTimer = 0.0f;
        nextFeverTurn = 0;
        isBattlePrepared = false;
    }

    private void ResetFeverState()
    {
        isFeverActive = false;
        feverDuration = 0.0f;
        feverRemainingTime = 0.0f;
        nextDropTimer = 0.0f;
    }
}
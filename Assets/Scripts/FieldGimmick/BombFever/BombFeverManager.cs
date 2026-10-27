using System.Collections.Generic;
using UnityEngine;

// Bomb Feverの開始・進行と、フィールド上へのBomb生成を管理する
public sealed class BombFeverManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private BombFeverSettings settings;

    [Header("Field Reference")]
    [SerializeField]
    private Transform fieldRoot;

    [Header("Pool Reference")]
    [SerializeField]
    private BombPool bombPool;

    private readonly List<FieldTile>                candidateTiles      = new List<FieldTile>();
    private readonly HashSet<FieldTile>             bombOccupiedTiles   = new HashSet<FieldTile>();
    private readonly Dictionary<Bomb, FieldTile>    bombTiles           = new Dictionary<Bomb, FieldTile>();

    private FieldTile[] fieldTiles;

    private float   nextFeverTimer;
    private int     nextFeverTurn;
    private int     lastObservedTurn;

    private float   feverDuration;
    private float   feverRemainingTime;
    private float   nextDropTimer;

    private bool    wasBattleStarted;
    private bool    isFeverActive;

    private void Awake()
    {
        if (!ValidateReferences())
            enabled = false;
    }

    private void Start()
    {
        CacheFieldTiles();
        bombPool.Initialize(settings.BombSettings.BombPrefab);
    }

    private void Update()
    {
        TurnManager turnManager = TurnManager.Instance;

        if (turnManager == null)
            return;

        UpdateBattleState(turnManager);

        if (!CanUpdateFever(turnManager))
            return;

        if (isFeverActive)
        {
            UpdateFever();
            return;
        }

        UpdateFeverTrigger(turnManager);
    }

    private bool ValidateReferences()
    {
        if (settings == null)
        {
            Debug.LogError("[BombFeverManager] BombFeverSettingsが設定されていません。", this);
            return false;
        }

        if (!settings.IsValid)
        {
            Debug.LogError("[BombFeverManager] BombFeverSettingsの設定値が不正です。", this);
            return false;
        }

        if (fieldRoot == null)
        {
            Debug.LogError("[BombFeverManager] Field Rootが設定されていません。", this);
            return false;
        }

        if (bombPool == null)
        {
            Debug.LogError("[BombFeverManager] Bomb Poolが設定されていません。", this);
            return false;
        }

        return true;
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

    // Battle開始・終了・切り替えを監視
    private void UpdateBattleState(TurnManager turnManager)
    {
        bool isBattleStarted    = turnManager.IsBattleStarted();
        int currentTurn         = turnManager.GetTurnCount();

        bool battleStartedNow   = isBattleStarted && !wasBattleStarted;
        bool turnCountReset     = isBattleStarted && lastObservedTurn > 0 && currentTurn < lastObservedTurn;

        if (battleStartedNow || turnCountReset)
        {
            PrepareForBattle(currentTurn);
        }
        else if (!isBattleStarted && wasBattleStarted)
        {
            ResetFever();
        }

        lastObservedTurn = currentTurn;
        wasBattleStarted = isBattleStarted;
    }

    // 新しいBattle用にFever状態を初期化
    private void PrepareForBattle(int currentTurn)
    {
        CacheFieldTiles();

        isFeverActive       = false;
        feverDuration       = 0.0f;
        feverRemainingTime  = 0.0f;
        nextDropTimer       = 0.0f;

        ScheduleNextFever(currentTurn);
    }

    private void CacheFieldTiles()
    {
        fieldTiles = fieldRoot.GetComponentsInChildren<FieldTile>(true);

        if (fieldTiles.Length == 0)
            Debug.LogWarning("[BombFeverManager] Field Root以下にFieldTileがありません。", this);
    }

    // Fever開始方式に応じて現在の待機処理を更新
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

    // 次回Feverの開始条件をランダムに決める
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

        isFeverActive       = true;
        feverDuration       = settings.GetRandomFeverDuration();
        feverRemainingTime  = feverDuration;

        // Fever開始時は最初のBombをすぐ生成
        nextDropTimer = 0.0f;
    }

    private void UpdateFever()
    {
        feverRemainingTime  -= Time.deltaTime;
        nextDropTimer       -= Time.deltaTime;

        if (nextDropTimer <= 0.0f)
        {
            TrySpawnBomb();
            nextDropTimer = GetNextDropInterval();
        }

        if (feverRemainingTime <= 0.0f)
            FinishFever();
    }

    // Fever終盤になるほどBomb生成間隔を短くする
    private float GetNextDropInterval()
    {
        float baseInterval  = settings.GetRandomDropInterval();
        float progress      = GetFeverProgress();
        float intervalScale = Mathf.Lerp(settings.StartDropIntervalScale, settings.EndDropIntervalScale, progress);

        return baseInterval * intervalScale;
    }

    private float GetFeverProgress()
    {
        if (feverDuration <= 0.0f)
            return 1.0f;

        return Mathf.Clamp01(1.0f - feverRemainingTime / feverDuration);
    }

    private void FinishFever()
    {
        isFeverActive       = false;
        feverDuration       = 0.0f;
        feverRemainingTime  = 0.0f;
        nextDropTimer       = 0.0f;

        int currentTurn     = TurnManager.Instance != null ? TurnManager.Instance.GetTurnCount() : 0;

        ScheduleNextFever(currentTurn);
    }

    // 使用可能なマスを選択し、PoolからBombを取得して開始する
    private bool TrySpawnBomb()
    {
        if (!TryGetSpawnTile(out FieldTile selectedTile))
            return false;

        Bomb bomb = GetBombFromPool(selectedTile);

        if (bomb == null)
            return false;

        RegisterBomb(bomb, selectedTile);
        StartBomb(bomb, selectedTile);

        return true;
    }

    // Bombを生成可能なFieldTileからランダムに1マス選ぶ
    private bool TryGetSpawnTile(out FieldTile selectedTile)
    {
        selectedTile = null;
        candidateTiles.Clear();

        foreach (FieldTile tile in fieldTiles)
        {
            if (tile == null || !tile.isActiveAndEnabled)
                continue;

            if (!settings.AllowMultipleBombsOnSameTile && bombOccupiedTiles.Contains(tile))
                continue;

            candidateTiles.Add(tile);
        }

        if (candidateTiles.Count == 0)
            return false;

        selectedTile = candidateTiles[Random.Range(0, candidateTiles.Count)];

        return true;
    }

    // PoolからBombを取得し、今回の生成位置へ移動する
    private Bomb GetBombFromPool(FieldTile selectedTile)
    {
        Bomb bomb = bombPool.Get();

        if (bomb == null)
            return null;

        Vector3 landingPosition = GetLandingPosition(selectedTile);
        Vector3 spawnPosition   = GetSpawnPosition(landingPosition);

        bomb.transform.SetParent(null);
        bomb.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);

        return bomb;
    }

    private Vector3 GetLandingPosition(FieldTile tile)
    {
        return tile.transform.position + Vector3.up * settings.BombSettings.SpawnHeight;
    }

    private Vector3 GetSpawnPosition(Vector3 landingPosition)
    {
        if (settings.SpawnMode == BombSpawnMode.Falling)
            return landingPosition + Vector3.up * settings.FallingHeight;

        return landingPosition;
    }

    private void StartBomb(Bomb bomb, FieldTile selectedTile)
    {
        BombSettings bombSettings = settings.BombSettings;

        if (settings.SpawnMode == BombSpawnMode.Falling)
        {
            Vector3 landingPosition = GetLandingPosition(selectedTile);
            bomb.InitializeFalling(bombSettings, landingPosition, settings.FallingSpeed);
            return;
        }

        bomb.Initialize(bombSettings);
    }

    // Bombと生成先FieldTileの対応を記録する
    private void RegisterBomb(Bomb bomb, FieldTile tile)
    {
        bombTiles.Add(bomb, tile);
        bombOccupiedTiles.Add(tile);
        bomb.Removed += OnBombRemoved;
    }

    // BombがPoolへ戻った後に使用中マスを解放する
    private void OnBombRemoved(Bomb bomb)
    {
        bomb.Removed -= OnBombRemoved;

        if (!bombTiles.TryGetValue(bomb, out FieldTile tile))
            return;

        bombTiles.Remove(bomb);

        if (!HasBombOnTile(tile))
            bombOccupiedTiles.Remove(tile);
    }

    private bool HasBombOnTile(FieldTile tile)
    {
        foreach (FieldTile occupiedTile in bombTiles.Values)
        {
            if (occupiedTile == tile)
                return true;
        }

        return false;
    }

    private void ResetFever()
    {
        isFeverActive       = false;
        nextFeverTimer      = 0.0f;
        nextFeverTurn       = 0;
        feverDuration       = 0.0f;
        feverRemainingTime  = 0.0f;
        nextDropTimer       = 0.0f;
        lastObservedTurn    = 0;
    }

    private void OnDestroy()
    {
        foreach (Bomb bomb in bombTiles.Keys)
        {
            if (bomb != null)
                bomb.Removed -= OnBombRemoved;
        }

        bombTiles.Clear();
        bombOccupiedTiles.Clear();
        candidateTiles.Clear();
    }
}
using UnityEngine;

// フィールド上で発生するBomb Feverの設定を保持
[CreateAssetMenu(fileName = "BombFeverSettings", menuName = "Field Gimmick/Bomb Fever Settings")]
public sealed class BombFeverSettings : ScriptableObject
{
    [Header("Bomb")]
    [SerializeField]
    private BombSettings bombSettings;

    [Header("Trigger Mode")]
    [SerializeField]
    private BombFeverTriggerMode triggerMode = BombFeverTriggerMode.ElapsedTime;

    [Header("Time Trigger")]
    [SerializeField, Min(0.0f)]
    private float minimumStartIntervalSeconds = 10.0f;

    [SerializeField, Min(0.0f)]
    private float maximumStartIntervalSeconds = 20.0f;

    [Header("Turn Trigger")]
    [SerializeField, Min(1)]
    private int minimumStartIntervalTurns = 3;

    [SerializeField, Min(1)]
    private int maximumStartIntervalTurns = 6;

    [Header("Fever Duration")]
    [SerializeField, Min(0.1f)]
    private float minimumFeverDuration = 5.0f;

    [SerializeField, Min(0.1f)]
    private float maximumFeverDuration = 8.0f;

    [Header("Bomb Drop Interval")]
    [SerializeField, Min(0.1f)]
    private float minimumDropInterval = 0.5f;

    [SerializeField, Min(0.1f)]
    private float maximumDropInterval = 1.5f;

    [Header("Drop Acceleration")]
    [SerializeField, Min(0.01f)]
    private float startDropIntervalScale    = 1.0f;

    [SerializeField, Min(0.01f)]
    private float endDropIntervalScale      = 0.3f;

    [Header("Spawn Mode")]
    [SerializeField]
    private BombSpawnMode spawnMode = BombSpawnMode.DirectSpawn;

    [SerializeField, Min(0.0f)]
    private float fallingHeight = 10.0f;

    [SerializeField, Min(0.01f)]
    private float fallingSpeed  = 8.0f;

    [Header("Tile Rules")]
    [SerializeField]
    private bool allowMultipleBombsOnSameTile = false;

    public BombSettings BombSettings => bombSettings;

    public BombFeverTriggerMode TriggerMode => triggerMode;

    public BombSpawnMode SpawnMode => spawnMode;

    public float FallingHeight => fallingHeight;

    public float FallingSpeed => fallingSpeed;

    public bool AllowMultipleBombsOnSameTile => allowMultipleBombsOnSameTile;

    public float StartDropIntervalScale => startDropIntervalScale;

    public float EndDropIntervalScale => endDropIntervalScale;

    public float GetRandomStartIntervalSeconds()
    {
        return GetRandomRange(minimumStartIntervalSeconds, maximumStartIntervalSeconds);
    }

    public int GetRandomStartIntervalTurns()
    {
        int minimum = Mathf.Min(minimumStartIntervalTurns, maximumStartIntervalTurns);
        int maximum = Mathf.Max(minimumStartIntervalTurns, maximumStartIntervalTurns);

        return Random.Range(minimum, maximum + 1);
    }

    public float GetRandomFeverDuration()
    {
        return GetRandomRange(minimumFeverDuration, maximumFeverDuration);
    }

    public float GetRandomDropInterval()
    {
        return GetRandomRange(minimumDropInterval, maximumDropInterval);
    }

    public bool IsValid =>  bombSettings != null && IsNonNegativeFinite(minimumStartIntervalSeconds) && IsNonNegativeFinite(maximumStartIntervalSeconds) && minimumStartIntervalTurns >= 1 &&
                            maximumStartIntervalTurns >= 1 && IsPositiveFinite(minimumFeverDuration) && IsPositiveFinite(maximumFeverDuration) && IsPositiveFinite(minimumDropInterval) &&
                            IsPositiveFinite(maximumDropInterval) && IsPositiveFinite(startDropIntervalScale) && IsPositiveFinite(endDropIntervalScale) && IsNonNegativeFinite(fallingHeight) &&
                            IsPositiveFinite(fallingSpeed);

    private static float GetRandomRange(float valueA, float valueB)
    {
        float minimum = Mathf.Min(valueA, valueB);
        float maximum = Mathf.Max(valueA, valueB);

        return Random.Range(minimum, maximum);
    }

    private static bool IsNonNegativeFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0.0f;
    }

    private static bool IsPositiveFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0.0f;
    }
}
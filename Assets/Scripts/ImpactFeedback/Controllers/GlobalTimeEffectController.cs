using System.Collections.Generic;
using UnityEngine;

// ゲーム全体のHitStopとSlowを管理
public sealed class GlobalTimeEffectController : MonoBehaviour
{
    public static GlobalTimeEffectController Instance { get; private set; }

    private sealed class SlowState
    {
        public float TimeScale { get; }
        public float RemainingDuration { get; set; }

        public SlowState(float timeScale, float duration)
        {
            TimeScale = timeScale;
            RemainingDuration = duration;
        }
    }

    private readonly List<SlowState> slowStates = new();

    private float hitStopRemainingDuration;
    private float baseFixedDeltaTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        baseFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void Update()
    {
        UpdateHitStop();
        UpdateSlows();
        ApplyTimeScale();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        RestoreTimeScale();
        Instance = null;
    }

    // HitStopが重なった場合は残り時間の長い方を採用する
    public void RequestHitStop(float duration)
    {
        if (duration <= 0.0f)
            return;

        hitStopRemainingDuration = Mathf.Max(hitStopRemainingDuration, duration);
        ApplyTimeScale();
    }

    // Slowは個別に保持し、有効なSlowの中で最も低い倍率を適用する
    public void RequestSlow(float timeScale, float duration)
    {
        if (timeScale <= 0.0f || timeScale >= 1.0f || duration <= 0.0f)
            return;

        slowStates.Add(new SlowState(timeScale, duration));
        ApplyTimeScale();
    }

    private void UpdateHitStop()
    {
        if (hitStopRemainingDuration <= 0.0f)
            return;

        hitStopRemainingDuration = Mathf.Max(0.0f, hitStopRemainingDuration - Time.unscaledDeltaTime);
    }

    private void UpdateSlows()
    {
        for (int i = slowStates.Count - 1; i >= 0; i--)
        {
            SlowState slowState = slowStates[i];
            slowState.RemainingDuration -= Time.unscaledDeltaTime;

            if (slowState.RemainingDuration <= 0.0f)
                slowStates.RemoveAt(i);
        }
    }

    private void ApplyTimeScale()
    {
        if (hitStopRemainingDuration > 0.0f)
        {
            SetTimeScale(0.0f);
            return;
        }

        float timeScale = 1.0f;

        foreach (SlowState slowState in slowStates)
            timeScale = Mathf.Min(timeScale, slowState.TimeScale);

        SetTimeScale(timeScale);
    }

    private void SetTimeScale(float timeScale)
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = timeScale > 0.0f ? baseFixedDeltaTime * timeScale : baseFixedDeltaTime;
    }

    private void RestoreTimeScale()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = baseFixedDeltaTime;
    }
}
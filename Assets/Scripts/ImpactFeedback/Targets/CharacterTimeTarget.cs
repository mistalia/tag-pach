using UnityEngine;

// キャラクター単体のHitStop時間を管理
public sealed class CharacterTimeTarget : MonoBehaviour, ICharacterTimeTarget
{
    private float remainingStopDuration;

    public bool IsStopped => remainingStopDuration > 0.0f;

    private void Update()
    {
        if (remainingStopDuration <= 0.0f)
            return;

        remainingStopDuration = Mathf.Max(0.0f, remainingStopDuration - Time.unscaledDeltaTime);
    }

    // Stopが重なった場合は現在の残り時間と新しい時間の長い方を採用する
    public void RequestStop(float duration)
    {
        remainingStopDuration = Mathf.Max(remainingStopDuration, duration);
    }
}
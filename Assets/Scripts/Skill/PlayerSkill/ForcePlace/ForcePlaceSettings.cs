using UnityEngine;

[CreateAssetMenu(
    fileName = "ForcePlaceSettings",
    menuName = "Skills/ForcePlace Settings")]
public class ForcePlaceSettings : ScriptableObject
{
    [Header("ステート時間")]
    [SerializeField, Min(0f)]
    [Tooltip("チャージ時間")]
    private float chargeDuration = 1f;

    [SerializeField, Min(0f)]
    [Tooltip("スキル発動後の待機時間")]
    private float endDuration = 0.5f;

    [Header("攻撃範囲")]
    [SerializeField, Min(0.01f)]
    [Tooltip("円形の当たり判定の半径")]
    private float hitRadius = 1f;

    [SerializeField, Min(0f)]
    [Tooltip("プレイヤーから円の中心までの前方距離")]
    private float hitForwardOffset = 1.5f;

    [Header("クールタイム")]
    [SerializeField, Min(0f)]
    [Tooltip("発動してから再使用できるまでの時間。単位は秒")]
    private float cooldownTime = 8f;

    public float ChargeDuration => chargeDuration;
    public float EndDuration => endDuration;
    
    public float HitRadius => hitRadius;
    public float HitForwardOffset => hitForwardOffset;

    public float CooldownTime => cooldownTime;

    public bool IsValid =>
        IsFinite(chargeDuration) && chargeDuration >= 0f &&
        IsFinite(endDuration) && endDuration >= 0f &&
        IsFinite(hitRadius) && hitRadius > 0f &&
        IsFinite(hitForwardOffset) && hitForwardOffset >= 0f &&
        IsFinite(cooldownTime) && cooldownTime >= 0f;

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
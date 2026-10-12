using UnityEngine;

[CreateAssetMenu(
    fileName = "SmashSettings",
    menuName = "Skills/Smash Settings")]
public class SmashSettings : ScriptableObject
{
    [Header("攻撃範囲")]
    [SerializeField, Min(0f)]
    [Tooltip("相手に届く距離。プレイヤー同士の水平距離で判定する")]
    private float range = 3f;

    [Header("ノックバック")]
    [SerializeField, Min(0f)]
    [Tooltip("相手を吹き飛ばす距離")]
    private float knockbackDistance = 2f;

    [SerializeField, Min(0.01f)]
    [Tooltip("吹き飛ばす速さ。単位はUnityの距離単位/秒")]
    private float knockbackSpeed = 8f;

    [Header("スタン")]
    [SerializeField, Min(0f)]
    [Tooltip("命中した瞬間から数える操作不能時間。この間の追加被弾は無視する")]
    private float knockbackDuration = 0.25f;

    [Header("クールタイム")]
    [SerializeField, Min(0f)]
    [Tooltip("発動してから再使用できるまでの時間。単位は秒")]
    private float cooldownTime = 2f;

    public float Range => range;
    public float KnockbackDistance => knockbackDistance;
    public float KnockbackSpeed => knockbackSpeed;
    public float StunDuration => knockbackDuration;
    public float CooldownTime => cooldownTime;

    // 移行中のSmashHitReceiverが参照するため、切り替え完了まで残す。
    public float KnockbackDuration => knockbackDuration;

    public bool IsValid =>
        IsFinite(range) && range > 0f &&
        IsFinite(knockbackDistance) && knockbackDistance >= 0f &&
        IsFinite(knockbackSpeed) && knockbackSpeed > 0f &&
        IsFinite(knockbackDuration) && knockbackDuration >= 0f &&
        IsFinite(cooldownTime) && cooldownTime >= 0f;

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
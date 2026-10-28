using UnityEngine;

[CreateAssetMenu(
    fileName = "ForcePlaceSettings",
    menuName = "Skills/ForcePlace Settings")]
public class ForcePlaceSettings : ScriptableObject
{
    [Header("有効時間")]
    [SerializeField, Min(0.01f)]
    [Tooltip("発動後、相手への接触で強制配置できる時間。単位は秒")]
    private float activeDuration = 3f;

    [Header("クールタイム")]
    [SerializeField, Min(0f)]
    [Tooltip("発動してから再使用できるまでの時間。単位は秒")]
    private float cooldownTime = 8f;

    public float ActiveDuration => activeDuration;
    public float CooldownTime => cooldownTime;

    public bool IsValid =>
        IsFinite(activeDuration) && activeDuration > 0f &&
        IsFinite(cooldownTime) && cooldownTime >= 0f;

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
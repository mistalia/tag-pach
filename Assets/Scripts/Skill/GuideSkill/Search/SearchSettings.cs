using UnityEngine;

[CreateAssetMenu(
    fileName = "SearchSettings",
    menuName = "Skills/Search Settings")]
public class SearchSettings : ScriptableObject
{
    [Header("サーチ範囲")]
    [SerializeField, Min(0.01f)]
    private float range = 5f;

    [SerializeField, Min(0.01f)]
    private float expansionSpeed = 5f;

    [Header("駒の表示時間")]
    [SerializeField, Min(0f)]
    private float revealDuration = 2f;

    [SerializeField, Min(0f)]
    private float fadeDuration = 1f;

    [Header("クールタイム")]
    [SerializeField, Min(0f)]
    private float cooldownTime = 8f;

    public float Range => range;
    public float ExpansionSpeed => expansionSpeed;
    public float RevealDuration => revealDuration;
    public float FadeDuration => fadeDuration;
    public float CooldownTime => cooldownTime;
}
using UnityEngine;

// ”š’e1ŒÂ‚Ì¶¬E”š”­‚ÉŽg—p‚·‚éÝ’è‚ð•ÛŽ
[CreateAssetMenu(fileName = "BombSettings", menuName = "Field Gimmick/Bomb Settings")]
public sealed class BombSettings : ScriptableObject
{
    [Header("Spawn")]
    [SerializeField]
    private GameObject bombPrefab;

    [SerializeField]
    private float spawnHeight       = 0.5f;

    [Header("Explosion")]
    [SerializeField, Min(0.0f)]
    private float explosionDelay    = 3.0f;

    [SerializeField, Min(0.0f)]
    private float explosionRadius   = 3.0f;

    [Header("Knockback")]
    [SerializeField, Min(0.0f)]
    private float knockbackDistance = 2.0f;

    [SerializeField, Min(0.01f)]
    private float knockbackSpeed    = 8.0f;

    [SerializeField, Min(0.0f)]
    private float stunDuration      = 0.25f;

    [Header("Visual")]
    [SerializeField]
    private Color startColor    = Color.gray;

    [SerializeField]
    private Color warningColor  = Color.red;

    public GameObject BombPrefab => bombPrefab;

    public float SpawnHeight => spawnHeight;

    public float ExplosionDelay => explosionDelay;

    public float ExplosionRadius => explosionRadius;

    public float KnockbackDistance => knockbackDistance;

    public float KnockbackSpeed => knockbackSpeed;

    public float StunDuration => stunDuration;

    public Color StartColor => startColor;

    public Color WarningColor => warningColor;

    public bool IsValid =>  bombPrefab != null && IsFinite(spawnHeight) && IsFinite(explosionDelay) && explosionDelay >= 0.0f && IsFinite(explosionRadius) && explosionRadius > 0.0f && 
                            IsFinite(knockbackDistance) && knockbackDistance >= 0.0f && IsFinite(knockbackSpeed) && knockbackSpeed > 0.0f && IsFinite(stunDuration) &&
                            stunDuration >= 0.0f;

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
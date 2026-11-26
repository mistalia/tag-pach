using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 爆弾1個の飛来・起爆演出・爆発処理を管理する
public sealed class Bomb : MonoBehaviour
{
    private const float MinDirectionSqrMagnitude = 0.0001f;

    [Header("Visual References")]
    [SerializeField]
    private Renderer bombRenderer;
    [SerializeField] ParticleSystem trailEffect;
    [SerializeField] ParticleSystem waitingEffect;
    [SerializeField] string explosionEffectKey = "BombExplosion";

    [SerializeField]
    private Transform explosionRangeVisual;

    private BombPool pool;
    private BombSettings settings;
    private MaterialPropertyBlock propertyBlock;
    private int colorPropertyId;
    private bool hasColorProperty;
    private bool hasNotifiedRemoval;

    public event Action<Bomb> Removed;

    public void SetPool(BombPool ownerPool)
    {
        pool = ownerPool;
    }

    public void InitializeFlying(BombSettings bombSettings, Vector3 landingPosition, float arcHeight, float flightDuration)
    {
        if (!TryInitialize(bombSettings))
            return;

        if (!IsNonNegativeFinite(arcHeight))
        {
            Debug.LogError("[Bomb] Arc Heightは0以上の値にしてください。", this);
            Release();
            return;
        }

        if (!IsPositiveFinite(flightDuration))
        {
            Debug.LogError("[Bomb] Flight Durationは0より大きい値にしてください。", this);
            Release();
            return;
        }

        StartCoroutine(FlyingSequence(landingPosition, arcHeight, flightDuration));

        if (trailEffect != null && !trailEffect.isPlaying)
            trailEffect.Play();
    }

    public void ResetForPool()
    {
        StopAllCoroutines();

        settings = null;
        hasColorProperty = false;
        hasNotifiedRemoval = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (explosionRangeVisual != null)
            explosionRangeVisual.localScale = Vector3.zero;
    }

    private bool TryInitialize(BombSettings bombSettings)
    {
        StopAllCoroutines();

        settings = bombSettings;
        hasColorProperty = false;
        hasNotifiedRemoval = false;

        // 全てのエフェクトの状態を停止させる
        if (trailEffect != null && trailEffect.isPlaying)
            trailEffect.Stop();
        if (waitingEffect != null && waitingEffect.isPlaying)
            waitingEffect.Stop();

        if (settings == null || !settings.IsValid)
        {
            Debug.LogError("[Bomb] BombSettingsが正しく設定されていません。", this);
            Release();
            return false;
        }

        if (!TryInitializeBombVisual())
        {
            Release();
            return false;
        }

        InitializeExplosionRangeVisual();
        return true;
    }

    private bool TryInitializeBombVisual()
    {
        if (bombRenderer == null)
        {
            Debug.LogError("[Bomb] Bomb Rendererが設定されていません。", this);
            return false;
        }

        Material material = bombRenderer.sharedMaterial;
        if (material == null)
        {
            Debug.LogError("[Bomb] Materialが設定されていません。", this);
            return false;
        }

        if (material.HasProperty("_BaseColor"))
        {
            colorPropertyId = Shader.PropertyToID("_BaseColor");
        }
        else if (material.HasProperty("_Color"))
        {
            colorPropertyId = Shader.PropertyToID("_Color");
        }
        else
        {
            Debug.LogError("[Bomb] Materialに対応する色プロパティがありません。", this);
            return false;
        }

        hasColorProperty = true;

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        ApplyColor(settings.StartColor);
        return true;
    }

    private void InitializeExplosionRangeVisual()
    {
        if (explosionRangeVisual == null)
        {
            Debug.LogWarning("[Bomb] Explosion Range Visualが設定されていません。", this);
            return;
        }

        explosionRangeVisual.localScale = Vector3.zero;
    }

    private IEnumerator FlyingSequence(Vector3 landingPosition, float arcHeight, float flightDuration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0.0f;

        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / flightDuration);
            Vector3 position = Vector3.Lerp(startPosition, landingPosition, progress);
            float height = 4.0f * arcHeight * progress * (1.0f - progress);

            position.y += height;
            transform.position = position;

            yield return null;
        }

        transform.position = landingPosition;

        if (trailEffect != null && trailEffect.isPlaying)
            trailEffect.Stop();
        if (waitingEffect != null && !waitingEffect.isPlaying)
            waitingEffect.Play();

        yield return ExplosionSequence();
    }

    private IEnumerator ExplosionSequence()
    {
        float explosionDelay = Mathf.Max(0.0f, settings.ExplosionDelay);

        if (explosionDelay <= 0.0f)
        {
            ApplyWarningVisual();
            Explode();
            yield break;
        }

        float elapsedTime = 0.0f;

        while (elapsedTime < explosionDelay)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / explosionDelay);
            UpdateWarningVisual(progress);

            yield return null;
        }

        ApplyWarningVisual();
        Explode();
    }

    private void UpdateWarningVisual(float progress)
    {
        UpdateBombColor(progress);
        UpdateExplosionRangeVisual(progress);
    }

    private void ApplyWarningVisual()
    {
        ApplyColor(settings.WarningColor);
        UpdateExplosionRangeVisual(1.0f);
    }

    private void UpdateBombColor(float progress)
    {
        Color currentColor = Color.Lerp(settings.StartColor, settings.WarningColor, Mathf.Clamp01(progress));
        ApplyColor(currentColor);
    }

    private void UpdateExplosionRangeVisual(float progress)
    {
        if (explosionRangeVisual == null)
            return;

        float diameter = settings.ExplosionRadius * 2.0f;
        explosionRangeVisual.localScale = Vector3.one * diameter * Mathf.Clamp01(progress);
    }

    private void ApplyColor(Color color)
    {
        if (bombRenderer == null || propertyBlock == null || !hasColorProperty)
            return;

        bombRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(colorPropertyId, color);
        bombRenderer.SetPropertyBlock(propertyBlock);
    }

    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, settings.ExplosionRadius);

        // 複数のColliderを持つPlayerへノックバックを重複適用しないため
        HashSet<Player> hitPlayers = new HashSet<Player>();

        EffectManager.Instance?.PlayEffect(explosionEffectKey, transform.position, Quaternion.identity);

        foreach (Collider hitCollider in hitColliders)
        {
            Player targetPlayer = hitCollider.GetComponentInParent<Player>();

            if (targetPlayer == null || !targetPlayer.isActiveAndEnabled || !hitPlayers.Add(targetPlayer))
                continue;

            Vector3 knockbackDirection = GetKnockbackDirection(targetPlayer);
            targetPlayer.AddKnockback(knockbackDirection, settings.KnockbackDistance, settings.KnockbackSpeed, settings.StunDuration);
        }

        Release();
    }

    private Vector3 GetKnockbackDirection(Player targetPlayer)
    {
        Vector3 direction = targetPlayer.transform.position - transform.position;
        direction.y = 0.0f;

        if (direction.sqrMagnitude >= MinDirectionSqrMagnitude)
            return direction.normalized;

        direction = targetPlayer.transform.forward;
        direction.y = 0.0f;

        if (direction.sqrMagnitude >= MinDirectionSqrMagnitude)
            return direction.normalized;

        return Vector3.forward;
    }

    private void Release()
    {
        NotifyRemoved();

        if (pool != null)
        {
            pool.Return(this);
            return;
        }

        Destroy(gameObject);
    }

    private void NotifyRemoved()
    {
        if (hasNotifiedRemoval)
            return;

        hasNotifiedRemoval = true;
        Removed?.Invoke(this);
        Removed = null;
    }

    private void OnDestroy()
    {
        NotifyRemoved();
    }

    private static bool IsPositiveFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > 0.0f;
    }

    private static bool IsNonNegativeFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0.0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (settings == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, settings.ExplosionRadius);
    }
}
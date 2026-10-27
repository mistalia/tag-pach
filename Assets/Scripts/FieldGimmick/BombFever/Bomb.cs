using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 爆弾1個の落下・起爆演出・爆発処理を管理する
public sealed class Bomb : MonoBehaviour
{
    private const float MinDirectionSqrMagnitude = 0.0001f;

    [Header("Visual References")]
    [SerializeField]
    private Renderer bombRenderer;

    [SerializeField]
    private Transform explosionRangeVisual;

    private BombPool                pool;
    private BombSettings            settings;
    private MaterialPropertyBlock   propertyBlock;

    private int colorPropertyId;

    private bool hasColorProperty;
    private bool hasNotifiedRemoval;

    // Bombが爆発またはPoolへ戻ることを通知する
    public event Action<Bomb> Removed;

    // このBombを管理するPoolを設定する
    public void SetPool(BombPool ownerPool)
    {
        pool = ownerPool;
    }

    // マス上へ直接生成されたBombの動作を開始する
    public void Initialize(BombSettings bombSettings)
    {
        if (!TryInitialize(bombSettings))
            return;

        StartCoroutine(ExplosionSequence());
    }

    // 上空から指定位置へ落下するBombの動作を開始する
    public void InitializeFalling(BombSettings bombSettings, Vector3 landingPosition, float fallingSpeed)
    {
        if (!TryInitialize(bombSettings))
            return;

        if (!IsPositiveFinite(fallingSpeed))
        {
            Debug.LogError("[Bomb] Falling Speedは0より大きい値にしてください。", this);
            Release();
            return;
        }

        StartCoroutine(FallingSequence(landingPosition, fallingSpeed));
    }

    // Poolへ戻る前に実行する
    public void ResetForPool()
    {
        StopAllCoroutines();

        settings                = null;
        hasNotifiedRemoval      = false;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (explosionRangeVisual != null)
            explosionRangeVisual.localScale = Vector3.zero;
    }

    // 共通の初期化処理
    private bool TryInitialize(BombSettings bombSettings)
    {
        StopAllCoroutines();

        settings            = bombSettings;
        hasNotifiedRemoval  = false;

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

    // Bomb本体の色変更に必要な情報を初期化する
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

    // 爆発範囲表示を最小サイズから開始する
    private void InitializeExplosionRangeVisual()
    {
        if (explosionRangeVisual == null)
        {
            Debug.LogWarning("[Bomb] Explosion Range Visualが設定されていません。", this);
            return;
        }

        explosionRangeVisual.localScale = Vector3.zero;
    }

    // 指定位置まで落下し、着地後に起爆処理を開始する
    private IEnumerator FallingSequence(Vector3 landingPosition, float fallingSpeed)
    {
        while ((transform.position - landingPosition).sqrMagnitude > MinDirectionSqrMagnitude)
        {
            transform.position = Vector3.MoveTowards(transform.position, landingPosition, fallingSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = landingPosition;

        yield return ExplosionSequence();
    }

    // 爆発までの時間に合わせて警告演出を進める
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

    // 起爆までの進行度に応じて色と爆発範囲を更新
    private void UpdateWarningVisual(float progress)
    {
        UpdateBombColor(progress);
        UpdateExplosionRangeVisual(progress);
    }

    // 爆発直前の表示へ確実に合わせる
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

        float diameter                  = settings.ExplosionRadius * 2.0f;
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

    // 爆発範囲内のPlayerへノックバックを与える
    private void Explode()
    {
        Collider[] hitColliders     = Physics.OverlapSphere(transform.position, settings.ExplosionRadius);
        HashSet<Player> hitPlayers  = new HashSet<Player>();

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

    // BombからPlayerへ向かう吹き飛ばし方向を取得
    private Vector3 GetKnockbackDirection(Player targetPlayer)
    {
        Vector3 direction   = targetPlayer.transform.position - transform.position;
        direction.y         = 0.0f;

        if (direction.sqrMagnitude >= MinDirectionSqrMagnitude)
            return direction;

        direction   = targetPlayer.transform.forward;
        direction.y = 0.0f;

        if (direction.sqrMagnitude >= MinDirectionSqrMagnitude)
            return direction;

        return Vector3.forward;
    }

    // Managerへ消滅を通知し、BombをPoolへ戻す
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

    // Bombの消滅を一度だけ通知
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

    // Sceneビューで実際の爆発範囲を確認
    private void OnDrawGizmosSelected()
    {
        if (settings == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, settings.ExplosionRadius);
    }
}
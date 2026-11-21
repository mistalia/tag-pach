using System.Collections;
using UnityEngine;

// 元のMaterialを維持したまま白いMaterialを追加・解除して点滅
public sealed class CharacterFlashTarget : MonoBehaviour, ICharacterFlashTarget
{
    [SerializeField]
    private GameObject rendererRoot;

    private Renderer[] targetRenderers;

    private Material[][] originalMaterials;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (rendererRoot == null)
        {
            Debug.LogError("[CharacterFlashTarget] Renderer Rootが設定されていません。", this);
            enabled = false;
            return;
        }

        targetRenderers = rendererRoot.GetComponentsInChildren<Renderer>(true);

        if (targetRenderers.Length == 0)
        {
            Debug.LogError("[CharacterFlashTarget] Renderer Root配下にRendererが見つかりません。", this);
            enabled = false;
            return;
        }

        CacheOriginalMaterials();
    }

    public void RequestFlash(CharacterFlashRequest request)
    {
        if (!enabled || request.FlashMaterial == null || request.FlashCount <= 0)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        RestoreOriginalMaterials();
        flashCoroutine = StartCoroutine(FlashCoroutine(request));
    }

    private IEnumerator FlashCoroutine(CharacterFlashRequest request)
    {
        for (int i = 0; i < request.FlashCount; i++)
        {
            ApplyFlashMaterial(request.FlashMaterial);

            if (request.VisibleDuration > 0.0f)
                yield return new WaitForSecondsRealtime(request.VisibleDuration);
            else
                yield return null;

            RestoreOriginalMaterials();

            if (i < request.FlashCount - 1)
            {
                if (request.HiddenDuration > 0.0f)
                    yield return new WaitForSecondsRealtime(request.HiddenDuration);
                else
                    yield return null;
            }
        }

        RestoreOriginalMaterials();
        flashCoroutine = null;
    }

    // Rendererごとの元Material構成を保存する
    private void CacheOriginalMaterials()
    {
        originalMaterials = new Material[targetRenderers.Length][];

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];

            if (targetRenderer != null)
                originalMaterials[i] = targetRenderer.sharedMaterials;
        }
    }

    // 元Material配列の末尾へFlash Materialを追加する
    private void ApplyFlashMaterial(Material flashMaterial)
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];
            Material[] materials = originalMaterials[i];

            if (targetRenderer == null || materials == null)
                continue;

            Material[] flashMaterials = new Material[materials.Length + 1];

            for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                flashMaterials[materialIndex] = materials[materialIndex];

            flashMaterials[flashMaterials.Length - 1] = flashMaterial;
            targetRenderer.sharedMaterials = flashMaterials;
        }
    }

    // Flash終了後は保存していた元Material構成へ戻す
    private void RestoreOriginalMaterials()
    {
        if (originalMaterials == null)
            return;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];
            Material[] materials = originalMaterials[i];

            if (targetRenderer != null && materials != null)
                targetRenderer.sharedMaterials = materials;
        }
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = null;
        RestoreOriginalMaterials();
    }

    private void OnDestroy()
    {
        RestoreOriginalMaterials();
    }
}
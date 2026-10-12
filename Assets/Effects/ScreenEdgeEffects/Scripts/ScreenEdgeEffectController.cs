
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 演出設定を読み込み、共通画面のエフェクトを再生する。
/// 同じRenderer Featureを操作するControllerは1つにする。
/// </summary>
[DisallowMultipleComponent]
public class ScreenEdgeEffectController : MonoBehaviour
{
    [Header("描画先")]

    [Tooltip("対象のFull Screen Passが登録されているRenderer。")]
    [SerializeField]
    private UniversalRendererData rendererData;

    [Tooltip("操作するFull Screen PassのName。")]
    [SerializeField]
    private string featureName = "ScreenEdgeFog";

    [Header("開始時の再生")]

    [Tooltip("有効になったときに再生する演出。未設定なら再生しない。")]
    [SerializeField]
    private ScreenEffectPreset initialPreset;

    // 再生状態を区別する。
    private enum PlaybackState
    {
        Idle,
        OneShot,
        FadeIn,
        Holding,
        FadeOut
    }

    private PlaybackState state = PlaybackState.Idle;

    private FullScreenPassRendererFeature targetFeature;
    private Material originalMaterial;
    private bool originalActive;

    private Material runtimeMaterial;
    private ScreenEffectPreset currentPreset;
    private int intensityId;

    private float elapsed;
    private float currentIntensity;
    private float fadeOutStartIntensity;

    // 再生番号。古い停止指示で新しい演出を消さないために使う。
    // 0は「有効な再生がない」ことを表す。
    private long nextPlaybackId;
    private long currentPlaybackId;

    private void OnEnable()
    {
        if (rendererData == null)
        {
            Debug.LogError("Renderer Dataを設定してください。", this);
            return;
        }

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is FullScreenPassRendererFeature fullScreen
                && feature.name == featureName)
            {
                targetFeature = fullScreen;
                break;
            }
        }

        if (targetFeature == null)
        {
            Debug.LogError(
                $"Full Screen Pass「{featureName}」が見つかりません。",
                this);
            return;
        }

        originalMaterial = targetFeature.passMaterial;
        originalActive = targetFeature.isActive;

        // 再生指示がない間は描画しない。
        targetFeature.SetActive(false);

        if (initialPreset != null)
        {
            Play(initialPreset);
        }
    }

    /// <summary>
    /// 演出を再生し、停止に使用する再生番号を返す。
    /// 失敗した場合は0を返す。
    /// </summary>
    public long Play(ScreenEffectPreset preset)
    {
        if (!isActiveAndEnabled || targetFeature == null)
        {
            Debug.LogWarning(
                "有効なControllerと描画先が必要です。", this);
            return 0;
        }

        if (preset == null || preset.EffectMaterial == null)
        {
            Debug.LogError(
                "演出設定とEffect Materialを設定してください。",
                this);
            return 0;
        }

        if (string.IsNullOrWhiteSpace(preset.IntensityProperty)
            || !preset.EffectMaterial.HasProperty(
                preset.IntensityProperty))
        {
            Debug.LogError(
                "Intensity PropertyがMaterialに存在しません。",
                this);
            return 0;
        }

        // 同じ継続演出の再呼び出しでは、表示を維持する。
        // 停止フェード中なら、新たに再生し直す。
        if (currentPreset == preset
            && preset.Mode == ScreenEffectPreset.PlaybackMode.Continuous
            && (state == PlaybackState.FadeIn
                || state == PlaybackState.Holding))
        {
            return currentPlaybackId;
        }

        // 設定の確認が済んでから、古い演出を置き換える。
        FinishPlayback();

        currentPreset = preset;
        intensityId = Shader.PropertyToID(
            preset.IntensityProperty);

        // 演出元のMaterialは変更せず、実行用コピーを使う。
        runtimeMaterial = new Material(preset.EffectMaterial);
        runtimeMaterial.name =
            preset.EffectMaterial.name + " (Runtime)";
        runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;

        targetFeature.passMaterial = runtimeMaterial;

        currentPlaybackId = ++nextPlaybackId;
        elapsed = 0f;

        if (preset.Mode == ScreenEffectPreset.PlaybackMode.OneShot)
        {
            state = PlaybackState.OneShot;
            ApplyIntensity(
                preset.MaxIntensity * preset.EvaluateIntensity(0f));
        }
        else if (preset.FadeInDuration > 0f)
        {
            state = PlaybackState.FadeIn;
            ApplyIntensity(
                preset.MaxIntensity * preset.EvaluateFadeIn(0f));
        }
        else
        {
            state = PlaybackState.Holding;
            ApplyIntensity(preset.MaxIntensity);
        }

        targetFeature.SetActive(true);
        return currentPlaybackId;
    }

    /// <summary>
    /// 指定した再生番号の演出を停止する。
    /// 古い番号や0を渡しても、現在の演出には影響しない。
    /// </summary>
    public void Stop(long playbackId)
    {
        if (playbackId == 0
            || playbackId != currentPlaybackId
            || state == PlaybackState.Idle
            || state == PlaybackState.FadeOut)
        {
            return;
        }

        if (currentPreset.FadeOutDuration <= 0f)
        {
            FinishPlayback();
            return;
        }

        // 途中の強さからフェードするので、
        // 停止指示で急に最大の強さへ跳ね上がらない。
        fadeOutStartIntensity = currentIntensity;
        elapsed = 0f;
        state = PlaybackState.FadeOut;
    }

    private void Update()
    {
        if (state == PlaybackState.Idle)
        {
            return;
        }

        // 再生中に設定などが失われた場合は終了する。
        if (currentPreset == null
            || runtimeMaterial == null
            || targetFeature == null)
        {
            FinishPlayback();
            return;
        }

        // timeScaleが0なら時間も停止する。
        float delta = Time.deltaTime;
        if (delta <= 0f)
        {
            return;
        }

        elapsed += delta;

        switch (state)
        {
            case PlaybackState.OneShot:
                {
                    float duration =
                        Mathf.Max(0.01f, currentPreset.Duration);

                    if (elapsed >= duration)
                    {
                        // 自動終了では追加の停止フェードを行わない。
                        // 終了までの変化はIntensity Curveで設定する。
                        FinishPlayback();
                        return;
                    }

                    ApplyIntensity(
                        currentPreset.MaxIntensity
                        * currentPreset.EvaluateIntensity(
                            elapsed / duration));
                    break;
                }

            case PlaybackState.FadeIn:
                {
                    float duration =
                        Mathf.Max(0f, currentPreset.FadeInDuration);

                    if (duration <= 0f || elapsed >= duration)
                    {
                        state = PlaybackState.Holding;
                        ApplyIntensity(currentPreset.MaxIntensity);
                    }
                    else
                    {
                        ApplyIntensity(
                            currentPreset.MaxIntensity
                            * currentPreset.EvaluateFadeIn(
                                elapsed / duration));
                    }
                    break;
                }

            case PlaybackState.Holding:
                ApplyIntensity(currentPreset.MaxIntensity);
                break;

            case PlaybackState.FadeOut:
                {
                    float duration =
                        Mathf.Max(0f, currentPreset.FadeOutDuration);

                    if (duration <= 0f || elapsed >= duration)
                    {
                        FinishPlayback();
                        return;
                    }

                    ApplyIntensity(
                        fadeOutStartIntensity
                        * currentPreset.EvaluateFadeOut(
                            elapsed / duration));
                    break;
                }
        }
    }

    private void ApplyIntensity(float value)
    {
        currentIntensity = Mathf.Clamp01(value);
        runtimeMaterial.SetFloat(intensityId, currentIntensity);
    }

    /// <summary>
    /// 表示を止め、実行用Materialを片付ける。
    /// 別演出への置き換え時にも使用する。
    /// </summary>
    private void FinishPlayback()
    {
        if (targetFeature != null)
        {
            targetFeature.SetActive(false);
            targetFeature.passMaterial = originalMaterial;
        }

        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }

        runtimeMaterial = null;
        currentPreset = null;
        currentPlaybackId = 0;
        currentIntensity = 0f;
        elapsed = 0f;
        state = PlaybackState.Idle;
    }

    private void OnDisable()
    {
        FinishPlayback();

        // Controllerが無効になったら、開始前の状態に戻す。
        if (targetFeature != null)
        {
            targetFeature.passMaterial = originalMaterial;
            targetFeature.SetActive(originalActive);
        }

        targetFeature = null;
        originalMaterial = null;
    }
}
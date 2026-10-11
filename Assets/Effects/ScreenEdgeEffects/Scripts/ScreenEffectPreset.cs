
using UnityEngine;

/// <summary>
/// 画面エフェクト1種類分の設定を保存するアセット。
/// GameObjectには追加せず、Project欄で作成して使用する。
/// 再生処理はController側が担当する。
/// </summary>
[CreateAssetMenu(
    fileName = "ScreenEffectPreset",
    menuName = "Effects/Screen Effect Preset")]
public class ScreenEffectPreset : ScriptableObject
{
    /// <summary>
    /// OneShot：指定時間で自動終了。
    /// Continuous：停止を指示するまで表示を維持。
    /// </summary>
    public enum PlaybackMode
    {
        OneShot,
        Continuous
    }

    [Header("描画の設定")]

    [Tooltip("この演出に使用するMaterial。見た目固有の設定はMaterialで行う。")]
    [SerializeField]
    private Material effectMaterial;

    [Tooltip("Shaderが強さを受け取るプロパティ名。Shader側の名前と合わせる。")]
    [SerializeField]
    private string intensityProperty = "_Intensity";

    [Tooltip("演出の最大の強さ。Curveの値に、この値を掛けて使用する。")]
    [SerializeField, Range(0f, 1f)]
    private float maxIntensity = 1f;

    [Header("再生方法")]

    [SerializeField]
    private PlaybackMode playbackMode = PlaybackMode.OneShot;

    [Header("一回再生の設定")]

    [Tooltip("OneShotの再生時間。秒単位。")]
    [SerializeField, Min(0.01f)]
    private float duration = 1f;

    [Tooltip(
        "横軸は再生の進行度0〜1、縦軸は強さ0〜1。"
        + "開始から終了までの強さを設定する。")]
    [SerializeField]
    private AnimationCurve intensityCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.15f, 1f),
        new Keyframe(1f, 0f)
    );

    [Header("継続表示の設定")]

    [Tooltip("Continuousで最大の強さになるまでの秒数。0なら即座に表示する。")]
    [SerializeField, Min(0f)]
    private float fadeInDuration = 0.1f;

    [Tooltip("Continuousの表示開始時の変化。横軸・縦軸ともに0〜1。")]
    [SerializeField]
    private AnimationCurve fadeInCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("停止時の設定")]

    [Tooltip(
        "停止指示から消えるまでの秒数。0なら即座に消す。"
        + "別の演出への置き換え時は、この時間を待たずに切り替える。")]
    [SerializeField, Min(0f)]
    private float fadeOutDuration = 0.1f;

    [Tooltip(
        "停止時の強さの倍率。横軸は進行度0〜1。"
        + "縦軸は1から0へ下がる形にする。")]
    [SerializeField]
    private AnimationCurve fadeOutCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    // 他のスクリプトは、プロパティから設定を読み取る。
    // 再生中の経過時間などは、このアセットには保存しない。
    public Material EffectMaterial => effectMaterial;
    public string IntensityProperty => intensityProperty;
    public float MaxIntensity => maxIntensity;
    public PlaybackMode Mode => playbackMode;
    public float Duration => duration;
    public float FadeInDuration => fadeInDuration;
    public float FadeOutDuration => fadeOutDuration;

    /// <summary>
    /// 一回再生の進行度から、0〜1の強さを取得する。
    /// </summary>
    public float EvaluateIntensity(float progress)
    {
        return Mathf.Clamp01(
            intensityCurve.Evaluate(Mathf.Clamp01(progress)));
    }

    /// <summary>
    /// 継続表示のフェードインに使用する。
    /// </summary>
    public float EvaluateFadeIn(float progress)
    {
        return Mathf.Clamp01(
            fadeInCurve.Evaluate(Mathf.Clamp01(progress)));
    }

    /// <summary>
    /// 停止時のフェードアウトに使用する。
    /// 停止指示を受けた時点の強さに、この倍率を掛ける。
    /// </summary>
    public float EvaluateFadeOut(float progress)
    {
        return Mathf.Clamp01(
            fadeOutCurve.Evaluate(Mathf.Clamp01(progress)));
    }
}
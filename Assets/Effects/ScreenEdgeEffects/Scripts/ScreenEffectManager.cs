
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キーと演出設定を登録し、キー指定による再生を受け付ける。
/// 描画と時間の管理はScreenEdgeEffectControllerに任せる。
/// </summary>
[DisallowMultipleComponent]
public class ScreenEffectManager : MonoBehaviour
{
    // 既存のSEManagerと同じように、Instanceから呼び出せる。
    // Scene内で使用するManagerは1つにする。
    public static ScreenEffectManager Instance { get; private set; }

    [Serializable]
    private class EffectEntry
    {
        [Tooltip("呼び出しに使用する名前。重複しないようにする。")]
        [SerializeField]
        private string key;

        [Tooltip("このキーで再生する演出設定。")]
        [SerializeField]
        private ScreenEffectPreset preset;

        public string Key => key;
        public ScreenEffectPreset Preset => preset;
    }

    [Header("再生先")]

    [SerializeField]
    private ScreenEdgeEffectController controller;

    [Header("演出の登録")]

    [Tooltip("再生キーと演出設定の組み合わせ。再生前に設定する。")]
    [SerializeField]
    private List<EffectEntry> effects = new List<EffectEntry>();

    // 登録されたキーから、演出設定を検索するための一覧。
    // 大文字・小文字を区別する。
    private readonly Dictionary<string, ScreenEffectPreset> presetLookup =
        new Dictionary<string, ScreenEffectPreset>(StringComparer.Ordinal);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "ScreenEffectManagerが重複しています。"
                + "Scene内では1つにしてください。",
                this);

            // 同じGameObjectにあるControllerなどは削除しない。
            Destroy(this);
            return;
        }

        Instance = this;

        // Inspectorの登録内容を、起動時に検索用一覧へ移す。
        foreach (EffectEntry entry in effects)
        {
            if (entry == null
                || string.IsNullOrWhiteSpace(entry.Key)
                || entry.Preset == null)
            {
                Debug.LogError(
                    "演出のKeyまたはPresetが未設定です。",
                    this);
                continue;
            }

            if (presetLookup.ContainsKey(entry.Key))
            {
                Debug.LogError(
                    $"演出キー「{entry.Key}」が重複しています。",
                    this);
                continue;
            }

            presetLookup.Add(entry.Key, entry.Preset);
        }
    }

    /// <summary>
    /// 登録キーで演出を再生する。
    /// 停止に使用する再生番号を返す。失敗した場合は0。
    /// </summary>
    public long Play(string key)
    {
        if (controller == null)
        {
            Debug.LogError(
                "ScreenEffectManagerのControllerを設定してください。",
                this);
            return 0;
        }

        if (string.IsNullOrWhiteSpace(key)
            || !presetLookup.TryGetValue(key, out var preset))
        {
            Debug.LogError(
                $"演出キー「{key}」が登録されていません。",
                this);
            return 0;
        }

        return controller.Play(preset);
    }

    /// <summary>
    /// このManagerから受け取った再生番号で停止する。
    /// 古い番号が新しい再生を止めないよう、Controllerが判定する。
    /// </summary>
    public void Stop(long playbackId)
    {
        if (controller != null)
        {
            controller.Stop(playbackId);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
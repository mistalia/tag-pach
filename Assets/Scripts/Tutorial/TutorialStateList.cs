using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialEntry
{
    [SerializeField] private string key;
    [SerializeField] private TutorialState sourceState;
    [SerializeField] private string nextStateKey;
    [NonSerialized] private TutorialState runtimeState;

    public string Key => key;
    public TutorialState RuntimeState => runtimeState;
    public string NextStateKey => nextStateKey;

    public bool CreateRuntimeCopy()
    {
        if (sourceState == null)
        {
            return false;
        }
        runtimeState = UnityEngine.Object.Instantiate(sourceState);
        runtimeState.name = $"{sourceState.name} (Runtime)";
        runtimeState.hideFlags = HideFlags.DontSave;
        
        runtimeState.SetKey(key, nextStateKey);

        return true;
    }

    public void DestroyRuntimeCopy()
    {
        if (runtimeState == null)
        {
            return;
        }

        runtimeState.SetKey(null, null);

        UnityEngine.Object.Destroy(runtimeState);
        runtimeState = null;
    }
}


[Serializable]
public class TutorialStateList
{
    [SerializeField]
    private List<TutorialEntry> stateList = new();
    private readonly Dictionary<string, TutorialEntry> runtimeStateByKey = new(StringComparer.Ordinal);

    public void CreateRunTimeCopies()
    {
        DestroyRunTimeCopies();
        foreach (var entry in stateList)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }
            if (!entry.CreateRuntimeCopy())
            {
                Debug.Log($"TutorialManager: ステートが設定されていません ({entry.Key})");
                continue;
            }
            if (!runtimeStateByKey.TryAdd(entry.Key, entry))
            {
                Debug.Log($"TutorialManager: 同じキーのステートが複数存在します ({entry.Key})");
                continue;
            }
        }
    }

    public bool TryGetRuntimeState(string key, out TutorialState runtimeState)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            runtimeState = null;
            return false;
        }
        if (!runtimeStateByKey.TryGetValue(key, out var entry))
        {
            runtimeState = null;
            return false;
        }
        runtimeState = entry.RuntimeState;
        return true;
    }

    public void DestroyRunTimeCopies()
    {
        foreach (var entry in runtimeStateByKey.Values)
        {
            entry.DestroyRuntimeCopy();
        }
        runtimeStateByKey.Clear();
    }
}
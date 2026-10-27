using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public sealed class LABPStateEntry
{
    [SerializeField] private string key;
    [SerializeField] private LABPState sourceState;
    [NonSerialized] private LABPState runtimeState;
    public string Key => key;
    public LABPState SourceState => sourceState;
    public LABPState RuntimeState => runtimeState;
    public bool CreateRuntimeCopy()
    {
        if(sourceState == null)
        {
            runtimeState = null;
            return false;
        }
        runtimeState = UnityEngine.Object.Instantiate(sourceState);
        runtimeState.name = $"{sourceState.name} (Runtime)";
        runtimeState.hideFlags = HideFlags.DontSave;
        return true;
    }
    public void DestroyRuntimeCopy()
    {
        if(runtimeState == null)
        {
            return;
        }
        UnityEngine.Object.Destroy(runtimeState);
        runtimeState = null;
    }
}

[Serializable]
public sealed class LABPStateList
{
    [SerializeField]
    private List<LABPStateEntry> stateList = new();
    
    private readonly Dictionary<string, LABPState> runtimeStateByKey = new(StringComparer.Ordinal);

    public void CreateRunTimeCopies()
    {
        DestroyRunTimeCopies();
        foreach (var entry in stateList)
        {
            if(entry == null || string.IsNullOrWhiteSpace(entry.Key))
            {
                continue;
            }
            if(!entry.CreateRuntimeCopy())
            {
                Debug.Log($"LABPDevice: ステートが設定されていません ({entry.Key})");
                continue;
            }
            if(!runtimeStateByKey.TryAdd(entry.Key, entry.RuntimeState))
            {
                Debug.Log($"LABPDevice: キーが重複しています ({entry.Key})");
                entry.DestroyRuntimeCopy();
            }
        }
    }

    public bool TryGetRuntimeState(string key, out LABPState runtimeState)
    {
        if(string.IsNullOrWhiteSpace(key))
        {
            runtimeState = null;
            return false;
        }
        return runtimeStateByKey.TryGetValue(key, out runtimeState);
    }

    public void DestroyRunTimeCopies()
    {
        foreach (var entry in stateList)
        {
            entry?.DestroyRuntimeCopy();
        }
        runtimeStateByKey.Clear();
    }
}

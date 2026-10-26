using UnityEngine;
using System.Collections.Generic;

public class RandomRemoveEvent : MonoBehaviour
{
    
    public static RandomRemoveEvent Instance { get; private set; }

    [Header("Event Settings")]
    [SerializeField, Tooltip("飽和時リムーブ割合")] private float removeRate = 0.7f;
    [SerializeField, Tooltip("イベント時リムーブ割合")] private float eventRemoveRate = 0.2f;
    [SerializeField, Tooltip("イベント発生時の基礎確率")] private float eventProbability = 0.1f;
    [SerializeField, Tooltip("イベント発生時の確率上昇値")] private float eventProbabilityIncrease = 0.05f;


    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public bool ExecuteRandomRemoveEvent()
    {
        float currentProbability = eventProbability;
        // イベント発生確率を計算
        if (Random.value < currentProbability)
        {
            // ランダムに石を削除する処理
            FieldManager.Instance.RandomRemove(Mathf.CeilToInt(eventRemoveRate * FieldManager.Instance.GetStoneCount()));

            // 確率をリセットする
            ResetEventProbability();

            return true;
        }

        return false;
    }

    public void ExecuteSaturationRemoveEvent()
    {
        // 飽和時のリムーブ処理
        FieldManager.Instance.RandomRemove(Mathf.CeilToInt(removeRate * FieldManager.Instance.GetStoneCount()));

        // 確率をリセットする
        ResetEventProbability();
    }

    public void ResetEventProbability()
    {
        // イベント発生確率をリセット
        eventProbability = 0.1f;
    }

    public void IncreaseEventProbability()
    {
        // イベント発生確率を上昇
        eventProbability += eventProbabilityIncrease;
    } 
    
    public void DecreaseEventProbability()
    {
        // イベント発生確率を減少
        eventProbability -= eventProbabilityIncrease;
    }

}

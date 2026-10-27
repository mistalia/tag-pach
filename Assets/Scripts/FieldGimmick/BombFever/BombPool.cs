using System.Collections.Generic;
using UnityEngine;

// Bombの生成と再利用を管理
public sealed class BombPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField, Min(1)]
    private int initialSize = 20;

    [SerializeField, Min(1)]
    private int maximumSize = 50;

    private readonly Queue<Bomb>    availableBombs  = new Queue<Bomb>();
    private readonly HashSet<Bomb>  activeBombs     = new HashSet<Bomb>();

    private GameObject bombPrefab;
    private bool isInitialized;

    public int ActiveCount => activeBombs.Count;
    public int AvailableCount => availableBombs.Count;

    // BombFeverManagerからPrefabを受け取り、必要数を先に生成する
    public void Initialize(GameObject prefab)
    {
        if (isInitialized)
            return;

        if (prefab == null)
        {
            Debug.LogError("[BombPool] Bomb Prefabが設定されていません。", this);
            return;
        }

        if (!prefab.TryGetComponent(out Bomb _))
        {
            Debug.LogError("[BombPool] Bomb PrefabにBombコンポーネントがありません。", prefab);
            return;
        }

        bombPrefab      = prefab;
        maximumSize     = Mathf.Max(initialSize, maximumSize);
        isInitialized   = true;

        for (int index = 0; index < initialSize; index++)
        {
            Bomb bomb = CreateBomb();
            Return(bomb);
        }
    }

    // 使用可能なBombを取得する
    public Bomb Get()
    {
        if (!isInitialized)
        {
            Debug.LogError("[BombPool] Initializeが呼ばれていません。", this);
            return null;
        }

        Bomb bomb = GetAvailableBomb();

        if (bomb == null)
            return null;

        activeBombs.Add(bomb);
        bomb.gameObject.SetActive(true);

        return bomb;
    }

    // 使用を終えたBombをPoolへ戻す
    public void Return(Bomb bomb)
    {
        if (bomb == null)
            return;

        activeBombs.Remove(bomb);

        bomb.ResetForPool();
        bomb.transform.SetParent(transform);
        bomb.gameObject.SetActive(false);

        if (!availableBombs.Contains(bomb))
            availableBombs.Enqueue(bomb);
    }

    // 現在使用中のBombをすべてPoolへ戻す
    public void ReturnAll()
    {
        Bomb[] bombs = new Bomb[activeBombs.Count];
        activeBombs.CopyTo(bombs);

        foreach (Bomb bomb in bombs)
        {
            Return(bomb);
        }
    }

    private Bomb GetAvailableBomb()
    {
        while (availableBombs.Count > 0)
        {
            Bomb bomb = availableBombs.Dequeue();

            if (bomb != null)
                return bomb;
        }

        if (activeBombs.Count >= maximumSize)
            return null;

        return CreateBomb();
    }

    private Bomb CreateBomb()
    {
        GameObject bombObject   = Instantiate(bombPrefab, transform);
        Bomb bomb               = bombObject.GetComponent<Bomb>();

        bomb.SetPool(this);
        bombObject.SetActive(false);

        return bomb;
    }
}
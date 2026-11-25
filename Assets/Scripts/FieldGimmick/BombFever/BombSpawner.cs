using System.Collections.Generic;
using UnityEngine;

// Bombの配置、発射、使用中マスを管理する
public sealed class BombSpawner : MonoBehaviour
{
    private const string GeneratedFieldsObjectName = "Generated Fields";

    [Header("Settings")]
    [SerializeField]
    private BombFeverSettings settings;

    [Header("References")]
    [SerializeField]
    private BombPool bombPool;

    private readonly List<FieldTile> candidateTiles = new List<FieldTile>();
    private readonly HashSet<FieldTile> bombOccupiedTiles = new HashSet<FieldTile>();
    private readonly Dictionary<Bomb, FieldTile> bombTiles = new Dictionary<Bomb, FieldTile>();

    private Transform fieldRoot;
    private FieldTile[] fieldTiles;
    private float fieldLeftEdge;
    private float fieldRightEdge;
    private bool isInitialized;

    public bool Initialize()
    {
        if (isInitialized)
            return true;

        if (!bombPool.Initialize(settings.BombSettings.BombPrefab))
            return false;

        isInitialized = true;
        return true;
    }

    public bool RefreshField()
    {
        fieldRoot = null;
        FindFieldRoot();
        return CacheFieldTiles();
    }

    public bool TrySpawnBomb()
    {
        if (!isInitialized)
        {
            Debug.LogError("[BombSpawner] Initializeが呼ばれていません。", this);
            return false;
        }

        if (!EnsureFieldTiles())
            return false;

        if (!TryGetSpawnTile(out FieldTile selectedTile))
            return false;

        Vector3 landingPosition = GetLandingPosition(selectedTile);
        Bomb bomb = GetBombFromPool(selectedTile, landingPosition);

        if (bomb == null)
            return false;

        RegisterBomb(bomb, selectedTile);
        StartBomb(bomb, landingPosition);
        return true;
    }

    public void ResetBattle()
    {
        ClearBombRegistrations();

        if (bombPool != null)
            bombPool.ReturnAll();

        candidateTiles.Clear();
        fieldRoot = null;
        fieldTiles = System.Array.Empty<FieldTile>();
        fieldLeftEdge = 0.0f;
        fieldRightEdge = 0.0f;
    }

    private bool EnsureFieldTiles()
    {
        if (fieldTiles != null)
        {
            foreach (FieldTile tile in fieldTiles)
            {
                if (tile != null)
                    return true;
            }
        }

        fieldRoot = null;
        FindFieldRoot();
        return CacheFieldTiles();
    }

    private void FindFieldRoot()
    {
        if (fieldRoot != null)
            return;

        FieldManager fieldManager = FindAnyObjectByType<FieldManager>();

        if (fieldManager == null)
        {
            Debug.LogError("[BombSpawner] FieldManagerが見つかりません。", this);
            return;
        }

        fieldRoot = fieldManager.transform.Find(GeneratedFieldsObjectName);

        if (fieldRoot == null)
            Debug.LogError($"[BombSpawner] {GeneratedFieldsObjectName}が見つかりません。", this);
    }

    private bool CacheFieldTiles()
    {
        if (fieldRoot == null)
        {
            fieldTiles = System.Array.Empty<FieldTile>();
            return false;
        }

        fieldTiles = fieldRoot.GetComponentsInChildren<FieldTile>(true);

        if (fieldTiles.Length == 0)
        {
            Debug.LogWarning("[BombSpawner] Field Root以下にFieldTileがありません。", this);
            return false;
        }

        return CacheFieldEdges();
    }

    private bool CacheFieldEdges()
    {
        fieldLeftEdge = float.MaxValue;
        fieldRightEdge = float.MinValue;
        bool foundValidTile = false;

        foreach (FieldTile tile in fieldTiles)
        {
            if (tile == null)
                continue;

            Vector3 localPosition = fieldRoot.InverseTransformPoint(tile.transform.position);
            fieldLeftEdge = Mathf.Min(fieldLeftEdge, localPosition.x);
            fieldRightEdge = Mathf.Max(fieldRightEdge, localPosition.x);
            foundValidTile = true;
        }

        if (!foundValidTile)
            Debug.LogWarning("[BombSpawner] 有効なFieldTileがありません。", this);

        return foundValidTile;
    }

    private bool TryGetSpawnTile(out FieldTile selectedTile)
    {
        selectedTile = null;
        candidateTiles.Clear();

        if (fieldTiles == null || fieldTiles.Length == 0)
            return false;

        foreach (FieldTile tile in fieldTiles)
        {
            if (tile == null || !tile.isActiveAndEnabled)
                continue;

            if (!settings.AllowMultipleBombsOnSameTile && bombOccupiedTiles.Contains(tile))
                continue;

            candidateTiles.Add(tile);
        }

        if (candidateTiles.Count == 0)
            return false;

        selectedTile = candidateTiles[Random.Range(0, candidateTiles.Count)];
        return true;
    }

    private Bomb GetBombFromPool(FieldTile selectedTile, Vector3 landingPosition)
    {
        Bomb bomb = bombPool.Get();

        if (bomb == null)
            return null;

        Vector3 spawnPosition = GetSpawnPosition(selectedTile, landingPosition);
        bomb.transform.SetParent(null);
        bomb.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        return bomb;
    }

    private Vector3 GetLandingPosition(FieldTile tile)
    {
        return tile.transform.position + Vector3.up * settings.BombSettings.SpawnHeight;
    }

    private Vector3 GetSpawnPosition(FieldTile tile, Vector3 landingPosition)
    {
        Vector3 tileLocalPosition = fieldRoot.InverseTransformPoint(tile.transform.position);
        float fieldCenterX = (fieldLeftEdge + fieldRightEdge) * 0.5f;
        bool isLeftSide = tileLocalPosition.x < fieldCenterX;
        float spawnLocalX = isLeftSide ? fieldLeftEdge - settings.LaunchOutsideDistance : fieldRightEdge + settings.LaunchOutsideDistance;

        Vector3 spawnLocalPosition = fieldRoot.InverseTransformPoint(landingPosition);
        spawnLocalPosition.x = spawnLocalX;
        return fieldRoot.TransformPoint(spawnLocalPosition);
    }

    private void StartBomb(Bomb bomb, Vector3 landingPosition)
    {
        bomb.InitializeFlying(settings.BombSettings, landingPosition, settings.ArcHeight, settings.FlightDuration);
    }

    private void RegisterBomb(Bomb bomb, FieldTile tile)
    {
        bombTiles.Add(bomb, tile);
        bombOccupiedTiles.Add(tile);
        bomb.Removed += OnBombRemoved;
    }

    // 同じマスに複数のBombがある場合は、最後のBombが消えるまで占有を維持する
    private void OnBombRemoved(Bomb bomb)
    {
        bomb.Removed -= OnBombRemoved;

        if (!bombTiles.TryGetValue(bomb, out FieldTile tile))
            return;

        bombTiles.Remove(bomb);

        if (!HasBombOnTile(tile))
            bombOccupiedTiles.Remove(tile);
    }

    private bool HasBombOnTile(FieldTile tile)
    {
        foreach (FieldTile occupiedTile in bombTiles.Values)
        {
            if (occupiedTile == tile)
                return true;
        }

        return false;
    }

    private void ClearBombRegistrations()
    {
        foreach (Bomb bomb in bombTiles.Keys)
        {
            if (bomb != null)
                bomb.Removed -= OnBombRemoved;
        }

        bombTiles.Clear();
        bombOccupiedTiles.Clear();
    }

    private void OnDestroy()
    {
        ClearBombRegistrations();
        candidateTiles.Clear();
    }
}
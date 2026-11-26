using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FieldEventConfig
{
    [Min(1)]
    public int turn = 1;

    public FieldRotationType eventType = FieldRotationType.Whole;
}

public enum FieldRotationType
{
    Whole,
    LeftRight,
    TopBottom,
    FourSections,
    FourCycle,
    Random,
}


public class FieldEventManager : MonoBehaviour
{
    public static FieldEventManager Instance { get; private set; }

    [Header("Match Settings")]
    [SerializeField] private bool rebuildMatches = true;
    [SerializeField, Min(1)] private int matchCount = 3;

    [SerializeField, Min(0.0f)]
    private float warningDuration = 1.0f;
    [SerializeField, Min(0.0f)]
    private float finishDelay = 0.5f;

    [Header("Rotation Animation")]
    [SerializeField, Min(0.01f)]
    private float rotationDuration = 2.0f;

    [Header("Stone Rotation Animation")]
    [SerializeField, Min(0.01f)]
    private float stoneRotationDuration = 0.5f;

    public bool IsRotating { get; private set; }

    private FieldManager fieldManager;

    private Transform generatedRoot;
    private float fieldSpacing;


    public bool IsEventPlaying { get; private set; }

    [Header("Turn Event Configs")]
    [SerializeField]
    private List<FieldEventConfig> turnEventConfigs = new List<FieldEventConfig>();

    // 実行済みイベントの管理
    private HashSet<int> executedEventIndices = new HashSet<int>();

    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
        new Vector2Int(1, -1)
    };

    private struct CellMove
    {
        public Transform Cell;
        public Vector3 TargetLocalPosition;

        public FieldTile Tile;
        public Stone Stone;

        public Vector3 StoneOffsetWorld;
        public Quaternion StoneRotationWorld;

        public Vector2Int TargetCoordinate;
    }

    private class RotationAnimationData
    {
        public Transform Cell;

        public Vector3 StartLocalPosition;
        public Quaternion StartLocalRotation;

        public Vector3 PivotLocalPosition;

        public Stone Stone;
        public Vector3 StoneStartRootPosition;
        public Quaternion StoneStartWorldRotation;

        public Vector3 TargetLocalPosition;
    }

    // ========================================
    // 初期化
    // ========================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ========================================
    // イベント呼び出し
    // ========================================
    public bool RotateRandom()
    {
        return Rotate(GetRandomRotationType());
    }

    // ========================================
    // メイン回転処理
    // ========================================

    public bool Rotate(FieldRotationType type)
    {
        if (type == FieldRotationType.Random)
        {
            return RotateRandom();
        }

        if (!Application.isPlaying || IsRotating)
        {
            return false;
        }

        if (!FindFieldReference())
        {
            return false;
        }

        if (fieldManager.IsRemoveing)
        {
            Debug.LogWarning(
                "[FieldEventManager] RandomRemove is running."
            );

            return false;
        }

        Vector2Int size = fieldManager.GetFieldSize();

        int width = size.x;
        int height = size.y;

        bool splitX =
            type == FieldRotationType.LeftRight ||
            type == FieldRotationType.FourSections ||
            type == FieldRotationType.FourCycle;

        bool splitY =
            type == FieldRotationType.TopBottom ||
            type == FieldRotationType.FourSections ||
            type == FieldRotationType.FourCycle;

        // 分割条件
        if (width <= 0 || height <= 0 ||
            (splitX && width % 2 != 0) ||
            (splitY && height % 2 != 0))
        {
            Debug.LogError(
                "[FieldEventManager] Invalid field size."
            );

            return false;
        }

        Transform root = generatedRoot;
        float spacing = fieldSpacing;

        if (root == null || Mathf.Abs(spacing) < 0.0001f)
        {
            Debug.LogError(
                "[FieldEventManager] Invalid field root."
            );

            return false;
        }

        // ========================================
        // セル情報の収集
        // ========================================

        Transform[,] cells =
            new Transform[width, height];

        int foundCount = 0;

        foreach (Transform child in root)
        {
            FieldTile tile =
                child.GetComponent<FieldTile>();

            // フィールドと障害物
            if (tile == null &&
                !child.name.StartsWith("Barrier_"))
            {
                continue;
            }

            int x;
            int y;

            if (tile != null)
            {
                Vector2Int coordinate =
                    tile.GetCoordinate();

                x = coordinate.x;
                y = coordinate.y;
            }
            else
            {
                x = Mathf.RoundToInt(
                    child.localPosition.x / spacing
                );

                y = Mathf.RoundToInt(
                    child.localPosition.z / spacing
                );
            }

            // 外周の障害物を除外
            if (x < 0 || x >= width ||
                y < 0 || y >= height)
            {
                continue;
            }

            // 障害物がグリッド上にあるか確認
            if (tile == null)
            {
                if (Mathf.Abs(
                    child.localPosition.x - x * spacing
                    ) > 0.01f ||
                    Mathf.Abs(
                    child.localPosition.z - y * spacing
                    ) > 0.01f)
                {
                    continue;
                }
            }

            if (cells[x, y] != null)
            {
                Debug.LogError(
                    $"[FieldEventManager] Duplicate cell ({x},{y})"
                );

                return false;
            }

            cells[x, y] = child;
            foundCount++;
        }

        if (foundCount != width * height)
        {
            Debug.LogError(
                $"[FieldEventManager] Missing cells: " +
                $"{foundCount}/{width * height}"
            );

            return false;
        }

        // ========================================
        // 回転先の座標計算
        // ========================================

        int regionWidth =
            splitX ? width / 2 : width;

        int regionHeight =
            splitY ? height / 2 : height;

        List<CellMove> moves =
            new List<CellMove>(foundCount);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Transform cell = cells[x, y];

                int regionX =
                    (x / regionWidth) * regionWidth;

                int regionY =
                    (y / regionHeight) * regionHeight;

                int targetX;
                int targetY;

                if (type == FieldRotationType.FourCycle)
                {
                    // 4領域を時計回りに移動
                    Vector2Int target = GetFourCycleTarget(x, y, width, height);

                    targetX = target.x;
                    targetY = target.y;
                }
                else
                {
                    // 既存の180度回転
                    targetX = regionX + regionWidth - 1 - (x - regionX);

                    targetY = regionY + regionHeight - 1 - (y - regionY);
                }

                FieldTile tile =
                    cell.GetComponent<FieldTile>();

                Stone stone =
                    tile != null ? tile.PutedStone : null;

                moves.Add(new CellMove
                {
                    Cell = cell,

                    TargetLocalPosition = new Vector3(
                        targetX * spacing,
                        cell.localPosition.y,
                        targetY * spacing
                    ),

                    TargetCoordinate =
                        new Vector2Int(targetX, targetY),

                    Tile = tile,
                    Stone = stone,

                    StoneOffsetWorld =
                        stone != null
                        ? stone.transform.position -
                          cell.position
                        : Vector3.zero,

                    StoneRotationWorld =
                        stone != null
                        ? stone.transform.rotation
                        : Quaternion.identity
                });
            }
        }

        // ========================================
        // 回転実行
        // ========================================

        IsRotating = true;

        try
        {
            Quaternion rotation =
                Quaternion.Euler(0, 180, 0);

            foreach (CellMove move in moves)
            {
                // マスを移動
                move.Cell.localPosition =
                    move.TargetLocalPosition;

                // FourCycle以外は180度回転
                if (type != FieldRotationType.FourCycle)
                {
                    move.Cell.localRotation =
                        move.Cell.localRotation * rotation;
                }

                // コマの移動
                if (move.Stone != null)
                {
                    move.Stone.transform.position =
                        move.Cell.position +
                        move.StoneOffsetWorld;

                    // FourCycleはコマの向きも維持
                    if (type != FieldRotationType.FourCycle)
                    {
                        move.Stone.transform.rotation =
                            rotation * move.StoneRotationWorld;
                    }
                }

                if (move.Tile != null)
                {
                    move.Tile.SetCoodinate(
                        move.TargetCoordinate.x,
                        move.TargetCoordinate.y
                    );
                }
            }

            // FieldManagerの配列更新
            FieldMaker maker = fieldManager.GetComponent<FieldMaker>();

            if (maker == null)
            {
                Debug.LogError("FieldMakerが見つかりません。", this);
                return false;
            }

            maker.RegisterFieldManager();

            // 回転後の並び判定
            if (rebuildMatches)
            {
                RebuildMatchStorage();
            }

            Debug.Log(
                $"[FieldEventManager] Rotate completed: {type}"
            );

            return true;
        }
        finally
        {
            IsRotating = false;
        }
    }

    private bool TryPrepareRotationAnimation(FieldRotationType type, out List<RotationAnimationData> animationData)
    {
        animationData = new List<RotationAnimationData>();

        if (!FindFieldReference())
        {
            return false;
        }

        if (fieldManager.IsRemoveing)
        {
            Debug.LogWarning("ランダム削除中は回転できません。");
            return false;
        }

        Vector2Int size = fieldManager.GetFieldSize();

        int width = size.x;
        int height = size.y;

        bool splitX =
            type == FieldRotationType.LeftRight ||
            type == FieldRotationType.FourSections ||
            type == FieldRotationType.FourCycle;

        bool splitY =
            type == FieldRotationType.TopBottom ||
            type == FieldRotationType.FourSections ||
            type == FieldRotationType.FourCycle;

        if (width <= 0 || height <= 0 ||
            (splitX && width % 2 != 0) ||
            (splitY && height % 2 != 0))
        {
            Debug.LogError("回転できないフィールドサイズです。");
            return false;
        }

        if (generatedRoot == null || Mathf.Abs(fieldSpacing) < 0.0001f)
        {
            return false;
        }

        int regionWidth = splitX ? width / 2 : width;
        int regionHeight = splitY ? height / 2 : height;

        // 重複・不足を検出するための配列
        bool[,] occupied = new bool[width, height];
        int cellCount = 0;

        foreach (Transform child in generatedRoot)
        {
            FieldTile tile = child.GetComponent<FieldTile>();

            if (tile == null && !child.name.StartsWith("Barrier_"))
            {
                continue;
            }

            int x;
            int y;

            if (tile != null)
            {
                Vector2Int coordinate = tile.GetCoordinate();
                x = coordinate.x;
                y = coordinate.y;
            }
            else
            {
                x = Mathf.RoundToInt(
                    child.localPosition.x / fieldSpacing
                );

                y = Mathf.RoundToInt(
                    child.localPosition.z / fieldSpacing
                );

                // 外周の壁は除外する
                if (Mathf.Abs(child.localPosition.x - x * fieldSpacing) > 0.01f ||
                    Mathf.Abs(child.localPosition.z - y * fieldSpacing) > 0.01f)
                {
                    continue;
                }
            }

            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                continue;
            }

            if (occupied[x, y])
            {
                Debug.LogError($"セルが重複しています: ({x}, {y})");
                return false;
            }

            occupied[x, y] = true;
            cellCount++;

            // 所属する領域の左下座標
            int regionX = (x / regionWidth) * regionWidth;
            int regionY = (y / regionHeight) * regionHeight;

            // 各領域の回転中心
            float centerX =
                (regionX + (regionWidth - 1) * 0.5f) * fieldSpacing;

            float centerY =
                (regionY + (regionHeight - 1) * 0.5f) * fieldSpacing;

            Stone stone = tile != null ? tile.PutedStone : null;

            Vector3 targetPosition = child.localPosition;

            if (type == FieldRotationType.FourCycle)
            {
                Vector2Int target = GetFourCycleTarget(
                    x, y, width, height
                );

                targetPosition += new Vector3(
                    (target.x - x) * fieldSpacing,
                    0.0f,
                    (target.y - y) * fieldSpacing
                );
            }

            animationData.Add(new RotationAnimationData
            {
                Cell = child,

                StartLocalPosition = child.localPosition,
                StartLocalRotation = child.localRotation,

                PivotLocalPosition = new Vector3(
                    centerX,
                    0.0f,
                    centerY
                ),

                Stone = stone,

                StoneStartRootPosition =
                    stone != null
                    ? generatedRoot.InverseTransformPoint(
                        stone.transform.position)
                    : Vector3.zero,

                StoneStartWorldRotation =
                    stone != null
                    ? stone.transform.rotation
                    : Quaternion.identity,

                TargetLocalPosition = targetPosition
            });
        }

        if (cellCount != width * height)
        {
            Debug.LogError(
                $"回転対象のセルが不足しています: " +
                $"{cellCount}/{width * height}"
            );

            return false;
        }

        return true;
    }

    private IEnumerator AnimateRotation(FieldRotationType type)
    {
        // Randomの場合は最初に回転種類を決定
        if(type == FieldRotationType.Random)
{
            type = GetRandomRotationType();
        }

        if (IsRotating)
        {
            yield break;
        }

        if (!TryPrepareRotationAnimation(type, out var data))
        {
            yield break;
        }

        IsRotating = true;

        SetRotationHolo(data, true);

        float elapsed = 0.0f;

        // アニメーション本体

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / rotationDuration
            );

            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            Quaternion rotation = Quaternion.Euler(
                0f, 180f * smoothT, 0f
            );

            Quaternion worldRotation =
                generatedRoot.rotation *
                rotation *
                Quaternion.Inverse(generatedRoot.rotation);

            foreach (RotationAnimationData item in data)
            {
                if (type == FieldRotationType.FourCycle)
                {
                    // 4領域の移動アニメーション
                    item.Cell.localPosition = Vector3.Lerp(
                        item.StartLocalPosition,
                        item.TargetLocalPosition,
                        smoothT
                    );

                    // コマもマスと同じ移動量で移動
                    if (item.Stone != null)
                    {
                        Vector3 movement =
                            item.Cell.localPosition -
                            item.StartLocalPosition;

                        item.Stone.transform.position =
                            generatedRoot.TransformPoint(
                                item.StoneStartRootPosition + movement
                            );

                        // コマの向きは変更しない
                    }
                }
                else
                {
                    // 既存の180度回転アニメーション
                    Vector3 offset =
                        item.StartLocalPosition -
                        item.PivotLocalPosition;

                    item.Cell.localPosition =
                        item.PivotLocalPosition +
                        rotation * offset;

                    item.Cell.localRotation =
                        rotation * item.StartLocalRotation;

                    if (item.Stone != null)
                    {
                        Vector3 stoneOffset =
                            item.StoneStartRootPosition -
                            item.PivotLocalPosition;

                        Vector3 stonePosition =
                            item.PivotLocalPosition +
                            rotation * stoneOffset;

                        item.Stone.transform.position =
                            generatedRoot.TransformPoint(stonePosition);

                        item.Stone.transform.rotation =
                            worldRotation *
                            item.StoneStartWorldRotation;
                    }
                }
            }

            yield return null;
        }


        // 元の配置情報を一時的に復元する
        // 次のRotate()で既存の確定処理を利用するため
        foreach (RotationAnimationData item in data)
        {
            item.Cell.localPosition = item.StartLocalPosition;
            item.Cell.localRotation = item.StartLocalRotation;

            if (item.Stone != null)
            {
                item.Stone.transform.position =
                    generatedRoot.TransformPoint(
                        item.StoneStartRootPosition
                    );

                item.Stone.transform.rotation =
                    item.StoneStartWorldRotation;
            }
        }

        // 既存のRotate()を実行できるようにする
        IsRotating = false;

        // 座標・フィールド配列・勝敗判定を確定
        bool success = Rotate(type);

        SetRotationHolo(data, false);

        if (!success)
        {
            Debug.LogError(
                $"[FieldEventManager] 回転確定に失敗: {type}"
            );

            yield break;
        }

        // フィールド回転後、コマだけ180度回転
        IsRotating = true;

        if (type != FieldRotationType.FourCycle)
        {
            yield return StartCoroutine(PlayStoneRotationAnimation(data));
        }

        // コマ回転終了
        IsRotating = false;
    }

    private void SetRotationHolo(List<RotationAnimationData> data,bool holo)
    {
        foreach (var item in data)
        {
            FieldTile tile =
                item.Cell.GetComponent<FieldTile>();

            if (tile != null)
            {
                tile.SetHolo(holo);
            }

            if (item.Stone != null)
            {
                item.Stone.SetHolo(holo);
            }
        }
    }

    private IEnumerator PlayStoneRotationAnimation(List<RotationAnimationData> data)
    {
        float elapsed = 0.0f;

        List<(Transform stone, Quaternion start)> rotations = new();

        foreach (RotationAnimationData item in data)
        {
            if (item.Stone == null)
            {
                continue;
            }

            rotations.Add((item.Stone.transform, item.Stone.transform.rotation));

            // 将来ここでジャンプアニメーションを再生
            // item.Stone.PlayAnimation();
        }

        // コマの回転は引き続きC#で制御
        while (elapsed < stoneRotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / stoneRotationDuration);

            float smoothT = Mathf.SmoothStep(0.0f, 1.0f, t);

            foreach (var item in rotations)
            {
                if (item.stone == null)
                {
                    continue;
                }

                item.stone.rotation = Quaternion.AngleAxis(180.0f * smoothT, Vector3.up) * item.start;
            }

            yield return null;
        }

        // 最終角度を確定
        foreach (var item in rotations)
        {
            if (item.stone != null)
            {
                item.stone.rotation = Quaternion.AngleAxis(180.0f, Vector3.up) * item.start;
            }
        }
    }

    private Vector2Int GetFourCycleTarget(int x, int y, int width, int height)
    {
        int halfWidth = width / 2;
        int halfHeight = height / 2;

        bool isLeft = x < halfWidth;
        bool isTop = y >= halfHeight;

        // 1 → 2
        if (isLeft && isTop)
        {
            return new Vector2Int(x + halfWidth, y);
        }

        // 2 → 3
        if (!isLeft && isTop)
        {
            return new Vector2Int(x, y - halfHeight);
        }

        // 3 → 4
        if (!isLeft && !isTop)
        {
            return new Vector2Int(x - halfWidth, y);
        }

        // 4 → 1
        return new Vector2Int(x, y + halfHeight);
    }

    // ========================================
    // 回転後の並び判定
    // ========================================

    private void RebuildMatchStorage()
    {
        MatchStorage storage = MatchStorage.Instance;

        if (storage == null)
        {
            Debug.LogWarning("[FieldEventManager] MatchStorage not found.");

            return;
        }

        storage.ClearMatchData();

        Vector2Int size = fieldManager.GetFieldSize();

        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int start = new Vector2Int(x, y);

                FieldTile tile = fieldManager.GetFieldTile(start);

                if (tile == null ||
                    tile.PutedStone == null)
                {
                    continue;
                }

                Player owner = tile.PutedStone.Owner;

                if (owner == null)
                {
                    continue;
                }

                foreach (Vector2Int direction in Directions)
                {
                    Vector2Int previous = start - direction;

                    // 同じラインの重複登録を防ぐ
                    if (SameOwner(previous, owner, size))
                    {
                        continue;
                    }

                    int count = 0;
                    Vector2Int cursor = start;

                    while (SameOwner(cursor, owner, size))
                    {
                        count++;
                        cursor += direction;
                    }

                    if (count >= matchCount)
                    {
                        storage.AddMatchData(count, start, direction, owner);
                    }
                }
            }
        }
    }

    private bool SameOwner(Vector2Int coordinate, Player owner, Vector2Int size)
    {
        if (coordinate.x < 0 ||
            coordinate.y < 0 ||
            coordinate.x >= size.x ||
            coordinate.y >= size.y)
        {
            return false;
        }

        FieldTile tile = fieldManager.GetFieldTile(coordinate);

        return tile != null &&
               tile.PutedStone != null &&
               tile.PutedStone.Owner == owner;
    }


    public bool HasTurnEvent(int turnCount)
    {
        for (int i = 0; i < turnEventConfigs.Count; i++)
        {
            FieldEventConfig config = turnEventConfigs[i];

            if (config == null || executedEventIndices.Contains(i))
            {
                continue;
            }

            if (config.turn == turnCount)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetTurnEvent()
    {
        executedEventIndices.Clear();
    }

    public bool StartTurnEvent(int turnCount)
    {
        if (IsEventPlaying)
        {
            return false;
        }

        List<FieldRotationType> events = new List<FieldRotationType>();

        foreach (FieldEventConfig config in turnEventConfigs)
        {
            if (config == null || config.turn != turnCount)
            {
                continue;
            }

            events.Add(config.eventType);
        }

        if (events.Count == 0)
        {
            return false;
        }

        IsEventPlaying = true;
        StartCoroutine(PlayFieldEvents(events));

        return true;
    }

    private IEnumerator PlayFieldEvents(List<FieldRotationType> events)
    {
        // =====================================
        // イベント開始演出
        // =====================================

        yield return StartCoroutine(PlayWarningAnimation());

        // =====================================
        // フィールド回転
        // =====================================

        foreach (FieldRotationType type in events)
        {
            yield return StartCoroutine(AnimateRotation(type));
        }

        // =====================================
        // イベント終了演出
        // =====================================

        yield return StartCoroutine(PlayFinishAnimation());

        // 回転前のハイライト色をすべてリセット
        fieldManager?.ResetAllTileColors();

        IsEventPlaying = false;
    }

    private IEnumerator PlayWarningAnimation()
    {
        Debug.Log("特殊イベント発生！");

        // 警告終了演出開始
        WarningEvent.Instance?.SetWarningEventActive(true);

        yield return new WaitForSeconds(warningDuration);

        // 警告演出終了
        WarningEvent.Instance?.SetWarningEventActive(false);
    }

    private IEnumerator PlayFinishAnimation()
    {
        // 終了演出開始

        yield return new WaitForSeconds(finishDelay);

        // 終了演出終了
    }

    private bool FindFieldReference()
    {
        // 既に参照が取得できている場合
        if (fieldManager != null && generatedRoot != null)
        {
            return true;
        }

        // FieldManagerを取得
        fieldManager = FindAnyObjectByType<FieldManager>();

        if (fieldManager == null)
        {
            Debug.LogError("FieldManagerが見つかりません。", this);
            return false;
        }

        // Generated Fieldsを取得
        generatedRoot = fieldManager.transform.Find("Generated Fields");

        if (generatedRoot == null)
        {
            Debug.LogError("Generated Fieldsが見つかりません。", this);
            return false;
        }

        // FieldMakerを取得
        FieldMaker fieldMaker = fieldManager.GetComponent<FieldMaker>();

        if (fieldMaker == null)
        {
            Debug.LogError("FieldMakerが見つかりません。", this);
            return false;
        }

        // フィールド間隔を取得
        fieldSpacing = fieldMaker.GetFieldSpacing();

        return true;
    }

    private FieldRotationType GetRandomRotationType()
    {
        FieldRotationType[] types =
        {
        FieldRotationType.Whole,
        FieldRotationType.LeftRight,
        FieldRotationType.TopBottom,
        FieldRotationType.FourSections,
        FieldRotationType.FourCycle
        };

        return types[Random.Range(0, types.Length)];
    }
}


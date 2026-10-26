using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MatchCount
{
    public int lineDir_x1_y0;
    public int lineDir_x0_y1;
    public int lineDir_x1_y1;
    public int lineDir_x1_yn1;
}

public class FieldManager : MonoBehaviour
{

    public static FieldManager Instance { get; private set; }

    [Header("ランダム削除")]
    [SerializeField, Min(0.001f)] private float removeInterval = 0.1f;
    [SerializeField, Min(0.001f)] private float removeStartDelay = 1.5f;

    public bool IsRemoveing { get; private set; } = false;

    private Coroutine removeCoroutine;

    private Vector2Int fieldSize = new(10, 10);
    public Vector2Int GetFieldSize() { return fieldSize; }
    private List<FieldTile> FieldTile = new List<FieldTile>();
    public int GetFieldTileCount() { return FieldTile.Count; }

    private int stoneCount = 0;

    private Vector2Int[] checkDirection = new Vector2Int[4]
    {
        new Vector2Int(1, 0), // 横方向
        new Vector2Int(0, 1), // 縦方向
        new Vector2Int(1, 1), // 斜め方向（右上）
        new Vector2Int(1, -1) // 斜め方向（右下）
    };

    // ５目生後判定用の配列、ゲーム中の盤面アクセス用
    private FieldTile[,] FieldTileArray;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
    }

    public void BeginFieldSetup(Vector2Int size)
    {

        CancelRandomRemove();

        fieldSize = size;
        FieldTile.Clear();

        FieldTileArray = new FieldTile[fieldSize.x, fieldSize.y];
    }

    public void RegisterFieldTile(Vector2Int coodinate, FieldTile piece)
    { 
        if(!IsInsideField(coodinate))
        {
            Debug.LogError($"[FieldManager] RegisterFieldTile: 範囲外の指定です Coodinate:{coodinate} / FieldSize:{fieldSize}");
            return;
        }
        FieldTile.Add(piece);
        FieldTileArray[coodinate.x, coodinate.y] = piece;
        piece.SetCoodinate(coodinate.x, coodinate.y);
    }

    public void UnregisterFieldTile(Vector2Int coodinate)
    {
        if(!IsInsideField(coodinate))
        {
            Debug.LogError($"[FieldManager] UnregisterFieldTile: 範囲外の指定です Coodinate:{coodinate} / FieldSize:{fieldSize}");
            return;
        }
        FieldTile piece = FieldTileArray[coodinate.x, coodinate.y];
        if(piece != null)
        {
            FieldTile.Remove(piece);
            FieldTileArray[coodinate.x, coodinate.y] = null;
        }
    }

    public FieldTile GetFieldTile(Vector2Int coodinate)
    {
        if(!IsInsideField(coodinate))
        {
            Debug.LogError($"[FieldManager] GetFieldTile: 範囲外の指定です Coodinate:{coodinate} / FieldSize:{fieldSize}");
            return null;
        }

        FieldTile ret = FieldTileArray[coodinate.x, coodinate.y];

        return ret;
    }

    public MatchCount CheckMatch(Vector2Int currentCoodinate)
    {
        MatchCount ret;
        ret.lineDir_x0_y1 = 0;
        ret.lineDir_x1_y0 = 0;
        ret.lineDir_x1_y1 = 0;
        ret.lineDir_x1_yn1 = 0;

        if (!IsInsideField(currentCoodinate))
        {
            Debug.LogError($"[FieldManager] CheckMatch: 範囲外の指定です Coodinate:{currentCoodinate} / FieldSize:{fieldSize}");
            return ret;
        }

        FieldTile currentTile = GetFieldTile(currentCoodinate);
        if(currentTile == null)
        {
            Debug.LogError($"[FieldManager] CheckMatch: 指定座標にFieldTileが存在しません Coodinate:{currentCoodinate}");
            return ret;
        }

        Stone currentStone = currentTile.PutedStone;
        
        if(currentStone == null)
        {
            Debug.LogError($"[FieldManager] CheckMatch: 指定座標に石が存在しません Coodinate:{currentCoodinate}");
            return ret;
        }

        foreach (Vector2Int num in checkDirection)
        {
            int LineCount = 1;

            // 正方向と逆方向の両方をチェック
            LineCount += CountMatchingStones(currentCoodinate, num, currentStone);
            LineCount += CountMatchingStones(currentCoodinate, -num, currentStone);

            if(num == new Vector2Int(1, 0))
            {
                // 横方向のライン数を格納
                ret.lineDir_x1_y0 = LineCount;
            }
            else if(num == new Vector2Int(0, 1))
            {
                // 縦方向のライン数を格納
                ret.lineDir_x0_y1 = LineCount;
            }
            else if(num == new Vector2Int(1, 1))
            {
                // 斜め方向（右上）のライン数を格納
                ret.lineDir_x1_y1 = LineCount;
            }
            else if(num == new Vector2Int(1, -1))
            {
                // 斜め方向（右下）のライン数を格納
                ret.lineDir_x1_yn1 = LineCount;
            }
        }
        return ret;
    }

    private int CountMatchingStones(Vector2Int startCoodinate, Vector2Int direction, Stone currentStone)
    {
        int count = 0;
        Vector2Int checkCoodinate = startCoodinate + direction;
        while (IsInsideField(checkCoodinate))
        {
            FieldTile checkTile = GetFieldTile(checkCoodinate);
            if (checkTile == null || checkTile.PutedStone == null || checkTile.PutedStone.Owner != currentStone.Owner)
            {
                break;
            }
            count++;
            checkCoodinate += direction;
        }
        return count;
    }

    private bool IsInsideField(Vector2Int coodinate)
    {
        return coodinate.x >= 0 && coodinate.x < fieldSize.x && coodinate.y >= 0 && coodinate.y < fieldSize.y;
    }

    public Vector3 GetCenter()
    {
        return new Vector3((fieldSize.x - 1) * 0.5f, 0.0f, (fieldSize.y - 1) * 0.5f);
    }

    public void HighlightAllStoneColor()
    {
        foreach (FieldTile tile in FieldTile)
        {
            if (tile.PutedStone != null)
            {
                tile.PutedStone.RevealColor(tile.PutedStone.Owner.TeamColor);
            }
        }
    }

    // 石の乗っているフィールドを返す関数
    public List<FieldTile> GetAllStoneField()
    {
        List<FieldTile> stoneFields = new List<FieldTile>();
        foreach (FieldTile tile in FieldTile)
        {
            if (tile.PutedStone != null)
            {
                stoneFields.Add(tile);
            }
        }
        return stoneFields;
    }

    public void AddStoneCount()
    {
        stoneCount++;
    }

    public void SubtractStoneCount()
    {
        stoneCount--;
    }

    public int GetStoneCount()
    {
        return stoneCount;
    }

    public void RandomRemove(int count)
    {
        if(!isActiveAndEnabled || IsRemoveing || stoneCount <= 0)
        {
            return;
        }

        List<FieldTile> candiData = GetAllStoneField();

        count = Mathf.Min(count, candiData.Count);

        if (count <= 0)
        {
            return;
        }

        int[] removeIndex = new int[count];
        List<FieldTile> stoneFields = GetAllStoneField();

        if(stoneFields.Count <= 0)
        {
            return;
        }

        var target = new List<(FieldTile tile, Stone stone)>();

        for(int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, stoneFields.Count);
            FieldTile tile = candiData[randomIndex];

            target.Add((tile, tile.PutedStone));

            candiData.RemoveAt(randomIndex);
        }

        IsRemoveing = true;
        removeCoroutine = StartCoroutine(RemoveStoneRoutine(target));
    }

    private IEnumerator RemoveStoneRoutine(List<(FieldTile tile, Stone stone)> target)
    {
        yield return null;
        
        if(removeStartDelay > 0.0f)
        {
            yield return new WaitForSeconds(removeStartDelay);
        }

        var wait = new WaitForSeconds(Mathf.Max(removeInterval, 0.001f));

        for(int i = 0; i<target.Count; i++)
        {
            var targetObj = target[i];
            if(targetObj.tile != null && targetObj.stone != null && targetObj.tile.PutedStone == targetObj.stone)
            {
                targetObj.tile.RemoveStone();
            }

            if(i < target.Count - 1)
            {
                yield return wait;
            }
        }

        IsRemoveing = false;
        removeCoroutine = null;

    }

    private void CancelRandomRemove()
    {
        if(removeCoroutine != null)
        {
            StopCoroutine(removeCoroutine);
            removeCoroutine = null;
        }

        IsRemoveing = false;
    }

    private void OnDisable()
    {
        CancelRandomRemove();
    }

    public List<FieldTile> GetRandomOnStoneTile(int count)
    {
        List<FieldTile> stoneFields = GetAllStoneField();
        List<FieldTile> randomTiles = new List<FieldTile>();

        if(stoneFields.Count <= 0)
        {
            return randomTiles;
        }

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, stoneFields.Count);
            randomTiles.Add(stoneFields[randomIndex]);
            stoneFields.RemoveAt(randomIndex);
        }

        return randomTiles;
    }

    //-----------------------------------------------------------

    //リセット
    public void ResetField()
    {
        CancelRandomRemove();

        foreach (FieldTile tile in FieldTile)
        {
            if (tile == null)
            {
                continue;
            }

            tile.RemoveStone();
            tile.ResetTileColor();
            stoneCount = 0;
        }
    }
    //-----------------------------------------------------------
}

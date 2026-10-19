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
    [Header("空駒の閾値 x = 駒ありタイルのmin y = 駒ありタイルのmax z = 数")]
    [SerializeField] private List<Vector3Int> voidStoneTurnThreshold = new List<Vector3Int> { new Vector3Int(1, 3, 0) };

    public static FieldManager Instance { get; private set; }

    private Vector2Int fieldSize = new(10, 10);
    private List<FieldTile> FieldTile = new List<FieldTile>();

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
            if (checkTile == null || checkTile.PutedStone == null || checkTile.PutedStone.GetOwner() != currentStone.GetOwner())
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
                tile.PutedStone.RevealColor(tile.PutedStone.GetOwner().TeamColor);
            }
        }
    }

    public List<FieldTile> SearchOnStoneTile()
    {
        List<FieldTile> result = new List<FieldTile>();
        foreach (FieldTile tile in FieldTile)
        {
            if (tile.PutedStone != null)
            {
                //tile.ShowColor();
                result.Add(tile);
            }
        }

        return result;
    }

    public void SetVoidStone()
    {
        Debug.Log($"[FieldManager] SetVoidStone: ターン数に応じて空駒化します");
        List<FieldTile> onStoneTile = SearchOnStoneTile();

        foreach (Vector3Int threshold in voidStoneTurnThreshold)
        {
            if (onStoneTile.Count >= threshold.x && onStoneTile.Count <= threshold.y)
            {
                int voidStoneCount = threshold.z;

                int[] index = new int[onStoneTile.Count];

                for (int i = 0; i < voidStoneCount; i++)
                {
                    index[i] = Random.Range(0, onStoneTile.Count);

                    // 重複チェック
                    for (int j = 0; j < i; j++)
                    {
                        if (index[i] == index[j])
                        {
                            i--; // 重複があった場合は再度ランダムに選び直す
                            break;
                        }
                    }
                }

                for (int i = 0; i < voidStoneCount; i++)
                {
                    FieldTile tile = onStoneTile[index[i]];
                    if (tile.PutedStone != null)
                    {
                        tile.PutedStone.SetVoid();
                    }
                }
            }
        }
    }

    public void DecreaseVoidStoneTurn()
    {
        List<FieldTile> onStoneTile = SearchOnStoneTile();

        foreach (FieldTile tile in onStoneTile)
        {
            if (tile.PutedStone != null)
            {
                tile.PutedStone.DecreaseVoid();
            }
        }
    }

    //-----------------------------------------------------------

    //リセット
    public void ResetField()
    {
        foreach (FieldTile tile in FieldTile)
        {
            if (tile == null)
            {
                continue;
            }

            tile.RemoveStone();
            tile.ResetTileColor();
        }
    }
    //-----------------------------------------------------------
}

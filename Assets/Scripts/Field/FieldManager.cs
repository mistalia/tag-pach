using System.Collections.Generic;
using Unity.VisualScripting;
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

            tile.ClearStone();
            tile.ResetTileColor();
        }
    }
    //-----------------------------------------------------------
}
